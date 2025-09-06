import { Component } from '@angular/core';
import { UsersService } from '../../services/users.service';
import { User } from '../../models/users/user.model';
import { DatePipe } from '@angular/common';
import { Paginated } from '../../models/wrappers/paginated';

@Component({
  selector: 'app-users',
  standalone: true,
  imports: [DatePipe],
  templateUrl: './users.component.html',
  styleUrl: './users.component.scss'
})
export class UsersComponent {
  users: User[] = [];

  constructor(private readonly usersService: UsersService) {}

  ngOnInit() {
    this.usersService.getAllUsers(1, 100).then((paginatedUsers: Paginated<User>) => {
      console.log('Users', paginatedUsers);
      this.users = paginatedUsers.data;
    }, (error) => {
      console.log('Error', error);
    });
  }
}
