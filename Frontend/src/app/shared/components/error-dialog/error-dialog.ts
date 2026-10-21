import { DIALOG_DATA, DialogRef } from '@angular/cdk/dialog';
import { Component, inject } from '@angular/core';

import { DisplayError } from '../../../core/errors/api-error';

@Component({
  selector: 'app-error-dialog',
  template: `
    <div class="modal-content" role="alertdialog" aria-labelledby="error-dialog-title" aria-describedby="error-dialog-message">
      <div class="modal-header bg-danger-subtle">
        <h2 class="modal-title h5 text-danger-emphasis" id="error-dialog-title">{{ data.title }}</h2>
        <button type="button" class="btn-close" aria-label="Close" (click)="close()"></button>
      </div>
      <div class="modal-body">
        <p class="mb-0" id="error-dialog-message">{{ data.message }}</p>
        @if (data.details.length) {
          <ul class="mt-2 mb-0 small">
            @for (detail of data.details; track $index) {
              <li>{{ detail }}</li>
            }
          </ul>
        }
      </div>
      <div class="modal-footer">
        <button type="button" class="btn btn-danger" (click)="close()" cdkFocusInitial>OK</button>
      </div>
    </div>
  `,
})
export class ErrorDialog {
  protected readonly data = inject<DisplayError>(DIALOG_DATA);
  private readonly dialogRef = inject(DialogRef);

  protected close(): void {
    this.dialogRef.close();
  }
}
