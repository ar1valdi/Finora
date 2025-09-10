import { Injectable, signal } from '@angular/core';
import { Client, StompSubscription } from '@stomp/stompjs';
import { v4 as uuidv4 } from 'uuid';
import { IRequest, MessageType } from '../models/communication/base/IRequest';
import { IResponse } from '../models/communication/base/IResponse';
import { IMessage } from '@stomp/stompjs';
import { CurrentUserService } from './current-user.service';
import { NotificationService } from './notification.service';
import { createHealthCheckRequest } from '../models/communication/system/HealthCheckRequest';

enum CircuitState {
  CLOSED = 'CLOSED',
  OPEN = 'OPEN',
  HALF_OPEN = 'HALF_OPEN'
}

@Injectable({
  providedIn: 'root'
})
export class RabbitMqService {
  private readonly brokerURL = 'ws://localhost:15674/ws';
  private readonly connectHeaders = {
    login: 'guest',
    passcode: 'guest'
  };

  private requestsExchange = 'finora.requests';

  private client: Client;
  private isConnected = signal(false);

  private circuitState: CircuitState = CircuitState.CLOSED;
  private consecutiveFailures = 0;
  private lastFailureTime: Date | null = null;
  private readonly failureThreshold = 5;
  private readonly healthCheckIntervals = [2000, 5000, 10000];
  private currentIntervalIndex = 0;
  private healthCheckTimer: any = null;
  private isHealthCheckInProgress = false;

  private onConnect: () => void;
  private onStompError: (frame: any) => void;

  constructor(private currentUserService: CurrentUserService, private notificationService: NotificationService) {
    this.client = new Client({
      brokerURL: this.brokerURL,
      connectHeaders: this.connectHeaders,
      debug: (str: string) => {
        console.log(str);
      },
      onDisconnect: () => {
        console.log('Disconnected from RabbitMQ');
        this.isConnected.set(false);
      },
      onWebSocketError: (error: any) => {
        console.error('WebSocket error:', error);
        this.isConnected.set(false);
      }
    });

    this.onConnect = () => {
      console.log('Connected to RabbitMQ');
      this.isConnected.set(true);
    };

    this.onStompError = (frame: any) => {
      console.error('Stomp error:', frame);
      this.isConnected.set(false);
    };
  }

  private async ensureConnection(): Promise<Client> {
    if (!this.client || !this.client.connected) {
      return new Promise((resolve, reject) => {
        this.client!.onConnect = () => {
          this.onConnect();
          resolve(this.client);
        };
        this.client!.onStompError = (frame: any) => {
          this.onStompError(frame);
          reject(new Error(frame.headers['message']));
        };
        this.client!.activate();
      });
    }

    return this.client;
  }

  async send(message: IRequest, timeout: number = 10000): Promise<any> {
    if (this.circuitState === CircuitState.OPEN) {
      this.notificationService.showError('Circuit breaker is OPEN - service unavailable');
      throw new Error('Circuit breaker is OPEN - service unavailable');
    }

    const client = await this.ensureConnection();
    
    if (!client.connected) {
      this.notificationService.showError('Not connected to RabbitMQ');
      throw new Error('Not connected to RabbitMQ');
    }

    return new Promise((resolve, reject) => {
      const isCommand = message.messageType === MessageType.COMMAND;

      const correlationId = uuidv4();
      const messageId = message.messageId ? message.messageId : uuidv4();
      message.messageId = messageId;
      message.userId = this.currentUserService.getCurrentUser()?.id;

      const routingKey = isCommand ? 'command' : 'query';

      const replyQueueName = `responses.${correlationId}.${uuidv4()}`;
      
      const wrappedResolve = (response: any) => {
        clearTimeout(timer);
        this.onSuccess();
        resolve(response);
      };

      const wrappedReject = (error: any) => {
        clearTimeout(timer);
        this.onFailure();
        reject(error);
      };

      const subscription = this.subscribeTemporaryReplyQueue(
        client,
        replyQueueName,
        correlationId,
        wrappedResolve,
        wrappedReject
      );

      this.publishToExchange(client, routingKey, correlationId, replyQueueName, message);

      const timer = setTimeout(() => {
        try { subscription.unsubscribe(); } catch {}
        this.onFailure();
        this.notificationService.showError('Timeout');
        reject({
          correlationId: correlationId,
          errors: ['Timeout'],
          statusCode: 504,
          data: null,
          messageId: messageId,
        } as IResponse);
      }, timeout);
    });
  }

