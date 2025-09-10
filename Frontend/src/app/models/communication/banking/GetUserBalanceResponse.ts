export interface GetUserBalanceResponse {
  userId: string;
  bankAccountId: string;
  balance: number;
  isClosed: boolean;
}
