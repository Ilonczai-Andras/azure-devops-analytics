using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;

namespace Azure_devops_analytics.Services;

public class AzureDevOpsService
{
    private readonly HttpClient _httpClient;
    private readonly string _baseUrl;
    private readonly string _project;
    private readonly string _team;
    private readonly string _apiVersion;
    private readonly string _pat;

    public AzureDevOpsService(HttpClient httpClient, IConfiguration config)
    {
        _httpClient = httpClient;

        _baseUrl = config["AzureDevOps:BaseUrl"]?? throw new ArgumentNullException("AzureDevOps:BaseUrl not found in configuration");
        _project = config["AzureDevOps:Project"]?? throw new ArgumentNullException("AzureDevOps:Project not found in configuration");
        _team = config["AzureDevOps:Team"]?? throw new ArgumentNullException("AzureDevOps:Team not found in configuration");
        _apiVersion = config["AzureDevOps:ApiVersion"] ?? "7.1";
        _pat = config["AzureDevOps:PersonalAccessToken"]?? throw new ArgumentNullException("AzureDevOps:PersonalAccessToken not found in configuration");

        var authToken = Convert.ToBase64String(System.Text.Encoding.ASCII.GetBytes($":{_pat}"));
        _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic", authToken);
    }

    public async Task<string> GetCurrentSprintIdAsync()
    {
        var url = $"{_baseUrl}/{_project}/{_team}/_apis/work/teamsettings/iterations?$timeframe=current&api-version={_apiVersion}";
        var response = await _httpClient.GetAsync(url);
        response.EnsureSuccessStatusCode();

        var jsonString = await response.Content.ReadAsStringAsync();
        using var doc = JsonDocument.Parse(jsonString);

        var values = doc.RootElement.GetProperty("value");
        if (values.GetArrayLength() == 0)
            throw new InvalidOperationException("No current sprint found.");

        return values[0].GetProperty("id").GetString() ?? throw new InvalidOperationException("Current sprint has no ID.");
    }

    public async Task<JsonDocument> GetSprintDataAsync(string iterationId)
    {
        var url = $"{_baseUrl}/{_project}/{_team}/_apis/work/teamsettings/iterations/{iterationId}?api-version={_apiVersion}";

        var response = await _httpClient.GetAsync(url);
        response.EnsureSuccessStatusCode();

        var jsonString = await response.Content.ReadAsStringAsync();
        return JsonDocument.Parse(jsonString);
    }

    public async Task<JsonDocument> GetSprintWorkItemsAsync(string iterationId)
    {
        var url = $"{_baseUrl}/{_project}/{_team}/_apis/work/teamsettings/iterations/{iterationId}/workitems?api-version={_apiVersion}";

        var response = await _httpClient.GetAsync(url);
        response.EnsureSuccessStatusCode();

        var jsonString = await response.Content.ReadAsStringAsync();
        return JsonDocument.Parse(jsonString);
    }

    // 1. Sprint WI Count
    public async Task<JsonDocument> GetWorkItemCountByTypeAsync(string iterationId)
    {
        var workItemsDoc = await GetSprintWorkItemsAsync(iterationId);

        if (!workItemsDoc.RootElement.TryGetProperty("workItemRelations", out var relations))
            return JsonDocument.Parse("""{ "total": 0, "byType": {} }""");

        var ids = relations.EnumerateArray()
            .Select(item => item.GetProperty("target").GetProperty("id").GetInt32())
            .ToList();

        if (ids.Count == 0)
            return JsonDocument.Parse("""{ "total": 0, "byType": {} }""");

        var idsParam = string.Join(",", ids);
        var fields = "System.WorkItemType";

        var url = $"{_baseUrl}/{_project}/_apis/wit/workitems?ids={idsParam}&fields={fields}&api-version={_apiVersion}";
        var response = await _httpClient.GetAsync(url);
        response.EnsureSuccessStatusCode();

        var detailsString = await response.Content.ReadAsStringAsync();
        var detailsDoc = JsonDocument.Parse(detailsString);

        var counts = new Dictionary<string, int>();

        foreach (var item in detailsDoc.RootElement.GetProperty("value").EnumerateArray())
        {
            var fieldsEl = item.GetProperty("fields");
            string type = fieldsEl.GetProperty("System.WorkItemType").GetString() ?? "Unknown";

            if (!counts.ContainsKey(type))
                counts[type] = 0;

            counts[type]++;
        }

        var byTypeObj = new JsonObject();
        foreach (var kvp in counts)
        {
            byTypeObj[kvp.Key] = kvp.Value;
        }

        var root = new JsonObject
        {
            ["total"] = ids.Count,
            ["byType"] = byTypeObj
        };

        var json = root.ToJsonString(new JsonSerializerOptions { WriteIndented = true });
        return JsonDocument.Parse(json);
    }

