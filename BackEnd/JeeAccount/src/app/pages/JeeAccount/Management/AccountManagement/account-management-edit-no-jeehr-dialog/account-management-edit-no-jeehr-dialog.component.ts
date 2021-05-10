import { ChangeDetectionStrategy, ChangeDetectorRef, Component, Inject, OnInit } from '@angular/core';
import { MatDialogRef, MAT_DIALOG_DATA } from '@angular/material/dialog';
import { FormArray, FormBuilder, FormControl, FormGroup, Validators } from '@angular/forms';
import { AccountManagementModel, AppListDTO, InfoUserDTO } from '../Model/account-management.model';
import { AccountManagementService } from '../Services/account-management.service';
import { ReplaySubject } from 'rxjs';
import { AuthService } from 'src/app/modules/auth/_services/auth.service';
import { DepartmentSelection } from '../../../_core/models/danhmuc.model';
import { LayoutUtilsService, MessageType } from '../../../_core/utils/layout-utils.service';
import { DanhMucChungService } from '../../../_core/services/danhmuc.service';
import { ResultModel } from '../../../_core/models/_base.model';

@Component({
  selector: 'app-account-management-edit-no-jeehr-dialog',
  templateUrl: './account-management-edit-no-jeehr-dialog.component.html',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class AccountManagementEditNoJeeHRDialogComponent implements OnInit {
  itemForm = this.fb.group({
    HoLot: ['', [Validators.required]],
    Ten: ['', [Validators.required]],
    Email: [''],
    PhongBan: ['', [Validators.required]],
    SoDienThoai: [''],
    ChucVu: ['', [Validators.required]],
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
  item: InfoUserDTO;
  username: string;

  constructor(
    @Inject(MAT_DIALOG_DATA) public data: any,
    public dialogRef: MatDialogRef<AccountManagementEditNoJeeHRDialogComponent>,
    private fb: FormBuilder,
    private accountManagementService: AccountManagementService,
    private changeDetect: ChangeDetectorRef,
    private layoutUtilsService: LayoutUtilsService,
    public danhmuc: DanhMucChungService,
    private authService: AuthService
  ) { }

  ngOnInit(): void {
    this.accountManagementService.GetInfoByUsername(this.data.item.Username).subscribe((res) => {
      if (res && res.data) {
        this.item = res.data;
        this.initData();
      }
    });
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
  initData() {
    this.itemForm.controls.HoLot.setValue(this.item.LastName);
    this.itemForm.controls.Ten.setValue(this.item.Name);
    this.itemForm.controls.Email.setValue(this.item.Email);
    this.itemForm.controls.SoDienThoai.setValue(this.item.PhoneNumber);
    this.itemForm.controls.ChucVu.setValue(this.item.Jobtitle);
    const index = this.phongBans.findIndex((x) => x.RowID === this.item.Departmemt);
    this.itemForm.controls.PhongBan.setValue(this.phongBans[index].RowID);
  }
  get AppsFromArray() {
    return this.itemForm.controls.AppsCheckbox as FormArray;
  }
  private addCheckboxes() {
    this.listApp.forEach((item) => this.AppsFromArray.push(new FormControl(item.IsDefaultApp)));
  }
  onSubmit() {
    if (this.itemForm.valid) {
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
    // this.accountManagementService.createAccount(acc).subscribe((res) => {
    //   if (res && res.status === 1) {
    //     this.authService.saveNewUserMe(res.access_token, res.refresh_token);
    //     this.dialogRef.close(res);
    //   } else {
    //     this.layoutUtilsService.showActionNotification(res.error.message, MessageType.Read, 999999999, true, false, 3000, 'top', 0);
    //   }
    // });
  }
  goBack() {
    this.dialogRef.close();
  }
}
