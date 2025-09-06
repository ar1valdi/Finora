export interface QueueError {
    correlationId: string;
    messageType: string;
    error: string;
}