    public async Task<JsonDocument> GetWorkItemUpdatesAsync(int id)
    {
        var url = $"{_baseUrl}/{_project}/{_team}/_apis/wit/workitems/{id}/updates?api-version={_apiVersion}";
        var response = await _httpClient.GetAsync(url);
        response.EnsureSuccessStatusCode();
        return JsonDocument.Parse(await response.Content.ReadAsStringAsync());
    }

    private async Task<List<int>> RunWiqlQueryAsync(string query)
    {
        var payload = new { query = query };
        var jsonPayload = JsonSerializer.Serialize(payload);
        var content = new StringContent(jsonPayload, Encoding.UTF8, "application/json");

        var url = $"{_baseUrl}/{_project}/{_team}/_apis/wit/wiql?api-version={_apiVersion}";

        var response = await _httpClient.PostAsync(url, content);

        response.EnsureSuccessStatusCode();

        var responseContent = await response.Content.ReadAsStringAsync();
        var doc = JsonDocument.Parse(responseContent);

        var ids = doc.RootElement.GetProperty("workItems")
                     .EnumerateArray()
                     .Select(x => x.GetProperty("id").GetInt32())
                     .ToList();

        return ids;
    }

    // 2. Removed from Sprint
    public async Task<JsonDocument> RemovedFromSprintWisAsync(string iterationId)
    {
        var sprintDoc = await GetSprintDataAsync(iterationId);
        var sprintPath = sprintDoc.RootElement.GetProperty("path").GetString();
        var sprintName = sprintDoc.RootElement.GetProperty("name").GetString();

        if (string.IsNullOrEmpty(sprintPath))
            throw new InvalidOperationException("Could not determine sprint path.");
        string wiqlQuery = $@"
        SELECT [System.Id]
        FROM WorkItems
        WHERE [System.IterationPath] EVER '{sprintPath}'
        AND [System.IterationPath] <> '{sprintPath}'";

        List<int> workItemIds = await RunWiqlQueryAsync(wiqlQuery);

        var removedList = new List<JsonObject>();

        foreach (var workItemId in workItemIds)
        {
            var sprintItemData = await GetWorkItemUpdatesAsync(workItemId);
            var updates = sprintItemData.RootElement.GetProperty("value").EnumerateArray();

            foreach (var update in updates)
            {
                if (update.TryGetProperty("fields", out var fields) &&
                    fields.TryGetProperty("System.IterationPath", out var iterationPathChange))
                {
                    string? oldValue = iterationPathChange.TryGetProperty("oldValue", out var oldProp) ? oldProp.GetString() : null;
                    string? newValue = iterationPathChange.TryGetProperty("newValue", out var newProp) ? newProp.GetString() : null;

                    if (!string.IsNullOrEmpty(oldValue) &&
                        oldValue == sprintPath &&
                        (string.IsNullOrEmpty(newValue) || newValue != sprintPath))
                    {
                        var revisedDate = update.TryGetProperty("revisedDate", out var revDateProp) ? revDateProp.GetString() : null;

                        var obj = new JsonObject
                        {
                            ["workItemId"] = workItemId,
                            ["changeType"] = "RemovedFromSprint",
                            ["sprintName"] = sprintName,
                            ["oldSprintPath"] = oldValue,
                            ["newSprintPath"] = newValue,
                            ["date"] = revisedDate
                        };
                        removedList.Add(obj);

                    }
                }
            }
        }

        var json = new JsonArray(removedList.ToArray()).ToJsonString(new JsonSerializerOptions { WriteIndented = true });
        return JsonDocument.Parse(json);
    }

