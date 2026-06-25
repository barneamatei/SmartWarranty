import { HttpClient } from '@angular/common/http';
import { Injectable, inject } from '@angular/core';

import { apiConfig } from '../../../core/config/api.config';
import { FamilyShareRecord, SubscriptionRecord, UserRecord } from '../models/account.models';

@Injectable({ providedIn: 'root' })
export class AccountApiService {
  private readonly http = inject(HttpClient);
  private readonly baseUrl = apiConfig.gatewayBaseUrl;

  getUserById(userId: string) {
    return this.http.get<UserRecord>(`${this.baseUrl}/users/api/user/${userId}`);
  }

  createUser(payload: {
    userId: string;
    email: string;
    name: string;
    phone?: string | null;
    language?: string | null;
    preferences?: string | null;
  }) {
    return this.http.post<UserRecord>(`${this.baseUrl}/users/api/user`, payload);
  }

  updateUser(
    userId: string,
    payload: {
      email: string;
      name: string;
      phone?: string | null;
      language?: string | null;
      preferences?: string | null;
    }
  ) {
    return this.http.put<UserRecord>(`${this.baseUrl}/users/api/user/${userId}`, payload);
  }

  getAllUsers() {
    return this.http.get<UserRecord[]>(`${this.baseUrl}/users/api/user`);
  }

  getSubscriptionByUserId(userId: string) {
    return this.http.get<SubscriptionRecord>(`${this.baseUrl}/users/api/subscription/user/${userId}`);
  }

  createSubscription(payload: { userId: string; planType: string; startDate: string; endDate: string }) {
    return this.http.post<SubscriptionRecord>(`${this.baseUrl}/users/api/subscription`, payload);
  }

  updateSubscription(subscriptionId: string, payload: { planType: string; endDate: string }) {
    return this.http.put<SubscriptionRecord>(`${this.baseUrl}/users/api/subscription/${subscriptionId}`, payload);
  }

  deleteSubscription(subscriptionId: string) {
    return this.http.delete<void>(`${this.baseUrl}/users/api/subscription/${subscriptionId}`);
  }

  getOwnedShares(userId: string) {
    return this.http.get<FamilyShareRecord[]>(`${this.baseUrl}/users/api/familyshare/owner/${userId}`);
  }

  getMemberShares(userId: string) {
    return this.http.get<FamilyShareRecord[]>(`${this.baseUrl}/users/api/familyshare/member/${userId}`);
  }

  createFamilyShare(payload: { ownerUserId: string; memberUserId: string; permissions: number }) {
    return this.http.post<FamilyShareRecord>(`${this.baseUrl}/users/api/familyshare`, payload);
  }

  updateFamilyShare(shareId: string, payload: { permissions: number }) {
    return this.http.put<FamilyShareRecord>(`${this.baseUrl}/users/api/familyshare/${shareId}`, payload);
  }

  deleteFamilyShare(shareId: string) {
    return this.http.delete<void>(`${this.baseUrl}/users/api/familyshare/${shareId}`);
  }
}
