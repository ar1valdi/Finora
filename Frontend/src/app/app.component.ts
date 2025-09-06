import { Component } from '@angular/core';
import { RouterOutlet } from '@angular/router';
import { RabbitMqService } from './services/rabbit-mq.service';
import { MessageEnvelope, MessageType } from './models/communication/base/MessageEnvelope';

@Component({
  selector: 'app-root',
  standalone: true,
  imports: [RouterOutlet],
  templateUrl: './app.component.html',
  styleUrl: './app.component.scss'
})
export class AppComponent {
  title = 'Finora';

  constructor(private rabbitMqService: RabbitMqService) {}

  testSendCommand() {
    const message = {
      correlationId: '00000000-0000-0000-0000-000000000001',
      text: 'Simple command!',
      messageType: MessageType.COMMAND,
      messageId: '',
      type: 'TestCommand',
      data: 'Simple command!'
    } as MessageEnvelope;

    this.rabbitMqService.send(message).then((response) => {
      console.log(response);
    });
  }

  testSendCommandReply() {
    this.rabbitMqService.testReply('00000000-0000-0000-0000-000000000001');
  }


  testSendQuery() {
    const message = {
      correlationId: '00000000-0000-0000-0000-000000000002',
      text: 'Simple query!',
      messageType: MessageType.QUERY,
      messageId: '',
      type: 'TestQuery',
      data: 'Simple query!'
    } as MessageEnvelope;

    this.rabbitMqService.send(message).then((response) => {
      console.log(response);
    });
  }

  testSendQueryReply() {
    this.rabbitMqService.testReply('00000000-0000-0000-0000-000000000002');
  }
}