    // 3. Created After Sprint Start
    public async Task<JsonDocument> WIsCreatedAfterStartOfSprintAsync(string iterationId)
    {
        var sprintDoc = await GetSprintDataAsync(iterationId);

        var iterationPath = sprintDoc.RootElement.GetProperty("path").GetString();
        var startDate = sprintDoc.RootElement.GetProperty("attributes").GetProperty("startDate").GetString();

        var wiql = new
        {
            query = $@"
            SELECT [System.Id], [System.Title], [System.State], [System.CreatedDate]
            FROM WorkItems
            WHERE
                [System.IterationPath] = '{iterationPath}'
                AND [System.CreatedDate] > '{startDate}'
            ORDER BY [System.CreatedDate] ASC"
        };

        var wiqlJson = JsonSerializer.Serialize(wiql);
        var content = new StringContent(wiqlJson, Encoding.UTF8, "application/json");
        var url = $"{_baseUrl}/{_project}/{_team}/_apis/wit/wiql?api-version={_apiVersion}";

        var response = await _httpClient.PostAsync(url, content);
        response.EnsureSuccessStatusCode();

        var wiqlResult = JsonDocument.Parse(await response.Content.ReadAsStringAsync());
        var workItemIds = wiqlResult.RootElement.GetProperty("workItems")
            .EnumerateArray()
            .Select(wi => wi.GetProperty("id").GetInt32())
            .ToList();

        var mainDataList = new List<Dictionary<string, object?>>();

        foreach (var id in workItemIds)
        {
            var detailDoc = await GetWorkItemUpdatesAsync(id);
            var mergedRevision = MergeAllRevisions(detailDoc);
            var mainData = ExtractMainData(mergedRevision);
            mainDataList.Add(mainData);
        }

        var json = JsonSerializer.Serialize(new { workItems = mainDataList }, new JsonSerializerOptions { WriteIndented = true });
        return JsonDocument.Parse(json);
    }

    // 4. Sprint Capacity
    public async Task<JsonDocument> GetSprintCapacityWithMembersAsync(string iterationId)
    {
        var sprintDoc = await GetSprintDataAsync(iterationId);
        var sprint = sprintDoc.RootElement;

        var startDate = sprint.GetProperty("attributes").GetProperty("startDate").GetDateTime();
        var finishDate = sprint.GetProperty("attributes").GetProperty("finishDate").GetDateTime();
        var totalWorkingDays = WorkingDaysBetween(startDate, finishDate);

        var url = $"{_baseUrl}/{_project}/{_team}/_apis/work/teamsettings/iterations/{iterationId}/capacities?api-version={_apiVersion}";
        var response = await _httpClient.GetAsync(url);
        response.EnsureSuccessStatusCode();
        var capacitiesDoc = JsonDocument.Parse(await response.Content.ReadAsStringAsync());

        var members = capacitiesDoc.RootElement.GetProperty("teamMembers");
        double totalHours = 0;
        var memberList = new List<JsonObject>();

        foreach (var member in members.EnumerateArray())
        {
            var teamMember = member.GetProperty("teamMember");
            var name = teamMember.GetProperty("displayName").GetString();

            double capacityPerDay = 0;
            var activities = member.GetProperty("activities");
            if (activities.GetArrayLength() > 0)
            {
                capacityPerDay = activities[0].GetProperty("capacityPerDay").GetDouble();
            }

            int personalDaysOff = CountDayOffs(member.GetProperty("daysOff"));
            int effectiveDays = totalWorkingDays - personalDaysOff;
            if (effectiveDays < 0) effectiveDays = 0;

            double hours = effectiveDays * capacityPerDay;
            totalHours += hours;

            memberList.Add(new JsonObject
            {
                ["name"] = name,
                ["capacityPerDay"] = capacityPerDay,
                ["workingDays"] = totalWorkingDays,
                ["personalDaysOff"] = personalDaysOff,
                ["effectiveDays"] = effectiveDays,
                ["hours"] = hours
            });
        }

        var root = new JsonObject
        {
            ["totalRealWorkHours"] = totalHours,
            ["members"] = new JsonArray(memberList.ToArray())
        };

        return JsonDocument.Parse(root.ToJsonString(new JsonSerializerOptions { WriteIndented = true }));
    }

