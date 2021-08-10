import { AccountManagementDTO, CheckEditAppListByDTO } from './../Model/account-management.model';
import { ChangeDetectionStrategy, ChangeDetectorRef, Component, Inject, OnInit, OnDestroy, HostListener } from '@angular/core';
import { MatDialogRef, MAT_DIALOG_DATA } from '@angular/material/dialog';
import { FormArray, FormBuilder, FormControl, FormGroup, Validators } from '@angular/forms';
import { AccountManagementModel, AppListDTO, JeeHRNhanVien } from '../Model/account-management.model';
import { AccountManagementService } from '../Services/account-management.service';
import { ReplaySubject, of, BehaviorSubject, Subscription } from 'rxjs';
import { LayoutUtilsService, MessageType } from '../../../_core/utils/layout-utils.service';
import { DanhMucChungService } from '../../../_core/services/danhmuc.service';
import { ResultModel } from '../../../_core/models/_base.model';
import { DatePipe } from '@angular/common';
import { catchError, finalize, tap } from 'rxjs/operators';
import { TranslateService } from '@ngx-translate/core';

@Component({
  selector: 'app-account-management-chinhsua-jeehr-dialog',
  templateUrl: './account-management-chinhsua-jeehr-dialog.component.html',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class AccountManagementChinhSuaJeeHRDialogComponent implements OnInit, OnDestroy {
  item: AccountManagementModel;
  itemData: AccountManagementDTO;
  userid: number;
  staffid: number = 0;
  itemForm = this.fb.group({
    NhanVien: ['', [Validators.required]],
    NhanVienFilterCtrl: [],
    AppsCheckbox: new FormArray([]),
  });
  listApp: CheckEditAppListByDTO[] = [];
  CompanyCode: string;
  imgFile = '../assets/media/Img/NoImage.jpg';
  // ngx-mat-search area
  NhanViens: JeeHRNhanVien[] = [];
  filterNhanViens: ReplaySubject<JeeHRNhanVien[]> = new ReplaySubject<JeeHRNhanVien[]>();
  private _isLoading$ = new BehaviorSubject<boolean>(false);
  private _isFirstLoading$ = new BehaviorSubject<boolean>(true);
  private _errorMessage$ = new BehaviorSubject<string>('');
  private subscriptions: Subscription[] = [];
  public isLoadingSubmit$: BehaviorSubject<boolean> = new BehaviorSubject<boolean>(false);
  private translate: TranslateService;
  get isLoading$() {
    return this._isLoading$.asObservable();
  }
  get isFirstLoading$() {
    return this._isFirstLoading$.asObservable();
  }
  get errorMessage$() {
    return this.errorMessage$.asObservable();
  }
  // End
  constructor(
    @Inject(MAT_DIALOG_DATA) public data: any,
    public dialogRef: MatDialogRef<AccountManagementChinhSuaJeeHRDialogComponent>,
    private fb: FormBuilder,
    public accountManagementService: AccountManagementService,
    private changeDetect: ChangeDetectorRef,
    private layoutUtilsService: LayoutUtilsService,
    public danhmuc: DanhMucChungService,
    public datepipe: DatePipe,
    private translateService: TranslateService
  ) {}

  ngOnDestroy(): void {
    this.accountManagementService.ngOnDestroy();
    this.subscriptions.forEach((sb) => sb.unsubscribe());
  }

  createForm() {
    this.itemForm = this.fb.group({
      NhanVien: ['', [Validators.required]],
      NhanVienFilterCtrl: [],
      AppsCheckbox: new FormArray([]),
    });
  }

  ngOnInit(): void {
    this._isFirstLoading$.next(true);
    if (this.data.item) {
      this.item = new AccountManagementModel();
      this.itemData = this.data.item;
      this.userid = this.itemData.UserId;
      this.initItemData();
    } else {
      this.item = new AccountManagementModel();
    }
    const sb = this.accountManagementService
      .GetEditListAppByUserIDByListCustomerId(this.userid)
      .pipe(
        tap((res: ResultModel<CheckEditAppListByDTO>) => {
          if (res) {
            this.listApp = res.data;
            this.addCheckboxes();
            this.initItemListApp();
          }
        }),
        finalize(() => {
          this._isFirstLoading$.next(true);
          this.loadNhanVienJeeHR();
        }),
        catchError((err) => {
          console.log(err);
          this._errorMessage$.next(err);
          return of();
        })
      )
      .subscribe();
    this.subscriptions.push(sb);
    const sb4 = this.danhmuc
      .getCompanyCode()
      .pipe(
        tap((res) => {
          this.CompanyCode = res.CompanyCode;
        }),
        finalize(() => {
          this._isLoading$.next(false);
        }),
        catchError((err) => {
          console.log(err);
          this._errorMessage$.next(err);
          return of();
        })
      )
      .subscribe();
    this.subscriptions.push(sb4);
  }

  GetCommonInfo(userid: number) {
    const sb5 = this.danhmuc
      .GetCommonAccount(userid)
      .pipe(
        tap((res) => {
          this.staffid = res.StaffID;
          this.itemForm.controls.NhanVien.patchValue(this.staffid);
        }),
        finalize(() => {
          this._isLoading$.next(false);
        }),
        catchError((err) => {
          console.log(err);
          this._errorMessage$.next(err);
          return of();
        })
      )
      .subscribe();
    this.subscriptions.push(sb5);
  }

  initItemData() {
    this.item.Birthday = this.itemData.NgaySinh;
    this.item.Departmemt = this.itemData.Department;
    this.item.DepartmemtID = this.itemData.DepartmentID;
    this.item.Email = this.itemData.Email;
    this.item.Fullname = this.itemData.FullName;
    this.item.ImageAvatar = this.itemData.AvartarImgURL;
    this.item.Jobtitle = this.itemData.Jobtitle;
    this.item.JobtitleID = this.itemData.JobtitleID;
    this.item.Phonemumber = this.itemData.PhoneNumber;
    this.item.Username = this.itemData.Username;
  }
  initItemListApp() {
    let lstUserd = this.listApp.filter((item) => item.IsUsed);
    let appId = lstUserd.map((item) => item.AppID);
    let appCode = lstUserd.map((item) => item.AppCode);
    this.item.AppID = appId;
    this.item.AppCode = appCode;
  }

  get AppsFromArray() {
    return this.itemForm.get('AppsCheckbox') as FormArray;
  }

  private addCheckboxes() {
    this.listApp.forEach((item) => this.AppsFromArray.push(new FormControl(item.IsUsed)));
  }

  loadNhanVienJeeHR() {
    const sb3 = this.accountManagementService
      .GetDSJeeHRToUpdate()
      .pipe(
        tap((res) => {
          this.NhanViens = [...res];
          this.filterNhanViens.next([...res]);
          this.itemForm.controls.NhanVienFilterCtrl.valueChanges.subscribe(() => {
            this.profilterNhanViens();
          });
          this.GetCommonInfo(this.userid);
        }),
        finalize(() => {
          this._isFirstLoading$.next(false);
          this._isLoading$.next(false);
        }),
        catchError((err) => {
          console.log(err);
          this._errorMessage$.next(err);
          return of();
        })
      )
      .subscribe();
    this.subscriptions.push(sb3);
  }

  protected profilterNhanViens() {
    if (!this.itemForm.controls.NhanVien) {
      return;
    }
    let search = this.itemForm.controls.NhanVienFilterCtrl.value;
    if (!search) {
      this.filterNhanViens.next([...this.NhanViens]);
      return;
    } else {
      search = search.toLowerCase();
    }
    this.filterNhanViens.next(this.NhanViens.filter((item) => item.HoTen.toLowerCase().indexOf(search) > -1));
  }

  onSubmit(withBack: boolean) {
    if (this.itemForm.valid) {
      const acc = this.prepareDataFromFB();
      this.update(acc, withBack);
    } else {
      this.validateAllFormFields(this.itemForm);
    }
  }

  prepareDataFromFB(): AccountManagementModel {
    const acc = new AccountManagementModel();
    const AppCode: string[] = [];
    const AppID: number[] = [];
    for (let index = 0; index < this.AppsFromArray.controls.length; index++) {
      if (this.AppsFromArray.controls[index].value === true) {
        AppCode.push(this.listApp[index].AppCode);
        AppID.push(this.listApp[index].AppID);
      }
    }
    acc.AppCode = AppCode;
    acc.AppID = AppID;
    if (this.itemForm.controls.NhanVien.value) {
      const indexNhanvien = +this.itemForm.controls.NhanVien.value;
      const nhanvien = this.NhanViens.find((item) => item.IDNV == indexNhanvien);
      acc.Fullname = nhanvien.HoTen;
      acc.Email = nhanvien.Email;
      acc.Phonemumber = '';
      acc.DepartmemtID = nhanvien.structureid;
      if (nhanvien.structureid != 0) acc.Departmemt = nhanvien.Structure;
      acc.JobtitleID = nhanvien.jobtitleid;
      if (nhanvien.jobtitleid != 0) acc.Jobtitle = nhanvien.TenChucVu;
      acc.ImageAvatar = nhanvien.avatar;
      acc.Birthday = nhanvien.NgaySinh;
    }
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
  update(acc: AccountManagementModel, withBack: boolean = false) {
    this.isLoadingSubmit$.next(true);
    this.accountManagementService
      .UpdateAccount(acc)
      .pipe(
        tap((res) => {
          this.isLoadingSubmit$.next(false);
          if (withBack) {
            this.dialogRef.close(res);
          }
        })
      )
      .subscribe(
        () => {},
        (error) => {
          this.layoutUtilsService.showActionNotification(error.error.message, MessageType.Read, 999999999, true, false, 3000, 'top', 0);
          console.log(error);
          this.isLoadingSubmit$.next(false);
          this._errorMessage$.next(error);
        }
      );
  }
  goBack() {
    if (this.checkDataBeforeClose()) {
      this.dialogRef.close();
    } else {
      const _title = this.translateService.instant('CHECKPOPUP.TITLE');
      const _description = this.translateService.instant('CHECKPOPUP.DESCRIPTION');
      const _waitDesciption = this.translateService.instant('CHECKPOPUP.WAITDESCRIPTION');
      const popup = this.layoutUtilsService.deleteElement(_title, _description, _waitDesciption);
      popup.afterClosed().subscribe((res) => {
        res ? this.dialogRef.close() : undefined;
      });
    }
  }

  format_date(value: any, args?: any): any {
    let latest_date = this.datepipe.transform(value, 'dd/MM/yyyy');
    return latest_date;
  }

  checkDataBeforeClose(): boolean {
    const model = this.prepareDataFromFB();
    if (this.item.Username === '') {
      const empty = new AccountManagementModel();
      empty.Username = `${this.CompanyCode}.`;
      empty.AppCode = this.listApp.filter((item) => item.IsUsed).map((item) => item.AppCode);
      empty.AppID = this.listApp.filter((item) => item.IsUsed).map((item) => item.AppID);
      return this.danhmuc.isEqual(empty, model);
    }
    return this.danhmuc.isEqual(model, this.item);
  }

  @HostListener('window:beforeunload', ['$event'])
  beforeunloadHandler(e) {
    if (!this.checkDataBeforeClose()) {
      e.preventDefault(); //for Firefox
      return (e.returnValue = ''); //for Chorme
    }
  }
}
