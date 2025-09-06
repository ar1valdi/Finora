export enum MessageType {
    COMMAND = 1,
    QUERY = 2
}

export interface MessageEnvelope {
    messageType: MessageType;
    correlationId?: string;
    messageId?: string;
    type: string;
    data: unknown;
}