    private async Task<JsonDocument> ExecuteWiqlAndAggregateHours(object wiqlObj, string metricName)
    {
        var wiqlJson = JsonSerializer.Serialize(wiqlObj);
        var content = new StringContent(wiqlJson, Encoding.UTF8, "application/json");
        var wiqlUrl = $"{_baseUrl}/{_project}/{_team}/_apis/wit/wiql?api-version={_apiVersion}";

        var wiqlResponse = await _httpClient.PostAsync(wiqlUrl, content);
        wiqlResponse.EnsureSuccessStatusCode();

        var wiqlResult = JsonDocument.Parse(await wiqlResponse.Content.ReadAsStringAsync());
        var workItemIds = wiqlResult.RootElement.GetProperty("workItems")
            .EnumerateArray()
            .Select(wi => wi.GetProperty("id").GetInt32())
            .ToList();

        if (workItemIds.Count == 0)
        {
            return JsonDocument.Parse($$"""{ "total{{char.ToUpper(metricName[0]) + metricName.Substring(1)}}": 0, "members": [] }""");
        }

        var idsParam = string.Join(",", workItemIds);
        var fields = "System.Title,System.AssignedTo,Microsoft.VSTS.Scheduling.Effort";
        var detailsUrl = $"{_baseUrl}/{_project}/_apis/wit/workitems?ids={idsParam}&fields={fields}&api-version={_apiVersion}";

        var detailsResponse = await _httpClient.GetAsync(detailsUrl);
        detailsResponse.EnsureSuccessStatusCode();
        var detailsDoc = JsonDocument.Parse(await detailsResponse.Content.ReadAsStringAsync());

        var hoursByMember = new Dictionary<string, double>();
        double totalHours = 0;
        var itemsByMember = new Dictionary<string, int>();

        foreach (var item in detailsDoc.RootElement.GetProperty("value").EnumerateArray())
        {
            var fieldsEl = item.GetProperty("fields");

            string memberName = "Unassigned";
            if (fieldsEl.TryGetProperty("System.AssignedTo", out var assignedToEl))
            {
                if (assignedToEl.ValueKind == JsonValueKind.Object && assignedToEl.TryGetProperty("displayName", out var dn))
                    memberName = dn.GetString() ?? "Unassigned";
                else
                    memberName = assignedToEl.GetString() ?? "Unassigned";
            }

            double effort = 0;
            if (fieldsEl.TryGetProperty("Microsoft.VSTS.Scheduling.Effort", out var eff) && eff.ValueKind == JsonValueKind.Number)
            {
                effort = eff.GetDouble();
            }

            if (!hoursByMember.ContainsKey(memberName))
            {
                hoursByMember[memberName] = 0;
                itemsByMember[memberName] = 0;
            }

            hoursByMember[memberName] += effort;
            itemsByMember[memberName] += 1;
            totalHours += effort;
        }

        var memberArr = new JsonArray();
        foreach (var kv in hoursByMember)
        {
            var obj = new JsonObject
            {
                ["name"] = kv.Key,
                [metricName] = kv.Value,
                ["workItemCount"] = itemsByMember[kv.Key]
            };

            memberArr.Add(obj);
        }

        var root = new JsonObject
        {
            [$"total{char.ToUpper(metricName[0]) + metricName.Substring(1)}"] = totalHours,
            ["members"] = memberArr
        };

        return JsonDocument.Parse(root.ToJsonString(new JsonSerializerOptions { WriteIndented = true }));
    }

    // 5. New Development Hours
    public async Task<JsonDocument> GetNewDevelopmentHoursPerMemberAsync(string iterationId)
    {
        var sprintDoc = await GetSprintDataAsync(iterationId);
        var iterationPath = sprintDoc.RootElement.GetProperty("path").GetString();

        var wiql = new
        {
            query = $@"
            SELECT [System.Id]
            FROM WorkItems
            WHERE
                [System.IterationPath] = '{iterationPath}'
                AND [System.Title] <> 'SUPPORT'
            "
        };

        return await ExecuteWiqlAndAggregateHours(wiql, "developmentHours");
    }

