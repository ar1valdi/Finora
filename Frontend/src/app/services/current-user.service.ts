import { Injectable, Signal, signal, WritableSignal } from '@angular/core';
import { User } from '../models/users/user.model';

@Injectable({
  providedIn: 'root'
})
export class CurrentUserService {

  private currentUser: WritableSignal<User | null> = signal(null);
  /*({
    id: '8834f3bf-afae-472f-865d-c6a255960183',
    firstName: 'nowy',
    secondName: 'Testowy',
    lastName: 'Admin',
    email: 'jkaczerski@gmail.com',
    dateOfBirth: '1111-11-11',
    isDeleted: false,
    bankAccountId: '8ebda597-9f0a-405e-aaf6-fbac23c66078'
  });*/
  

  constructor() { }

  getCurrentUserSignal(): Signal<User | null> {
    return this.currentUser;
  }

  getCurrentUser(): User | null {
    return this.currentUser();
  }

  setCurrentUser(user: User | null) {
    console.log('Setting current user:', user);
    this.currentUser.set(user);
  }
}
