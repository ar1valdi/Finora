import { Component } from '@angular/core';
import { FormControl, FormGroup, ReactiveFormsModule, Validators, AbstractControl, ValidationErrors } from '@angular/forms';
import { CommonModule } from '@angular/common';
import { UsersService } from '../../../services/users.service';

@Component({
  selector: 'app-add-user-form',
  standalone: true,
  imports: [ReactiveFormsModule, CommonModule],
  templateUrl: './add-user-form.component.html',
  styleUrl: './add-user-form.component.scss'
})
export class AddUserFormComponent {
  form = new FormGroup({
    firstName: new FormControl('', [Validators.required]),
    secondName: new FormControl(''),
    lastName: new FormControl('', [Validators.required]),
    email: new FormControl('', [Validators.required, Validators.email]),
    dateOfBirth: new FormControl('', [Validators.required, this.ageValidator]),
    password: new FormControl('', [Validators.required]),
  });

  constructor(private usersService: UsersService) {}
  
  onSubmit() {
    if (!this.validateForm()) {
      this.form.markAllAsTouched();
      return;
    }

    this.usersService.addUser({
      firstName: this.form.value.firstName ?? '',
      secondName: this.form.value.secondName ?? '',
      lastName: this.form.value.lastName ?? '',
      email: this.form.value.email ?? '',
      dateOfBirth: this.form.value.dateOfBirth ? new Date(this.form.value.dateOfBirth) : new Date(),
      password: this.form.value.password ?? '',
    }).then((data) => {
      console.log('User added', data);
    });
  }

  ageValidator(control: AbstractControl): ValidationErrors | null {
    if (!control.value) {
      return null;
    }

    const birthDate = new Date(control.value);
    const today = new Date();
    const age = today.getFullYear() - birthDate.getFullYear();
    const monthDiff = today.getMonth() - birthDate.getMonth();
    
    const actualAge = monthDiff < 0 || (monthDiff === 0 && today.getDate() < birthDate.getDate()) 
      ? age - 1 
      : age;

    if (actualAge < 18) {
      return { underage: { value: actualAge, required: 18 } };
    }

    return null;
  }

  validateForm() {
    return this.form.valid;
  }
}
