import { Component } from '@angular/core';
import { AddUserFormComponent } from '../users/add-user-form/add-user-form.component';

@Component({
  selector: 'app-register',
  standalone: true,
  imports: [AddUserFormComponent],
  templateUrl: './register.component.html',
  styleUrl: './register.component.scss'
})
export class RegisterComponent {

}
