import { Component } from '@angular/core';
import { RouterOutlet } from '@angular/router';
import { TopNavMenuComponent } from "./common/top-nav-menu/top-nav-menu.component";
import { NotificationComponent } from "./components/notification/notification.component";

@Component({
  selector: 'app-root',
  standalone: true,
  imports: [RouterOutlet, TopNavMenuComponent, NotificationComponent],
  templateUrl: './app.component.html',
  styleUrl: './app.component.scss'
})
export class AppComponent { }