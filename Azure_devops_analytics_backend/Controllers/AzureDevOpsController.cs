using Azure_devops_analytics.Services;
using Microsoft.AspNetCore.Mvc;

namespace Azure_devops_analytics.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AzureDevOpsController : ControllerBase
{
    private readonly AzureDevOpsService _devOpsService;

    public AzureDevOpsController(AzureDevOpsService devOpsService)
    {
        _devOpsService = devOpsService;
    }

    private async Task<string> ResolveIterationId(string? iterationId)
    {
        if (!string.IsNullOrEmpty(iterationId))
        {
            return iterationId;
        }
        return await _devOpsService.GetCurrentSprintIdAsync();
    }

    //1. sprintben indított WI-k (work item) száma
    [HttpGet("wi-count")]
    public async Task<IActionResult> GetWorkItemCount([FromQuery] string? iterationId)
    {
        var id = await ResolveIterationId(iterationId);
        var result = await _devOpsService.GetWorkItemCountByTypeAsync(id);
        return Ok(result.RootElement);
    }

    // 2. Sprint Changes (Removed)
    [HttpGet("sprint-changes")]
    public async Task<IActionResult> GetSprintChanges([FromQuery] string? iterationId)
    {
        var id = await ResolveIterationId(iterationId);
        var result = await _devOpsService.RemovedFromSprintWisAsync(id);
        return Ok(result.RootElement);
    }

    // 3. Created After Sprint Start
    [HttpGet("created-after-start")]
    public async Task<IActionResult> WIsCreatedAfterStart([FromQuery] string? iterationId)
    {
        var id = await ResolveIterationId(iterationId);
        var result = await _devOpsService.WIsCreatedAfterStartOfSprintAsync(id);
        return Ok(result.RootElement);
    }

    // 4. Sprint Capacity
    [HttpGet("sprint-capacity")]
    public async Task<IActionResult> GetSprintCapacity([FromQuery] string? iterationId)
    {
        var id = await ResolveIterationId(iterationId);
        var result = await _devOpsService.GetSprintCapacityWithMembersAsync(id);
        return Ok(result.RootElement);
    }

    // 5. New Development Hours
    [HttpGet("new-development-hours")]
    public async Task<IActionResult> GetNewDevelopmentHours([FromQuery] string? iterationId)
    {
        var id = await ResolveIterationId(iterationId);
        var result = await _devOpsService.GetNewDevelopmentHoursPerMemberAsync(id);
        return Ok(result.RootElement);
    }

    // 6. Support Hours
    [HttpGet("support-hours")]
    public async Task<IActionResult> GetSupportHours([FromQuery] string? iterationId)
    {
        var id = await ResolveIterationId(iterationId);
        var result = await _devOpsService.GetSupportHoursAsync(id);
        return Ok(result.RootElement);
    }

    // 7. Support Effort vs Remaining Work
    [HttpGet("support-effort-remaining")]
    public async Task<IActionResult> GetSupportEffortVsRemaining([FromQuery] string? iterationId)
    {
        var id = await ResolveIterationId(iterationId);
        var result = await _devOpsService.GetSupportEffortVsRemainingAsync(id);
        return Ok(result.RootElement);
    }

    // GET Specific Sprint Data (csak az adatok, nem a kalkulációk)
    [HttpGet("sprint-data")]
    public async Task<IActionResult> GetSprintData([FromQuery] string? iterationId)
    {
        var id = await ResolveIterationId(iterationId);
        var sprint = await _devOpsService.GetSprintDataAsync(id);
        return Ok(sprint.RootElement);
    }

    [HttpGet("all-work-item-data")]
    public async Task<IActionResult> GetAllWorkItemData([FromQuery] string? iterationId)
    {
        var id = await ResolveIterationId(iterationId);
        var result = await _devOpsService.GetAllWorkItemDataAsync(id);
        return Ok(result.RootElement);
    }

}

