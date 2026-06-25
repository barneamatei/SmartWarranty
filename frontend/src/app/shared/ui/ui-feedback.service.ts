import { inject, Injectable } from '@angular/core';
import { MatSnackBar } from '@angular/material/snack-bar';
import { MatDialog } from '@angular/material/dialog';
import { map } from 'rxjs';

import { ConfirmDialogComponent } from './confirm-dialog/confirm-dialog.component';

@Injectable({ providedIn: 'root' })
export class UiFeedbackService {
  private readonly snackBar = inject(MatSnackBar);
  private readonly dialog = inject(MatDialog);

  success(message: string) {
    this.snackBar.open(message, 'OK', {
      duration: 3200,
      panelClass: ['snackbar-success']
    });
  }

  error(message: string) {
    this.snackBar.open(message, 'Close', {
      duration: 4200,
      panelClass: ['snackbar-error']
    });
  }

  confirm(title: string, message: string) {
    return this.dialog
      .open(ConfirmDialogComponent, {
        width: '420px',
        data: { title, message }
      })
      .afterClosed()
      .pipe(map((result) => Boolean(result)));
  }
}
