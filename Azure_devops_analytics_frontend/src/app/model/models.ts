export interface WorkItemCount {
  total: number;
  byType: Record<string, number>; 
}


export interface SprintWorkItemChange {
  workItemId: number;
  changeType: 'RemovedFromSprint' | 'AddedToSprint' | string;
  sprintName: string;
  oldSprintPath: string;
  newSprintPath: string;
  date: string;
}
export type SprintChangeLog = SprintWorkItemChange[];