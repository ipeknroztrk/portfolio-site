export interface Project {
    id?: number;
    title: string;
    description: string;
    imageUrl?: string;
    liveUrl?: string;
    githubUrl?: string;
    techStack: string;
    isVisible: boolean;
    orderIndex: number;
    createdAt?: Date;
  }