import { Injectable, Signal, signal, WritableSignal } from '@angular/core';
import { User } from '../models/users/user.model';

@Injectable({
  providedIn: 'root'
})
export class CurrentUserService {

  private currentUser: WritableSignal<User | null> = signal(null);

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
