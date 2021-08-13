import { JobtitleManagementService } from './../Sevices/jobtitle-management.service';
import { LayoutUtilsService, MessageType } from '../../../_core/utils/layout-utils.service';
import { ChangeDetectionStrategy, Component, Inject, OnInit } from '@angular/core';
import { MatDialogRef, MAT_DIALOG_DATA } from '@angular/material/dialog';
import { FormBuilder, FormControl, FormGroup, Validators } from '@angular/forms';
import { BehaviorSubject } from 'rxjs';
import { JobChangeTinhTrangModel } from '../Model/jobtitle-management.model';

@Component({
  selector: 'app-change-tinh-trang-jobtitle-edit-dialog',
  templateUrl: './change-tinh-trang-jobtitle-edit-dialog.component.html',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class ChangeTinhTrangJobtitleEditDialogComponent implements OnInit {
  itemForm = this.fb.group({
    GhiChu: ['', [Validators.required]],
  });
  isLoadingSubmit$: BehaviorSubject<boolean>;

  constructor(
    @Inject(MAT_DIALOG_DATA) public data: any,
    public dialogRef: MatDialogRef<ChangeTinhTrangJobtitleEditDialogComponent>,
    private fb: FormBuilder,
    private jobtitleManagementService: JobtitleManagementService,
    private layoutUtilsService: LayoutUtilsService
  ) {}

  ngOnInit(): void {
    this.isLoadingSubmit$ = new BehaviorSubject(false);
  }

  onSubmit() {
    if (this.itemForm.valid) {
      const accChangTinhTrang = this.initDataFromFB();
      this.update(accChangTinhTrang);
    } else {
      this.validateAllFormFields(this.itemForm);
    }
  }

  update(acc: JobChangeTinhTrangModel) {
    this.isLoadingSubmit$.next(true);
    this.jobtitleManagementService.changeTinhTrang(acc).subscribe(
      (res) => {
        if (res) {
          this.isLoadingSubmit$.next(false);
          this.dialogRef.close(res);
        }
      },
      (error) => {
        this.isLoadingSubmit$.next(false);
        this.layoutUtilsService.showActionNotification(error.error.message, MessageType.Read, 999999999, true, false, 3000, 'top', 0);
      }
    );
  }

  initDataFromFB(): JobChangeTinhTrangModel {
    const dep = new JobChangeTinhTrangModel();
    dep.clear();
    dep.Note = this.itemForm.controls.GhiChu.value;
    dep.RowID = this.data.RowID;
    return dep;
  }

  validateAllFormFields(formGroup: FormGroup) {
    Object.keys(formGroup.controls).forEach((field) => {
      const control = formGroup.get(field);
      if (control instanceof FormControl) {
        control.markAsTouched({ onlySelf: true });
      } else if (control instanceof FormGroup) {
        this.validateAllFormFields(control);
      }
    });
  }

  goBack() {
    this.dialogRef.close();
  }
}
