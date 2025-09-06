import { Injectable } from '@angular/core';
import { RabbitMqService } from './rabbit-mq.service';
import { AddUserRequest } from '../models/communication/users/addUser.request';

@Injectable({
  providedIn: 'root'
})
export class UsersService {

  constructor(private readonly rabbitMqService: RabbitMqService) { }

  async addUser(user: AddUserRequest) {
    return this.rabbitMqService.send('users', user);
  }
}