  private publishToExchange(
    client: Client,
    routingKey: string,
    correlationId: string,
    replyQueueName: string,
    payload: IRequest
  ) {
    client.publish({
      destination: `/exchange/${this.requestsExchange}/${routingKey}`,
      headers: {
        'correlation-id': correlationId,
        'reply-to': `${replyQueueName}`,
        'content-type': 'application/json',
      },
      body: JSON.stringify(payload)
    });
  }

  private subscribeTemporaryReplyQueue(
    client: Client,
    replyQueueName: string,
    correlationId: string,
    resolve: (response: IResponse) => void,
    reject: (error: any) => void
  ): StompSubscription {
    const subscription = client.subscribe(
      `/queue/${replyQueueName}`,
      (message: IMessage) => {
        try {
          console.log('Received message from reply queue', message);

          const response = JSON.parse(message.body);
          const msgCorrelationId = message.headers['correlation-id'] || response?.correlationId;
          
          if (msgCorrelationId === correlationId) {
            try { subscription.unsubscribe(); } catch {}
            if (response.statusCode.toString().startsWith('2')) {
              resolve(response.data as IResponse);
            } else {
              this.notificationService.showError(response.statusCode + ': ' + response.errors.join(', '));
              reject(response.errors);
            }
          }
        } catch (error) {
          reject(error);
        }
      },
      {
        'durable': 'false',
        'auto-delete': 'true',
        'exclusive': 'false'
      }
    );

    return subscription;
  }

  async testReply(corrId: string) {
    const client = await this.ensureConnection();
    if (!client.connected) {
      throw new Error('Not connected to RabbitMQ');
    }

    const replyQueueName = `responses.${corrId}`;
    client.publish({
      destination: `/queue/${replyQueueName}`,
      headers: {
        'correlation-id': corrId,
        'content-type': 'application/json'
      },
      body: JSON.stringify({
        correlationId: corrId,
        data: { text: 'Reply to message ' + corrId }
      })
    });
  }

  private onSuccess(): void {
    if (this.circuitState === CircuitState.HALF_OPEN) {
      this.circuitState = CircuitState.CLOSED;
      this.consecutiveFailures = 0;
      this.currentIntervalIndex = 0;
      this.stopHealthCheck();
    } else if (this.circuitState === CircuitState.CLOSED) {
      this.consecutiveFailures = 0;
    }
  }

  private onFailure(): void {
    this.consecutiveFailures++;
    this.lastFailureTime = new Date();

    if (this.circuitState === CircuitState.HALF_OPEN) {
      this.circuitState = CircuitState.OPEN;
      this.startHealthCheck();
    } else if (this.consecutiveFailures >= this.failureThreshold && this.circuitState === CircuitState.CLOSED) {
      this.circuitState = CircuitState.OPEN;
      this.notificationService.showError('Service temporarily unavailable - circuit breaker activated');
      this.startHealthCheck();
    }
  }

  private startHealthCheck(): void {
    if (this.healthCheckTimer) {
      clearTimeout(this.healthCheckTimer);
    }

    const interval = this.healthCheckIntervals[this.currentIntervalIndex];

    this.healthCheckTimer = setTimeout(() => {
      this.performHealthCheck();
    }, interval);

    if (this.currentIntervalIndex < this.healthCheckIntervals.length - 1) {
      this.currentIntervalIndex++;
    }
  }

  private stopHealthCheck(): void {
    if (this.healthCheckTimer) {
      clearTimeout(this.healthCheckTimer);
      this.healthCheckTimer = null;
    }
  }

  private async performHealthCheck(): Promise<void> {
    if (this.isHealthCheckInProgress) {
      return;
    }

    this.isHealthCheckInProgress = true;

    try {
      this.circuitState = CircuitState.HALF_OPEN;
      
      const healthCheckRequest = createHealthCheckRequest();
      await this.send(healthCheckRequest, 5000);
      
      this.notificationService.showSuccess('Service is available');
    } catch (error) {
      this.notificationService.showError(`Service is not available: ${error}`);
      this.startHealthCheck();
    } finally {
      this.isHealthCheckInProgress = false;
    }
  }
}
