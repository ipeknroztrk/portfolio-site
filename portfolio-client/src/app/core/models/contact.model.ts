export interface ContactMessage {
    id?: number;
    fullName: string;
    email: string;
    message: string;
    isRead?: boolean;
    sentAt?: Date;
  }