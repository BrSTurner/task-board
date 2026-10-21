import { ComponentFixture, TestBed } from '@angular/core/testing';
import { Router, provideRouter } from '@angular/router';
import { ToastrService } from 'ngx-toastr';

import { AuthStore } from '../../../../core/auth/auth.store';
import { RegisterPage } from './register';

describe('RegisterPage', () => {
  let fixture: ComponentFixture<RegisterPage>;
  let auth: { register: ReturnType<typeof vi.fn> };
  let toastr: { success: ReturnType<typeof vi.fn> };
  let element: HTMLElement;

  beforeEach(async () => {
    auth = { register: vi.fn() };
    toastr = { success: vi.fn() };
    await TestBed.configureTestingModule({
      imports: [RegisterPage],
      providers: [provideRouter([]), { provide: AuthStore, useValue: auth }, { provide: ToastrService, useValue: toastr }],
    }).compileComponents();
    fixture = TestBed.createComponent(RegisterPage);
    element = fixture.nativeElement;
    await fixture.whenStable();
  });

  function setValue(selector: string, value: string): void {
    const input = element.querySelector<HTMLInputElement | HTMLSelectElement>(selector)!;
    input.value = value;
    input.dispatchEvent(new Event(input instanceof HTMLSelectElement ? 'change' : 'input'));
  }

  async function submit(): Promise<void> {
    element.querySelector<HTMLButtonElement>('button[type=submit]')!.click();
    await fixture.whenStable();
  }

  it('highlights every empty required field with a message and does not submit (Scenario 2)', async () => {
    setValue('#register-timezone', '');

    await submit();

    for (const [id, message] of [
      ['#register-name', 'Name is required'],
      ['#register-email', 'Email is required'],
      ['#register-timezone', 'Time zone is required'],
      ['#register-password', 'Password is required'],
    ]) {
      expect(element.querySelector(id)!.classList).toContain('is-invalid');
      expect(element.textContent).toContain(message);
    }
    expect(auth.register).not.toHaveBeenCalled();
  });

  it('enforces the password policy and confirmation', async () => {
    setValue('#register-password', 'weak');
    setValue('#register-confirm', 'different');

    await submit();

    expect(element.textContent).toContain('Password must contain');
    expect(element.textContent).toContain('Passwords do not match');
    expect(auth.register).not.toHaveBeenCalled();
  });

  it('registers and sends the user to login (no auto sign-in)', async () => {
    auth.register.mockResolvedValue({ id: '1', name: 'Carol', email: 'carol@example.test', timeZone: 'Europe/London' });
    const navigate = vi.spyOn(TestBed.inject(Router), 'navigate').mockResolvedValue(true);
    setValue('#register-name', ' Carol ');
    setValue('#register-email', 'carol@example.test');
    setValue('#register-timezone', 'Europe/London');
    setValue('#register-password', 'CarolPass123!');
    setValue('#register-confirm', 'CarolPass123!');

    await submit();

    expect(auth.register).toHaveBeenCalledWith({ name: 'Carol', email: 'carol@example.test', timeZone: 'Europe/London', password: 'CarolPass123!' });
    expect(toastr.success).toHaveBeenCalled();
    expect(navigate).toHaveBeenCalledWith(['/login'], { queryParams: { email: 'carol@example.test' } });
  });
});
