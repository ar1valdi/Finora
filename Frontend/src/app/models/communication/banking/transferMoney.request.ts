export interface TransferMoneyRequest {
  fromBankAccountId: string;
  toBankAccountId: string;
  amount: number;
  description?: string;
}
