import { Injectable } from '@angular/core';
import { RabbitMqService } from './rabbit-mq.service';
import { IRequest, MessageType } from '../models/communication/base/IRequest';
import { 
  TransferMoneyRequest, 
  GetUserTransactionsRequest, 
  GetUserTransactionsResponse,
  DepositWithdrawlRequest 
} from '../models/communication/banking';

@Injectable({
  providedIn: 'root'
})
export class BankingService {

  constructor(private readonly rabbitMqService: RabbitMqService) { }

  public transferMoney(request: TransferMoneyRequest): Promise<any> {
    const message: IRequest = {
      messageType: MessageType.COMMAND,
      type: 'TransferMoney',
      data: request
    };

    return this.rabbitMqService.send(message);
  }

  public getUserTransactions(userId: string, page: number, pageSize: number): Promise<GetUserTransactionsResponse> {
    const request: GetUserTransactionsRequest = { userId, page, pageSize };

    const message: IRequest = {
      messageType: MessageType.QUERY,
      type: 'GetUserTransactions',
      data: request
    };

    return this.rabbitMqService.send(message);
  }

  public depositWithdrawl(request: DepositWithdrawlRequest): Promise<any> {
    const message: IRequest = {
      messageType: MessageType.COMMAND,
      type: 'DepositWithdrawl',
      data: request
    };

    return this.rabbitMqService.send(message);
  }
}
