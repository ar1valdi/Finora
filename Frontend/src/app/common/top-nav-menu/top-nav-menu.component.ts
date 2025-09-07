import { Component, effect } from '@angular/core';
import { RouterModule } from '@angular/router';
import { User } from '../../models/users/user.model';
import { CurrentUserService } from '../../services/current-user.service';

@Component({
  selector: 'app-top-nav-menu',
  standalone: true,
  imports: [RouterModule],
  templateUrl: './top-nav-menu.component.html',
  styleUrl: './top-nav-menu.component.scss'
})
export class TopNavMenuComponent {
  currentUser: User | null = null;
  constructor(private currentUserService: CurrentUserService) {
    effect(() => {
      this.currentUser = this.currentUserService.getCurrentUser();
      console.log(this.currentUser);
    });
  }

  logout() {
    this.currentUserService.setCurrentUser(null);
  }
}
