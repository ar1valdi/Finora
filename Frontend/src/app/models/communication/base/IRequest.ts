export enum MessageType {
    COMMAND = 1,
    QUERY = 2
}

export interface IRequest {
    messageType: MessageType;
    messageId?: string;
    type: string;
    userId?: string;
    data: unknown;
}

