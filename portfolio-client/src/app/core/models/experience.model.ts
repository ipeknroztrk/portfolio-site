export interface Experience {
    id?: number;
    company: string;
    position: string;
    description: string;
    startDate: Date;
    endDate?: Date;
    isCurrent: boolean;
    orderIndex: number;
  }