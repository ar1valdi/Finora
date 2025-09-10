import { IRequest, MessageType } from '../base/IRequest';

export interface HealthCheckRequest extends IRequest {
  component: string;
}

export function createHealthCheckRequest(component: string = 'system'): HealthCheckRequest {
  return {
    messageType: MessageType.QUERY,
    type: 'HealthCheck',
    component: component,
    messageId: '',
    userId: undefined,
    data: {
      component: component
    }
  };
}
