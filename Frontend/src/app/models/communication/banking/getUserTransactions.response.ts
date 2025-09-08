import { UserTransactionDTO } from '../../banking/bankTransaction.model';

export interface GetUserTransactionsResponse {
  total: number;
  items: UserTransactionDTO[];
}
