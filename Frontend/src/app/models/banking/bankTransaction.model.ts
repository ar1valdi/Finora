export interface BankTransaction {
  id: string;
  fromBankAccountId: string;
  toBankAccountId: string;
  amount: number;
  transactionDate: string;
  description: string;
}

export interface UserTransactionDTO {
  id: string;
  fromBankAccountId: string;
  toBankAccountId: string;
  amount: number;
  transactionDate: string;
  description: string;
}
