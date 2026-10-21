import { ComponentFixture, TestBed } from '@angular/core/testing';
import { Router, provideRouter } from '@angular/router';

import { AuthStore } from '../../../../core/auth/auth.store';
import { LoginPage } from './login';

describe('LoginPage', () => {
  let fixture: ComponentFixture<LoginPage>;
  let auth: { login: ReturnType<typeof vi.fn> };
  let element: HTMLElement;

  beforeEach(async () => {
    auth = { login: vi.fn() };
    await TestBed.configureTestingModule({
      imports: [LoginPage],
      providers: [provideRouter([]), { provide: AuthStore, useValue: auth }],
    }).compileComponents();
    fixture = TestBed.createComponent(LoginPage);
    element = fixture.nativeElement;
  });

  function setValue(selector: string, value: string): void {
    const input = element.querySelector<HTMLInputElement>(selector)!;
    input.value = value;
    input.dispatchEvent(new Event('input'));
  }

  async function submit(): Promise<void> {
    element.querySelector<HTMLButtonElement>('button[type=submit]')!.click();
    await fixture.whenStable();
  }

  it('requires email and password before calling the API', async () => {
    await fixture.whenStable();

    await submit();

    expect(element.textContent).toContain('Email is required');
    expect(element.textContent).toContain('Password is required');
    expect(auth.login).not.toHaveBeenCalled();
  });

  it('signs in and navigates to the requested page (Scenario 4)', async () => {
    fixture.componentRef.setInput('returnUrl', '/tasks');
    await fixture.whenStable();
    auth.login.mockResolvedValue({});
    const navigate = vi.spyOn(TestBed.inject(Router), 'navigateByUrl').mockResolvedValue(true);
    setValue('#login-email', ' demo@example.test ');
    setValue('#login-password', 'DemoPass123!');

    await submit();

    expect(auth.login).toHaveBeenCalledWith('demo@example.test', 'DemoPass123!');
    expect(navigate).toHaveBeenCalledWith('/tasks');
  });

  it('stays on the page and clears the password when the credentials are rejected (Scenario 5)', async () => {
    await fixture.whenStable();
    auth.login.mockRejectedValue(new Error('401'));
    const navigate = vi.spyOn(TestBed.inject(Router), 'navigateByUrl');
    setValue('#login-email', 'demo@example.test');
    setValue('#login-password', 'wrong');

    await submit();

    expect(navigate).not.toHaveBeenCalled();
    expect(element.querySelector<HTMLInputElement>('#login-password')!.value).toBe('');
  });

  it('prefills the email after registration', async () => {
    fixture.componentRef.setInput('email', 'carol@example.test');
    await fixture.whenStable();

    expect(element.querySelector<HTMLInputElement>('#login-email')!.value).toBe('carol@example.test');
  });
});
