import { IRequest, MessageType } from '../base/IRequest';

export interface GetUserBalanceRequest extends IRequest {
  userId: string;
}

export function createGetUserBalanceRequest(userId: string): GetUserBalanceRequest {
  return {
    messageType: MessageType.QUERY,
    type: 'GetUserBalance',
    userId: userId,
    messageId: '',
    data: {
      userId: userId
    }
  };
}
