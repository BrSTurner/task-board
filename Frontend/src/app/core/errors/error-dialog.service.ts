import { Dialog, DialogRef } from '@angular/cdk/dialog';
import { Injectable, inject } from '@angular/core';

import { ErrorDialog } from '../../shared/components/error-dialog/error-dialog';
import { DisplayError } from './api-error';

/** Shows API errors in a modal. Only one error modal is open at a time (parallel failures don't stack). */
@Injectable({ providedIn: 'root' })
export class ErrorDialogService {
  private readonly dialog = inject(Dialog);
  private openRef: DialogRef<unknown, ErrorDialog> | null = null;

  show(error: DisplayError): void {
    if (this.openRef) {
      return;
    }

    this.openRef = this.dialog.open(ErrorDialog, {
      data: error,
      panelClass: 'tb-dialog-panel',
      ariaModal: true,
    });
    this.openRef.closed.subscribe(() => (this.openRef = null));
  }
}
