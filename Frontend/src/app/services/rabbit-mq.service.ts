import { Injectable, signal } from '@angular/core';
import { Client, StompSubscription } from '@stomp/stompjs';
import { v4 as uuidv4 } from 'uuid';
import { IRequest, MessageType } from '../models/communication/base/IRequest';
import { IResponse } from '../models/communication/base/IResponse';
import { IMessage } from '@stomp/stompjs';
import { CurrentUserService } from './current-user.service';
import { NotificationService } from './notification.service';

@Injectable({
  providedIn: 'root'
})
export class RabbitMqService {
  private brokerURL = 'ws://localhost:15674/ws';
  private connectHeaders = {
    login: 'guest',
    passcode: 'guest'
  };

  private requestsExchange = 'finora.requests';

  private client: Client;
  private isConnected = signal(false);

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
    const client = await this.ensureConnection();
    
    if (!client.connected) {
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
        resolve(response);
      };

      const subscription = this.subscribeTemporaryReplyQueue(
        client,
        replyQueueName,
        correlationId,
        wrappedResolve,
        reject
      );

      this.publishToExchange(client, routingKey, correlationId, replyQueueName, message);

      const timer = setTimeout(() => {
        try { subscription.unsubscribe(); } catch {}
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
              debugger;
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
}
