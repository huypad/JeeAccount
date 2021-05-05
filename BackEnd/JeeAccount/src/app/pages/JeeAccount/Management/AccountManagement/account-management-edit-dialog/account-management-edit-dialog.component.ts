import { ChangeDetectionStrategy, ChangeDetectorRef, Component, Inject, OnInit } from '@angular/core';
import { MatDialogRef, MAT_DIALOG_DATA } from '@angular/material/dialog';
import { FormArray, FormBuilder, FormControl, FormGroup, Validators } from '@angular/forms';
import { AccountManagementModel, AppListDTO } from '../Model/account-management.model';
import { AccountManagementService } from '../Services/account-management.service';
import { ReplaySubject } from 'rxjs';
import { AuthService } from 'src/app/modules/auth/_services/auth.service';
import { DepartmentSelection } from '../../../_core/models/danhmuc.model';
import { LayoutUtilsService, MessageType } from '../../../_core/utils/layout-utils.service';
import { DanhMucChungService } from '../../../_core/services/danhmuc.service';
import { ResultModel } from '../../../_core/models/_base.model';

@Component({
  selector: 'app-account-management-edit-dialog',
  templateUrl: './account-management-edit-dialog.component.html',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class AccountManagementEditDialogComponent implements OnInit {
  item: any = [];
  itemForm = this.fb.group({
    AnhDaiDien: [''],
    HoTen: ['', [Validators.required]],
    Email: ['', [Validators.required]],
    PhongBan: ['', [Validators.required]],
    TenDangNhap: ['', [Validators.required]],
    MatKhau: ['', [Validators.required]],
    SoDienThoai: [''],
    ChucVu: ['', [Validators.required]],
    NhapLaiMatKhau: ['', [Validators.required]],
    AppsCheckbox: new FormArray([]),
    file: [],
    PhongBanFilterCtrl: [],
  });
  listApp: AppListDTO[] = [];
  CompanyCode = 'congtytest';
  imgFile = '../assets/media/Img/NoImage.jpg';
  // ngx-mat-search area
  phongBans: DepartmentSelection[] = [];
  filterPhongBans: ReplaySubject<DepartmentSelection[]> = new ReplaySubject<DepartmentSelection[]>();
  // End
  constructor(
    @Inject(MAT_DIALOG_DATA) public data: any,
    public dialogRef: MatDialogRef<AccountManagementEditDialogComponent>,
    private fb: FormBuilder,
    private accountManagementService: AccountManagementService,
    private changeDetect: ChangeDetectorRef,
    private layoutUtilsService: LayoutUtilsService,
    public danhmuc: DanhMucChungService,
    private authService: AuthService
  ) {}

  ngOnInit(): void {
    this.item = this.data.item;
    this.accountManagementService.GetListAppByCustomerID().subscribe((res: ResultModel<AppListDTO>) => {
      if (res && res.status === 1) {
        this.listApp = res.data;
        this.addCheckboxes();
        this.changeDetect.detectChanges();
      }
    });

    this.danhmuc.GetSelectionDepartment().subscribe((res: ResultModel<DepartmentSelection>) => {
      if (res && res.status === 1) {
        this.phongBans = res.data;
        this.filterPhongBans.next([...res.data]);
      }
    });
  }
  get AppsFromArray() {
    return this.itemForm.controls.AppsCheckbox as FormArray;
  }
  private addCheckboxes() {
    this.listApp.forEach((item) => this.AppsFromArray.push(new FormControl(item.IsDefaultApp)));
  }
  onSubmit() {
    if (this.itemForm.valid) {
      //  check password
      // if (this.itemForm.controls.MatKhau.value !== this.item.controls.NhapLaiMatKhau.value) {
      //   this.layoutUtilsService.showActionNotification(
      //     'Mật khẩu không trùng khớp',
      //     MessageType.Read,
      //     999999999,
      //     true,
      //     false,
      //     3000,
      //     'top',
      //     0
      //   );
      //   return;
      // }

      const acc = this.initDataFromFB();
      this.create(acc);
    } else {
      this.validateAllFormFields(this.itemForm);
    }
  }

  initDataFromFB(): AccountManagementModel {
    const acc = new AccountManagementModel();
    const AppCode: string[] = [];
    for (let index = 0; index < this.AppsFromArray.controls.length; index++) {
      if (this.AppsFromArray.controls[index].value === true) {
        AppCode.push(this.listApp[index].AppCode);
      }
    }
    acc.AppCode = AppCode;
    acc.Fullname = this.itemForm.controls.HoTen.value;
    acc.Email = this.itemForm.controls.Email.value;
    acc.Phonemumber = this.itemForm.controls.SoDienThoai.value;
    acc.Departmemt = this.itemForm.controls.PhongBan.value;
    acc.Jobtitle = this.itemForm.controls.ChucVu.value;
    acc.Username = `${this.CompanyCode}.${this.itemForm.controls.TenDangNhap.value}`;
    acc.Password = this.itemForm.controls.MatKhau.value;
    acc.ImageAvatar = this.imgFile.split(',')[1];
    return acc;
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

  onFileChange(event) {
    if (event.target.files && event.target.files[0]) {
      const reader = new FileReader();
      reader.onload = (e: any) => {
        this.imgFile = e.target.result;
        const filename = event.target.files[0].name;
        this.itemForm.controls.AnhDaiDien.setValue(filename);
        this.changeDetect.detectChanges();
      };
      reader.readAsDataURL(event.target.files[0]);
    }
  }
  create(acc: AccountManagementModel) {
    this.accountManagementService.createAccount(acc).subscribe((res) => {
      if (res && res.status === 1) {
        this.authService.saveNewUserMe(res.access_token, res.refresh_token);
        this.dialogRef.close(res);
      } else {
        this.layoutUtilsService.showActionNotification(res.error.message, MessageType.Read, 999999999, true, false, 3000, 'top', 0);
      }
    });
  }
  goBack() {
    this.dialogRef.close();
  }
}
