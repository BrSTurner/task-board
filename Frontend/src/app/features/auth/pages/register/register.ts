import { Component, inject, signal } from '@angular/core';
import { NonNullableFormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { Router, RouterLink } from '@angular/router';
import { ToastrService } from 'ngx-toastr';

import { AuthStore } from '../../../../core/auth/auth.store';
import { availableTimeZones, browserTimeZone } from '../../../../shared/utils/time-zones';
import { matchingFields, maxTrimmedLength, notBlank, passwordPolicy } from '../../../../shared/validators';

type Field = 'name' | 'email' | 'timeZone' | 'password' | 'confirmPassword';

@Component({
  selector: 'app-register',
  imports: [ReactiveFormsModule, RouterLink],
  templateUrl: './register.html',
})
export class RegisterPage {
  private readonly auth = inject(AuthStore);
  private readonly router = inject(Router);
  private readonly toastr = inject(ToastrService);

  protected readonly timeZones = availableTimeZones();
  protected readonly form = inject(NonNullableFormBuilder).group(
    {
      name: ['', [notBlank, maxTrimmedLength(100)]],
      email: ['', [notBlank, Validators.email, maxTrimmedLength(254)]],
      timeZone: [browserTimeZone(), [notBlank]],
      password: ['', [notBlank, passwordPolicy]],
      confirmPassword: ['', [notBlank]],
    },
    { validators: matchingFields('password', 'confirmPassword') },
  );
  protected readonly submitting = signal(false);
  protected readonly submitted = signal(false);

  protected invalid(field: Field): boolean {
    const control = this.form.controls[field];
    const mismatch = field === 'confirmPassword' && this.form.hasError('passwordMismatch');
    return (control.invalid || mismatch) && (control.touched || this.submitted());
  }

  protected errorFor(field: Field): string {
    const control = this.form.controls[field];
    const labels: Record<Field, string> = {
      name: 'Name',
      email: 'Email',
      timeZone: 'Time zone',
      password: 'Password',
      confirmPassword: 'Password confirmation',
    };

    if (control.hasError('required')) return `${labels[field]} is required`;
    if (control.hasError('email')) return 'Enter a valid email address';
    if (control.hasError('maxlength')) {
      return `${labels[field]} must be at most ${control.getError('maxlength').requiredLength} characters`;
    }
    if (control.hasError('passwordPolicy')) {
      return `Password must contain ${(control.getError('passwordPolicy') as string[]).join(', ')}`;
    }
    if (field === 'confirmPassword' && this.form.hasError('passwordMismatch')) return 'Passwords do not match';
    return '';
  }

  protected async submit(): Promise<void> {
    this.submitted.set(true);
    this.form.markAllAsTouched();
    if (this.form.invalid || this.submitting()) {
      return;
    }

    this.submitting.set(true);
    try {
      const { name, email, timeZone, password } = this.form.getRawValue();
      const user = await this.auth.register({ name: name.trim(), email: email.trim(), timeZone, password });
      this.toastr.success('Account created. Please sign in.');
      await this.router.navigate(['/login'], { queryParams: { email: user.email } });
    } catch {
      // Shown by the error interceptor (e.g. 409 "An account with this email already exists.").
    } finally {
      this.submitting.set(false);
    }
  }
}
