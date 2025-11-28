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

//4
export interface TeamMember {
  name: string;
  capacityPerDay: number;
  workingDays: number;
  personalDaysOff: number;
  effectiveDays: number;
  hours: number;
}

export interface TeamCapacity {
  totalRealWorkHours: number;
  members: TeamMember[];
}

//5

export interface DevelopmentHoursResponse {
  totalDevelopmentHours: number;
  members: DevelopmentMember[];
}

export interface DevelopmentMember {
  name: string;
  developmentHours: number;
  workItemCount: number;
}

//6
export interface SupportResponse {
  totalEffort: number;
  totalRemaining: number;
  members: SupportMember[];
}

export interface SupportMember {
  name: string;
  effort: number;
  remainingWork: number;
}