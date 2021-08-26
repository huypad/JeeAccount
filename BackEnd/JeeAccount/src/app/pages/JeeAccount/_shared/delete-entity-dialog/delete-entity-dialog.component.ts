import { TranslateService } from '@ngx-translate/core';
import { Component, HostListener, Inject, OnInit } from '@angular/core';
import { MatDialogRef, MAT_DIALOG_DATA } from '@angular/material/dialog';

@Component({
  selector: 'm-delete-entity-dialog',
  templateUrl: './delete-entity-dialog.component.html',
})
export class DeleteEntityDialogComponent implements OnInit {
  viewLoading: boolean = false;
  constructor(
    public dialogRef: MatDialogRef<DeleteEntityDialogComponent>,
    @Inject(MAT_DIALOG_DATA) public data: any,
    public trans: TranslateService
  ) {}

  nameButtonCancel: string;
  nameButtonOK: string;
  ngOnInit() {
    if (!this.data.nameButtonCancel) {
      this.nameButtonCancel = this.trans.instant('CHECKPOPUP.KHONG');
    } else {
      this.nameButtonCancel = this.data.nameButtonCancel;
    }
    if (!this.data.nameButtonOK) {
      this.nameButtonOK = this.trans.instant('CHECKPOPUP.CO');
    } else {
      this.nameButtonOK = this.data.nameButtonOK;
    }
  }

  @HostListener('document:keydown', ['$event'])
  onKeydownHandler(event: KeyboardEvent) {
    if (event.keyCode == 13) {
      //phím Enter
      if (this.data.isDel == true) {
        this.onNoClick(); //if delete confirm, default is no
      } else {
        this.onYesClick();
      }
    }
  }

  onNoClick(): void {
    this.dialogRef.close();
  }

  onYesClick(): void {
    /* Server loading imitation. Remove this */
    this.viewLoading = true;
    setTimeout(() => {
      this.dialogRef.close(true); // Keep only this row
    }, 0);
  }
}
