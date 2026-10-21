import { AbstractControl, ValidationErrors, ValidatorFn } from '@angular/forms';

/**
 * Client-side mirrors of the backend FluentValidation rules. The server remains authoritative; these give
 * immediate feedback. Lengths are measured after trimming, exactly like the API.
 */

/** Like `Validators.required`, but whitespace-only text is also missing. Error key: `required`. */
export const notBlank: ValidatorFn = (control: AbstractControl): ValidationErrors | null => {
  const value = control.value;
  const blank = value === null || value === undefined || (typeof value === 'string' && value.trim().length === 0);
  return blank ? { required: true } : null;
};

/** Error key: `maxlength` (same shape as Angular's), measured on the trimmed value. */
export function maxTrimmedLength(maxLength: number): ValidatorFn {
  return (control: AbstractControl): ValidationErrors | null => {
    const value = typeof control.value === 'string' ? control.value.trim() : '';
    return value.length > maxLength ? { maxlength: { requiredLength: maxLength, actualLength: value.length } } : null;
  };
}

export const PASSWORD_MIN_LENGTH = 8;
export const PASSWORD_MAX_LENGTH = 100;

/** Same policy as `RegisterUserRequestValidator`. Error key: `passwordPolicy` with the list of unmet rules. */
export const passwordPolicy: ValidatorFn = (control: AbstractControl): ValidationErrors | null => {
  const value: string = control.value ?? '';
  if (!value) {
    return null; // `required` reports the empty case
  }

  const unmet: string[] = [];
  if (value.length < PASSWORD_MIN_LENGTH || value.length > PASSWORD_MAX_LENGTH) {
    unmet.push(`${PASSWORD_MIN_LENGTH}–${PASSWORD_MAX_LENGTH} characters`);
  }
  if (!/[A-Z]/.test(value)) unmet.push('an upper-case letter');
  if (!/[a-z]/.test(value)) unmet.push('a lower-case letter');
  if (!/[0-9]/.test(value)) unmet.push('a digit');
  if (!/[^a-zA-Z0-9]/.test(value)) unmet.push('a symbol');

  return unmet.length ? { passwordPolicy: unmet } : null;
};

/** Group validator. Error key on the group: `passwordMismatch`. */
export function matchingFields(field: string, confirmation: string): ValidatorFn {
  return (group: AbstractControl): ValidationErrors | null => {
    const confirmationValue = group.get(confirmation)?.value;
    return confirmationValue && group.get(field)?.value !== confirmationValue ? { passwordMismatch: true } : null;
  };
}
