import {
  AfterViewInit,
  ChangeDetectionStrategy,
  ChangeDetectorRef,
  Component,
  ElementRef,
  Inject,
  OnDestroy,
  OnInit,
  ViewChild,
} from '@angular/core';
import { MatDialogRef, MAT_DIALOG_DATA } from '@angular/material/dialog';
import { FormArray, FormBuilder, FormControl, FormGroup, Validators } from '@angular/forms';
import { ResultModel } from '../../_core/models/_base.model';
import { AccChangeTinhTrangModel, AppListDTO } from '../../AccountManagement/Model/account-management.model';
import { DanhMucChungService } from '../../_core/services/danhmuc.service';
import { BehaviorSubject, Observable, ReplaySubject } from 'rxjs';
import { COMMA, ENTER } from '@angular/cdk/keycodes';
import { MatAutocomplete, MatAutocompleteSelectedEvent } from '@angular/material/autocomplete';
import { map, startWith } from 'rxjs/operators';
import { MatChipInputEvent } from '@angular/material/chips';
import { MatSelect } from '@angular/material/select';
import { LayoutUtilsService, MessageType } from '../../_core/utils/layout-utils.service';
import { NhanVienMatchip } from '../../_core/models/danhmuc.model';
import { AccountManagementService } from '../Services/account-management.service';
import { AccDirectManagerModel } from '../Model/account-management.model';

@Component({
  selector: 'app-change-tinh-trang-edit-dialog',
  templateUrl: './change-tinh-trang-edit-dialog.component.html',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class ChangeTinhTrangEditDialogComponent implements OnInit {
  itemForm = this.fb.group({
    GhiChu: ['', [Validators.required]],
  });

  constructor(
    @Inject(MAT_DIALOG_DATA) public data: any,
    public dialogRef: MatDialogRef<ChangeTinhTrangEditDialogComponent>,
    private fb: FormBuilder,
    private accountManagementService: AccountManagementService,
    private changeDetect: ChangeDetectorRef,
    private danhmucService: DanhMucChungService,
    private layoutUtilsService: LayoutUtilsService
  ) {}

  ngOnInit(): void {}
  onSubmit() {
    if (this.itemForm.valid) {
      const accChangTinhTrang = this.initDataFromFB();
      this.update(accChangTinhTrang);
    } else {
      this.validateAllFormFields(this.itemForm);
    }
  }
  update(accChangTinhTrang: AccChangeTinhTrangModel) {
    this.accountManagementService.changeTinhTrang(accChangTinhTrang).subscribe((res) => {
      if (res && res.status === 1) {
        this.dialogRef.close(res);
      } else {
        this.layoutUtilsService.showActionNotification(res.error.message, MessageType.Read, 999999999, true, false, 3000, 'top', 0);
      }
    });
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
