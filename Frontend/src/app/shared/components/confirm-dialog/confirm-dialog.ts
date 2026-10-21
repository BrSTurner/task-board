import { DIALOG_DATA, DialogRef } from '@angular/cdk/dialog';
import { Component, inject } from '@angular/core';

export interface ConfirmDialogData {
  title: string;
  message: string;
  confirmLabel: string;
}

/** Resolves `true` only when the user explicitly confirms. */
@Component({
  selector: 'app-confirm-dialog',
  template: `
    <div class="modal-content" role="alertdialog" aria-labelledby="confirm-dialog-title" aria-describedby="confirm-dialog-message">
      <div class="modal-header">
        <h2 class="modal-title h5" id="confirm-dialog-title">{{ data.title }}</h2>
        <button type="button" class="btn-close" aria-label="Cancel" (click)="dialogRef.close(false)"></button>
      </div>
      <div class="modal-body">
        <p class="mb-0 text-break" id="confirm-dialog-message">{{ data.message }}</p>
      </div>
      <div class="modal-footer">
        <button type="button" class="btn btn-outline-secondary" (click)="dialogRef.close(false)" cdkFocusInitial>Cancel</button>
        <button type="button" class="btn btn-danger" (click)="dialogRef.close(true)">{{ data.confirmLabel }}</button>
      </div>
    </div>
  `,
})
export class ConfirmDialog {
  protected readonly data = inject<ConfirmDialogData>(DIALOG_DATA);
  protected readonly dialogRef = inject<DialogRef<boolean>>(DialogRef);
}
