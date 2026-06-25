export interface UserProfileRecord {
  userId: string;
  name: string;
  phone?: string | null;
  language?: string | null;
  preferences?: string | null;
}

export interface SubscriptionRecord {
  subscriptionId: string;
  userId: string;
  planType: string;
  startDate: string;
  endDate: string;
  isPremium: boolean;
}

export interface UserRecord {
  userId: string;
  email: string;
  status: string;
  userProfile?: UserProfileRecord | null;
  subscription?: SubscriptionRecord | null;
}

export interface FamilyShareRecord {
  shareId: string;
  ownerUserId: string;
  memberUserId: string;
  permissions: number;
}
