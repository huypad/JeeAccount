import { DepartmentManagementService } from './../Sevices/department-management.service';
import { LayoutUtilsService, MessageType } from '../../../_core/utils/layout-utils.service';
import { ChangeDetectionStrategy, Component, Inject, OnDestroy, OnInit } from '@angular/core';
import { MatDialogRef, MAT_DIALOG_DATA } from '@angular/material/dialog';
import { FormBuilder, FormControl, FormGroup, Validators } from '@angular/forms';
import { BehaviorSubject, Subscription } from 'rxjs';
import { DepChangeTinhTrangModel } from '../Model/department-management.model';

@Component({
  selector: 'app-change-tinh-trang-department-edit-dialog',
  templateUrl: './change-tinh-trang-department-edit-dialog.component.html',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class ChangeTinhTrangDepartmentEditDialogComponent implements OnInit, OnDestroy {
  itemForm = this.fb.group({
    GhiChu: ['', [Validators.required]],
  });
  isLoadingSubmit$: BehaviorSubject<boolean>;
  private subscriptions: Subscription[] = [];

  constructor(
    @Inject(MAT_DIALOG_DATA) public data: any,
    public dialogRef: MatDialogRef<ChangeTinhTrangDepartmentEditDialogComponent>,
    private fb: FormBuilder,
    private departmentManagementService: DepartmentManagementService,
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

  update(acc: DepChangeTinhTrangModel) {
    this.isLoadingSubmit$.next(true);
    const sb = this.departmentManagementService.changeTinhTrang(acc).subscribe(
      (res) => {
        this.isLoadingSubmit$.next(false);
        this.dialogRef.close(res);
      },
      (error) => {
        this.isLoadingSubmit$.next(false);
        this.layoutUtilsService.showActionNotification(error.error.message, MessageType.Read, 999999999, true, false, 3000, 'top', 0);
      }
    );
    this.subscriptions.push(sb);
  }

  initDataFromFB(): DepChangeTinhTrangModel {
    const dep = new DepChangeTinhTrangModel();
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

  ngOnDestroy(): void {
    this.subscriptions.forEach((sb) => sb.unsubscribe());
    this.departmentManagementService.ngOnDestroy();
  }
}
