import { Injectable } from '@angular/core';
import { RabbitMqService } from './rabbit-mq.service';
import { AddUserRequest } from '../models/communication/users/addUser.request';
import { MessageEnvelope, MessageType } from '../models/communication/base/MessageEnvelope';
import { GetAllUsersRequest } from '../models/communication/users/getAllUsers.request';
import { GetUserRequest } from '../models/communication/users/getUser.request';
import { DeleteUserRequest } from '../models/communication/users/deleteUser.request';
import { Paginated } from '../models/wrappers/paginated';
import { User } from '../models/users/user.model';

@Injectable({
  providedIn: 'root'
})
export class UsersService {

  constructor(private readonly rabbitMqService: RabbitMqService) { }

  public addUser(user: AddUserRequest) {
    const message: MessageEnvelope = {
      messageType: MessageType.COMMAND,
      type: 'AddUser',
      data: user
    };

    return this.rabbitMqService.send(message);
  }

  public getAllUsers(page: number, pageSize: number) : Promise<Paginated<User>> {
    const request: GetAllUsersRequest = { page, pageSize };

    const message: MessageEnvelope = {
      messageType: MessageType.QUERY,
      type: 'GetAllUsers',
      data: request
    };

    return this.rabbitMqService.send(message);
  }

  public getUser(id: string) {
    const request: GetUserRequest = { id };

    const message: MessageEnvelope = {
      messageType: MessageType.QUERY,
      type: 'GetUser',
      data: request
    };

    return this.rabbitMqService.send(message);
  }

  public deleteUser(id: string) {
    const request: DeleteUserRequest = { id };

    const message: MessageEnvelope = {
      messageType: MessageType.COMMAND,
      type: 'DeleteUser',
      data: request
    };

    return this.rabbitMqService.send(message);
  }
  
}
