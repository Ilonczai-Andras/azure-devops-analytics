//1
export interface WorkItemCount {
  total: number;
  byType: Record<string, number>; 
}

//2
export interface SprintWorkItemChange {
  workItemId: number;
  changeType: 'RemovedFromSprint' | 'AddedToSprint' | string;
  sprintName: string;
  oldSprintPath: string;
  newSprintPath: string;
  date: string;
}
export type SprintChangeLog = SprintWorkItemChange[];

//3
export interface WorkItem {
  Id: number;
  Title: string;
  State: string;
  Reason: string;
  CreatedDate: string;
  AssignedTo: string;
  Description: string | null;
  History: string | null;
}

export interface WorkItemsResponse {
  workItems: WorkItem[];
}