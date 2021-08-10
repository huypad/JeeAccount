import { LayoutUtilsService, MessageType } from './../../../_core/utils/layout-utils.service';
import { ChangeDetectionStrategy, ChangeDetectorRef, Component, Inject, OnInit } from '@angular/core';
import { MatDialogRef, MAT_DIALOG_DATA } from '@angular/material/dialog';
import { FormBuilder, FormControl, FormGroup, Validators } from '@angular/forms';
import { AccChangeTinhTrangModel } from '../../AccountManagement/Model/account-management.model';
import { AccountManagementService } from '../Services/account-management.service';
import { DanhMucChungService } from '../../../_core/services/danhmuc.service';
import { BehaviorSubject } from 'rxjs';

@Component({
  selector: 'app-change-tinh-trang-edit-dialog',
  templateUrl: './change-tinh-trang-edit-dialog.component.html',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class ChangeTinhTrangEditDialogComponent implements OnInit {
  itemForm = this.fb.group({
    GhiChu: ['', [Validators.required]],
  });
  isLoadingSubmit$: BehaviorSubject<boolean>;

  constructor(
    @Inject(MAT_DIALOG_DATA) public data: any,
    public dialogRef: MatDialogRef<ChangeTinhTrangEditDialogComponent>,
    private fb: FormBuilder,
    private accountManagementService: AccountManagementService,
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
  update(accChangTinhTrang: AccChangeTinhTrangModel) {
    this.isLoadingSubmit$.next(true);
    this.accountManagementService.changeTinhTrang(accChangTinhTrang).subscribe(
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

  initDataFromFB(): AccChangeTinhTrangModel {
    const accChangTinhTrang = new AccChangeTinhTrangModel();
    accChangTinhTrang.clear();
    accChangTinhTrang.Note = this.itemForm.controls.GhiChu.value;
    accChangTinhTrang.Username = this.data.Username;
    return accChangTinhTrang;
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
