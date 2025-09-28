// Backend PagedResponse
export type Paged<T> = { total: number; page: number; pageSize: number; items: T[] };

// === Apps ===
export type AppDto = { id: string; code: string; name: string; isEnabled: boolean; createdDate?: string };
export type AppCreate = { code: string; name: string; isEnabled?: boolean };
export type AppUpdate = Partial<AppCreate>;

// === Plans === (zaten kullanıyorsun)
export type PlanDto = { id: string; code: string; name: string; description?: string|null; currency: number; price: number; billingPeriod: number; isPublic: boolean; trialDays: number; };
export type PlanCreate = Omit<PlanDto, "id">;
export type PlanUpdate = Partial<PlanCreate>;

// === Features ===
export type FeatureDto = { id: string; appId: string; code: string; name: string; description?: string|null };
export type FeatureCreate = Omit<FeatureDto, "id">;
export type FeatureUpdate = Partial<FeatureCreate>;

// === Orgs ===
export type OrganizationDto = { id: string; name: string; slug?: string|null; createdDate?: string };
export type OrganizationCreate = { name: string; slug?: string|null };
export type OrganizationUpdate = Partial<OrganizationCreate>;

// === Customers ===
export type CustomerDto = { id: string; organizationId: string; name: string; email: string; taxNumber?: string|null; billingAddress?: string|null; country?: string|null; city?: string|null; createdDate?: string };
export type CustomerCreate = Omit<CustomerDto,"id"|"createdDate">;
export type CustomerUpdate = Partial<CustomerCreate>;

// === Subscriptions ===
export type SubscriptionItemDto = { id: string; featureId?: string|null; quantity: number; unitPrice: number; currency: number };
export type SubscriptionDto = {
  id: string; organizationId: string; appId: string; planId: string; customerId?: string|null;
  subscriptionState: number; startsAt: string; trialEndsAt?: string|null; endsAt?: string|null;
  currentPeriodStart?: string|null; currentPeriodEnd?: string|null; cancelAtPeriodEnd: boolean;
  items: SubscriptionItemDto[];
};
export type SubscriptionCreate = Omit<SubscriptionDto,"id"|"subscriptionState"|"items"> & { items?: Omit<SubscriptionItemDto,"id">[] };
export type SubscriptionUpdate = Partial<SubscriptionCreate>;

// === Usage ===
export type UsageRecordDto = { id: string; subscriptionId: string; featureId: string; periodUnit: number; periodStart: string; periodEnd: string; usedValue: number };
export type UsageRecordCreate = Omit<UsageRecordDto,"id">;

// === Invoices ===
export type InvoiceLineDto = { id: string; description: string; quantity: number; unitPrice: number; lineTotal: number; subscriptionId?: string|null; featureId?: string|null };
export type InvoiceDto = {
  id: string; organizationId: string; customerId?: string|null; invoiceNumber: string;
  invoiceState: number; currency: number; subtotal: number; taxTotal: number; grandTotal: number;
  dueDate?: string|null; paidAt?: string|null; provider: number; lines: InvoiceLineDto[];
};
export type InvoiceCreate = Omit<InvoiceDto,"id"|"invoiceNumber"|"subtotal"|"taxTotal"|"grandTotal"|"lines"> & { lines?: Omit<InvoiceLineDto,"id"|"lineTotal">[] };

// === Payments ===
export type PaymentDto = { id: string; invoiceId: string; provider: number; amount: number; currency: number; paidAt: string };
export type PaymentCreate = Omit<PaymentDto,"id">;

// === Api Keys === (sayfan hazır)
export type ApiKeyDto = { id:string; organizationId:string; appId:string; name:string; keyLast4:string; expiresAt?:string|null; isRevoked:boolean; createdDate:string };
export type ApiKeyCreate = { organizationId:string; appId:string; name:string; expiresAt?:string|null };
export type ApiKeyCreated = { id:string; key:string; keyLast4:string };
