import { Component, OnInit, inject, input, signal } from '@angular/core';
import { NonNullableFormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { Router, RouterLink } from '@angular/router';

import { safeReturnUrl } from '../../../../core/auth/auth.guards';
import { AuthStore } from '../../../../core/auth/auth.store';
import { notBlank } from '../../../../shared/validators';

@Component({
  selector: 'app-login',
  imports: [ReactiveFormsModule, RouterLink],
  templateUrl: './login.html',
})
export class LoginPage implements OnInit {
  /** Query parameters (bound through `withComponentInputBinding`). */
  readonly email = input<string>();
  readonly returnUrl = input<string>();

  private readonly auth = inject(AuthStore);
  private readonly router = inject(Router);

  protected readonly form = inject(NonNullableFormBuilder).group({
    email: ['', [notBlank, Validators.email]],
    password: ['', [notBlank]],
  });
  protected readonly submitting = signal(false);
  protected readonly submitted = signal(false);

  ngOnInit(): void {
    const email = this.email();
    if (email) {
      this.form.controls.email.setValue(email);
    }
  }

  protected invalid(field: 'email' | 'password'): boolean {
    const control = this.form.controls[field];
    return control.invalid && (control.touched || this.submitted());
  }

  protected async submit(): Promise<void> {
    this.submitted.set(true);
    this.form.markAllAsTouched();
    if (this.form.invalid || this.submitting()) {
      return;
    }

    this.submitting.set(true);
    try {
      const { email, password } = this.form.getRawValue();
      await this.auth.login(email.trim(), password);
      await this.router.navigateByUrl(safeReturnUrl(this.returnUrl()));
    } catch {
      // The error interceptor already showed the reason (e.g. "Invalid email or password.") in a modal.
      this.form.controls.password.reset();
    } finally {
      this.submitting.set(false);
    }
  }
}
