export interface IResponse {
    messageId: string;
    statusCode: number;
    errors: string[];
    data: unknown;
}
