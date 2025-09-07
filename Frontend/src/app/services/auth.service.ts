import { Injectable } from '@angular/core';
import { MessageType } from '../models/communication/base/IRequest';
import { RabbitMqService } from './rabbit-mq.service';
import { User } from '../models/users/user.model';
import { CurrentUserService } from './current-user.service';

@Injectable({
  providedIn: 'root'
})
export class AuthService {
  constructor(private rabbitMqService: RabbitMqService, private currentUserService: CurrentUserService) { }

  login(email: string, password: string): Promise<User> {
    const message = {
      data: {
        email: email,
        password: password
      },
      type: 'Login',
      messageType: MessageType.QUERY
    }

    return this.rabbitMqService.send(message).then((response: User) => {
      this.currentUserService.setCurrentUser(response);
      return response;
    });
  }
}