    // 6. Support Hours
    public async Task<JsonDocument> GetSupportHoursAsync(string iterationId)
    {
        var sprintDoc = await GetSprintDataAsync(iterationId);
        var iterationPath = sprintDoc.RootElement.GetProperty("path").GetString();

        var wiql = new
        {
            query = $@"
            SELECT [System.Id]
            FROM WorkItems
            WHERE
                [System.IterationPath] = '{iterationPath}'
                AND [System.Title] = 'SUPPORT'
            "
        };

        return await ExecuteWiqlAndAggregateHours(wiql, "supportHours");
    }

    // 7. Support Effort vs Remaining Work (Csak SUPPORT title esetén)
    public async Task<JsonDocument> GetSupportEffortVsRemainingAsync(string iterationId)
    {
        var sprintDoc = await GetSprintDataAsync(iterationId);
        var iterationPath = sprintDoc.RootElement.GetProperty("path").GetString();

        var wiql = new
        {
            query = $@"
            SELECT [System.Id]
            FROM WorkItems
            WHERE
                [System.IterationPath] = '{iterationPath}'
                AND [System.Title] = 'SUPPORT'
            "
        };

        var wiqlJson = JsonSerializer.Serialize(wiql);
        var content = new StringContent(wiqlJson, Encoding.UTF8, "application/json");
        var wiqlUrl = $"{_baseUrl}/{_project}/{_team}/_apis/wit/wiql?api-version={_apiVersion}";

        var wiqlResponse = await _httpClient.PostAsync(wiqlUrl, content);
        wiqlResponse.EnsureSuccessStatusCode();

        var wiqlResult = JsonDocument.Parse(await wiqlResponse.Content.ReadAsStringAsync());
        var workItemIds = wiqlResult.RootElement.GetProperty("workItems")
            .EnumerateArray()
            .Select(wi => wi.GetProperty("id").GetInt32())
            .ToList();

        if (workItemIds.Count == 0)
        {
            return JsonDocument.Parse("""{ "totalEffort": 0, "totalRemaining": 0, "members": [] }""");
        }

        var idsParam = string.Join(",", workItemIds);

        var fields = "System.AssignedTo,Microsoft.VSTS.Scheduling.Effort,Microsoft.VSTS.Scheduling.RemainingWork";
        var detailsUrl = $"{_baseUrl}/{_project}/_apis/wit/workitems?ids={idsParam}&fields={fields}&api-version={_apiVersion}";

        var detailsResponse = await _httpClient.GetAsync(detailsUrl);
        detailsResponse.EnsureSuccessStatusCode();
        var detailsDoc = JsonDocument.Parse(await detailsResponse.Content.ReadAsStringAsync());

        var statsByMember = new Dictionary<string, (double Effort, double Remaining)>();
        double totalEffortGlobal = 0;
        double totalRemainingGlobal = 0;

        foreach (var item in detailsDoc.RootElement.GetProperty("value").EnumerateArray())
        {
            var fieldsEl = item.GetProperty("fields");

            string memberName = "Unassigned";
            if (fieldsEl.TryGetProperty("System.AssignedTo", out var assignedToEl))
            {
                if (assignedToEl.ValueKind == JsonValueKind.Object && assignedToEl.TryGetProperty("displayName", out var dn))
                    memberName = dn.GetString() ?? "Unassigned";
                else
                    memberName = assignedToEl.GetString() ?? "Unassigned";
            }

            double effort = 0;
            if (fieldsEl.TryGetProperty("Microsoft.VSTS.Scheduling.Effort", out var eff) && eff.ValueKind == JsonValueKind.Number)
            {
                effort = eff.GetDouble();
            }

            double remaining = 0;
            if (fieldsEl.TryGetProperty("Microsoft.VSTS.Scheduling.RemainingWork", out var rem) && rem.ValueKind == JsonValueKind.Number)
            {
                remaining = rem.GetDouble();
            }

            if (!statsByMember.ContainsKey(memberName))
            {
                statsByMember[memberName] = (0, 0);
            }

            var current = statsByMember[memberName];
            statsByMember[memberName] = (current.Effort + effort, current.Remaining + remaining);

            totalEffortGlobal += effort;
            totalRemainingGlobal += remaining;
        }

        var memberArr = new JsonArray();
        foreach (var kv in statsByMember)
        {
            memberArr.Add(new JsonObject
            {
                ["name"] = kv.Key,
                ["effort"] = kv.Value.Effort,
                ["remainingWork"] = kv.Value.Remaining
            });
        }

        var root = new JsonObject
        {
            ["totalEffort"] = totalEffortGlobal,
            ["totalRemaining"] = totalRemainingGlobal,
            ["members"] = memberArr
        };

        return JsonDocument.Parse(root.ToJsonString(new JsonSerializerOptions { WriteIndented = true }));
    }

