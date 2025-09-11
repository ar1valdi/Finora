import { Component, OnInit, output } from '@angular/core';
import { FormControl, FormGroup, ReactiveFormsModule, Validators, AbstractControl, ValidationErrors } from '@angular/forms';
import { CommonModule } from '@angular/common';
import { UsersService } from '../../../services/users.service';
import { ActivatedRoute, Router } from '@angular/router';
import { User } from '../../../models/users/user.model';

@Component({
  selector: 'app-update-user-form',
  standalone: true,
  imports: [ReactiveFormsModule, CommonModule],
  templateUrl: './update-user-form.component.html',
  styleUrl: './update-user-form.component.scss'
})
export class UpdateUserFormComponent implements OnInit {
  form = new FormGroup({
    firstName: new FormControl('', [Validators.required]),
    secondName: new FormControl(''),
    lastName: new FormControl('', [Validators.required]),
    email: new FormControl('', [Validators.required, Validators.email]),
    dateOfBirth: new FormControl('', [Validators.required, this.ageValidator]),
    password: new FormControl(''),
  });

  isSubmitted = false;
  isLoading = true;
  user: User | null = null;

  constructor(private usersService: UsersService, private router: Router, private route: ActivatedRoute) {}

  ngOnInit() {
    this.loadUser();
  }

  private async loadUser() {
    try {
      this.isLoading = true;
      this.user = await this.usersService.getUser(this.route.snapshot.params['id']);
      
      if (this.user) {
        const dateOfBirth = new Date(this.user.dateOfBirth).toISOString().split('T')[0];
        
        this.form.patchValue({
          firstName: this.user.firstName,
          secondName: this.user.secondName || '',
          lastName: this.user.lastName,
          email: this.user.email,
          dateOfBirth: dateOfBirth,
          password: ''
        });
      }
    } catch (error) {
      console.error('Error loading user:', error);
    } finally {
      this.isLoading = false;
    }
  }
  
  onSubmit() {
    if (this.isSubmitted || !this.user) {
      return;
    }
    
    this.isSubmitted = true;

    if (!this.validateForm()) {
      this.form.markAllAsTouched();
      this.isSubmitted = false;
      return;
    }

    this.usersService.updateUser({
      id: this.user.id,
      firstName: this.form.value.firstName ?? '',
      secondName: this.form.value.secondName ?? '',
      lastName: this.form.value.lastName ?? '',
      email: this.form.value.email ?? '',
      dateOfBirth: this.form.value.dateOfBirth ? new Date(this.form.value.dateOfBirth) : new Date(),
      password: this.form.value.password ?? '',
    }).then((data) => {
      this.isSubmitted = false;
      this.router.navigate(['/users']);
    }).catch((error) => {
      console.error('Error updating user:', error);
      this.isSubmitted = false;
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
