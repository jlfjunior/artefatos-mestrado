import { Component } from '@angular/core';
import { Router, ActivatedRoute } from '@angular/router';
import { FormBuilder, FormGroup, Validators } from '@angular/forms';
import { AuthService } from '../../services/auth.service';
import { ToastrService } from 'ngx-toastr';

@Component({
    selector: 'app-login',
    templateUrl: './login.component.html',
    standalone: false
})
export class LoginComponent {
  loginForm: FormGroup;
  loading = false;
  error = '';
  returnUrl: string;

  constructor(
    private readonly formBuilder: FormBuilder, 
    private readonly route: ActivatedRoute, 
    private readonly router: Router, 
    private readonly authService: AuthService, 
    private readonly toast: ToastrService
  ) {
    this.loginForm = this.formBuilder.group({
      email: ['', [Validators.required, Validators.email]],
      password: ['', [Validators.required, Validators.minLength(8)]]
    });

    this.returnUrl = this.route.snapshot.queryParams['returnUrl'] ?? '/';
  }

  ngOnInit() {
    if (this.authService.isAuthenticated()) {
      this.router.navigate([this.returnUrl]);
    }
  }

  get f() { return this.loginForm.controls; }

  onSubmit() {
    if (this.loginForm.invalid) {
      return;
    }

    this.loading = true;
    this.authService.login(this.f['email'].value, this.f['password'].value)
      .subscribe({
        next: () => {
          this.router.navigate([this.returnUrl]);
        },
        error: error => {
          this.toast.error('Login falhou. Verifique suas credenciais e tente novamente.');
          this.loading = false;
        },
        complete: () => {
          this.loading = false;
        }
      });
  }
}