    public async Task<JsonDocument> GetAllWorkItemDataAsync(string iterationId)
    {
        var resultList = new List<JsonObject>();
        var workItemsDoc = await GetSprintWorkItemsAsync(iterationId);

        var workItemIds = workItemsDoc.RootElement.GetProperty("workItemRelations")
            .EnumerateArray()
            .Select(item => item.GetProperty("target").GetProperty("id").GetInt32())
            .ToList();

        foreach (var workItemId in workItemIds)
        {
            var sprintItemData = await GetWorkItemUpdatesAsync(workItemId);
            var jsonObj = JsonNode.Parse(sprintItemData.RootElement.GetRawText())!.AsObject();
            jsonObj["workItemId"] = workItemId;
            resultList.Add(jsonObj);
        }

        var json = new JsonArray(resultList.ToArray()).ToJsonString(new JsonSerializerOptions { WriteIndented = true });
        return JsonDocument.Parse(json);
    }

    private JsonElement MergeAllRevisions(JsonDocument workItemUpdateDoc)
    {
        var updates = workItemUpdateDoc.RootElement.GetProperty("value");

        var merged = new Dictionary<string, JsonElement>();

        foreach (var update in updates.EnumerateArray())
        {
            if (!update.TryGetProperty("fields", out var fields))
                continue;

            foreach (var field in fields.EnumerateObject())
            {
                if (field.Value.TryGetProperty("newValue", out var newVal))
                {
                    merged[field.Name] = newVal;
                }
            }
        }

        using var stream = new MemoryStream();
        using (var writer = new Utf8JsonWriter(stream))
        {
            writer.WriteStartObject();
            writer.WritePropertyName("fields");
            writer.WriteStartObject();

            foreach (var kv in merged)
            {
                writer.WritePropertyName(kv.Key);
                kv.Value.WriteTo(writer);
            }

            writer.WriteEndObject();
            writer.WriteEndObject();
        }

        return JsonDocument.Parse(stream.ToArray()).RootElement;
    }

    private Dictionary<string, object?> ExtractMainData(JsonElement mergedRevision)
    {
        var result = new Dictionary<string, object?>();

        var fields = mergedRevision.GetProperty("fields");

        object? TryGet(string fieldName)
        {
            if (!fields.TryGetProperty(fieldName, out var v))
                return null;

            return v.ValueKind switch
            {
                JsonValueKind.String => v.GetString(),
                JsonValueKind.Number => v.GetDouble(),
                JsonValueKind.True => true,
                JsonValueKind.False => false,
                JsonValueKind.Object => v.ToString(),
                _ => null
            };
        }

        result["Id"] = TryGet("System.Id");
        result["Title"] = TryGet("System.Title");
        result["State"] = TryGet("System.State");
        result["Reason"] = TryGet("System.Reason");
        result["CreatedDate"] = TryGet("System.CreatedDate");
        result["AssignedTo"] = TryGet("System.AssignedTo");
        result["Description"] = TryGet("System.Description");
        result["History"] = TryGet("System.History");

        return result;
    }

    private int WorkingDaysBetween(DateTime start, DateTime end)
    {
        int workDays = 0;

        for (var day = start.Date; day <= end.Date; day = day.AddDays(1))
        {
            if (day.DayOfWeek is >= DayOfWeek.Monday and <= DayOfWeek.Friday)
                workDays++;
        }

        return workDays;
    }
    private int CountDayOffs(JsonElement daysOffJson)
    {
        int sum = 0;

        foreach (var d in daysOffJson.EnumerateArray())
        {
            var s = d.GetProperty("start").GetDateTime();
            var e = d.GetProperty("end").GetDateTime();

            sum += WorkingDaysBetween(s, e);
        }

        return sum;
    }
}
