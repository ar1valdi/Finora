import { Component, OnInit } from '@angular/core';
import { AddUserFormComponent } from '../users/add-user-form/add-user-form.component';
import { Router, ActivatedRoute } from '@angular/router';
import { FormControl, FormGroup, ReactiveFormsModule, Validators } from '@angular/forms';
import { CommonModule } from '@angular/common';
import { AuthService } from '../../services/auth.service';

@Component({
  selector: 'app-register',
  standalone: true,
  imports: [AddUserFormComponent, ReactiveFormsModule, CommonModule],
  templateUrl: './register.component.html',
  styleUrl: './register.component.scss'
})
export class RegisterComponent implements OnInit {
  formMode: 'register' | 'login' = 'register';
  loginForm = new FormGroup({
    email: new FormControl('', [Validators.required, Validators.email]),
    password: new FormControl('', [Validators.required]),
  });

  constructor(private router: Router, 
    private activatedRoute: ActivatedRoute, 
    private authService: AuthService) {}

  ngOnInit() {
    this.activatedRoute.queryParams.subscribe((params) => { 
    if (params['mode']) {
        this.formMode = params['mode'];
      } else {
        this.formMode = 'register';
      }
    });
  }

  toogleFormMode() {
    const newMode = this.formMode === 'register' ? 'login' : 'register';
    this.formMode = newMode;
  }

  redirectToLogin() {
    console.log('Redirecting to login');
    this.formMode = 'login';
  }

  onLoginSubmit() {
    if (this.loginForm.invalid) {
      this.loginForm.markAllAsTouched();
      return;
    }

    this.authService.login(this.loginForm.value.email!, this.loginForm.value.password!).then(() => {
      this.router.navigate(['/']);
    });

    this.router.navigate(['/']);
  }
}
