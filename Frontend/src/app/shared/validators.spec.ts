import { FormControl, FormGroup } from '@angular/forms';

import { matchingFields, maxTrimmedLength, notBlank, passwordPolicy } from './validators';

describe('validators', () => {
  it('notBlank treats whitespace-only text as missing', () => {
    expect(notBlank(new FormControl('   '))).toEqual({ required: true });
    expect(notBlank(new FormControl(null))).toEqual({ required: true });
    expect(notBlank(new FormControl(' a '))).toBeNull();
  });

  it('maxTrimmedLength ignores surrounding whitespace, like the API', () => {
    const max50 = maxTrimmedLength(50);

    expect(max50(new FormControl('  ' + 'x'.repeat(50) + '  '))).toBeNull();
    expect(max50(new FormControl('x'.repeat(51)))).toEqual({ maxlength: { requiredLength: 50, actualLength: 51 } });
  });

  it('passwordPolicy lists every unmet rule', () => {
    expect(passwordPolicy(new FormControl('DemoPass123!'))).toBeNull();
    expect(passwordPolicy(new FormControl('short'))).toEqual({
      passwordPolicy: ['8–100 characters', 'an upper-case letter', 'a digit', 'a symbol'],
    });
    expect(passwordPolicy(new FormControl(''))).toBeNull();
  });

  it('matchingFields flags a confirmation that differs', () => {
    const group = new FormGroup(
      { password: new FormControl('DemoPass123!'), confirmPassword: new FormControl('Different1!') },
      { validators: matchingFields('password', 'confirmPassword') },
    );

    expect(group.errors).toEqual({ passwordMismatch: true });

    group.controls.confirmPassword.setValue('DemoPass123!');
    expect(group.errors).toBeNull();
  });
});
