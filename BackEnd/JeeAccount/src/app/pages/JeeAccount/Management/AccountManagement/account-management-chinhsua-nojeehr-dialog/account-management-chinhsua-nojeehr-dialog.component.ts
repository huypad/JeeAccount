import { CheckEditAppListByDTO } from './../Model/account-management.model';
import { SelectModel } from '../../../_shared/jee-search-form/jee-search-form.model';
import { ChangeDetectionStrategy, ChangeDetectorRef, Component, Inject, OnInit, OnDestroy, HostListener } from '@angular/core';
import { MatDialog, MatDialogRef, MAT_DIALOG_DATA } from '@angular/material/dialog';
import { FormArray, FormBuilder, FormControl, FormGroup, Validators } from '@angular/forms';
import { AccountManagementDTO, AccountManagementModel, AppListDTO } from '../Model/account-management.model';
import { AccountManagementService } from '../Services/account-management.service';
import { ReplaySubject, of, BehaviorSubject, Subscription } from 'rxjs';
import { AuthService } from 'src/app/modules/auth/_services/auth.service';
import { LayoutUtilsService, MessageType } from '../../../_core/utils/layout-utils.service';
import { DanhMucChungService } from '../../../_core/services/danhmuc.service';
import { ResultModel } from '../../../_core/models/_base.model';
import { DepartmentManagementDTO } from '../../DepartmentManagement/Model/department-management.model';
import { DatePipe } from '@angular/common';
import { catchError, finalize, tap } from 'rxjs/operators';
import { TranslateService } from '@ngx-translate/core';
import { DeleteEntityDialogComponent } from '../../../_shared/delete-entity-dialog/delete-entity-dialog.component';

@Component({
  selector: 'app-account-management-chinhsua-nojeehr-dialog',
  templateUrl: './account-management-chinhsua-nojeehr-dialog.component.html',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class AccountManagementChinhSuaNoJeeHRDialogComponent implements OnInit, OnDestroy {
  item: AccountManagementModel;
  itemData: AccountManagementDTO;
  itemForm: FormGroup;
  listApp: CheckEditAppListByDTO[] = [];
  CompanyCode: string;
  userid: number = 0;
  // ngx-mat-search area
  phongBans: DepartmentManagementDTO[] = [];
  filterPhongBans: ReplaySubject<DepartmentManagementDTO[]> = new ReplaySubject<DepartmentManagementDTO[]>();
  chucvus: SelectModel[] = [];
  filterChucVus: ReplaySubject<SelectModel[]> = new ReplaySubject<SelectModel[]>();
  newpassword: string;
  private _isLoading$ = new BehaviorSubject<boolean>(false);
  private _isFirstLoading$ = new BehaviorSubject<boolean>(true);
  private _errorMessage$ = new BehaviorSubject<string>('');
  private subscriptions: Subscription[] = [];
  public isLoadingSubmit$: BehaviorSubject<boolean> = new BehaviorSubject<boolean>(false);
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
    public dialogRef: MatDialogRef<AccountManagementChinhSuaNoJeeHRDialogComponent>,
    public fb: FormBuilder,
    public accountManagementService: AccountManagementService,
    private layoutUtilsService: LayoutUtilsService,
    public danhmuc: DanhMucChungService,
    public datepipe: DatePipe,
    public cd: ChangeDetectorRef,
    private translate: TranslateService,
    public dialog: MatDialog
  ) {}

  ngOnDestroy(): void {
    this.accountManagementService.ngOnDestroy();
    this.subscriptions.forEach((sb) => sb.unsubscribe());
  }

  initDataToForm() {
    this.itemForm.controls.HoTen.patchValue(this.item.Fullname);
    this.itemForm.controls.Email.patchValue(this.item.Email);
    this.itemForm.controls.SoDienThoai.patchValue(this.item.Phonemumber);
    if (this.item.Birthday) this.itemForm.controls['BirthDay'].patchValue(this.danhmuc.f_string_date(this.item.Birthday));
  }
  createForm() {
    this.itemForm = this.fb.group({
      HoTen: ['' + this.item.Fullname, Validators.compose([Validators.required, Validators.minLength(3), Validators.maxLength(100)])],
      Email: ['', Validators.compose([Validators.email])],
      PhongBan: ['', [Validators.required]],
      SoDienThoai: ['', Validators.compose([Validators.pattern(/^-?(0|[0-9]\d*)?$/)])],
      AppsCheckbox: new FormArray([]),
      PhongBanFilterCtrl: [],
      Chucvu: ['', [Validators.required]],
      ChucVuFilterCtrl: [],
      BirthDay: [''],
    });
    this.initDataToForm();
  }
  ngOnInit(): void {
    this._isFirstLoading$.next(true);
    if (this.data.item) {
      this.item = new AccountManagementModel();
      this.itemData = this.data.item;
      this.userid = this.itemData.UserId;
      this.initItemData();
      this.getPassword();
    } else {
      this.item = new AccountManagementModel();
    }

    this.createForm();
    const sb = this.accountManagementService
      .GetEditListAppByUserIDByListCustomerId(this.userid)
      .pipe(
        tap((res: ResultModel<CheckEditAppListByDTO>) => {
          if (res) {
            const index = res.data.findIndex((item) => item.AppID === 14);
            res.data.splice(index, 1);
            this.listApp = res.data;
            this.accountManagementService.GetListAppByCustomerID().subscribe(data => {
              const lstApp = data.data;
              lstApp.forEach(app => {
                let isExist = false;
                this.listApp.forEach(item => {
                  if (item.AppID === app.AppID) {
                    isExist = true;
                  }
                });
                if (!isExist && app.AppID !== 14) {
                  const noApp: CheckEditAppListByDTO = { AppID: app.AppID, AppCode : app.AppCode,
                    AppName : app.AppName, Disable : false, IsUsed : false };
                  this.listApp.push(noApp);
                }
              });
              this.addCheckboxes();
              this.initItemListApp();
            });
          }
        }),
        finalize(() => {
          this.loadPhongBan();
          this.loadChucVu();
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

  getPassword() {
    var out = [];
    for (var i = 0; i < this.itemData.Username.length; i++) {
      out.push('*');
    }
    this.newpassword = out.join('');
  }
  resetPassword() {
    let saveMessageTranslateParam = 'COMMOM.MATKHAUMOI';
    let saveMessage = this.translate.instant(saveMessageTranslateParam);
    const messageType = MessageType.Create;
    const _title = this.translate.instant('COMMOM.RESETMATKHAU');
    const _description = this.translate.instant('COMMOM.RESETMATKHAUDESCRIPTION');
    const dialogRef = this.dialog.open(DeleteEntityDialogComponent, {
      data: { description: _description, title: _title },
      width: '450px',
    });
    const sb = dialogRef.afterClosed().subscribe((res) => {
      if (res) {
        this.accountManagementService.ResetPassword(this.itemData.Username).subscribe(
          (res) => {
            this.newpassword = res.newpassword;
            saveMessage += ': ' + this.newpassword;
            this.cd.detectChanges();
            this.layoutUtilsService.showActionNotification(saveMessage, messageType, 4000, true, false);
          },
          (error) => {
            this.layoutUtilsService.showActionNotification(error.error.message, MessageType.Read, 999999999, true, false, 3000, 'top', 0);
          },
          () => {
            this.accountManagementService.fetch();
          }
        );
      }
    });
    this.subscriptions.push(sb);
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

  loadChucVu() {
    const sb3 = this.danhmuc
      .getDSChucvu()
      .pipe(
        tap((res) => {
          this.chucvus = [...res.data];
          this.filterChucVus.next([...res.data]);
          this.itemForm.controls.Chucvu.patchValue(this.itemData.JobtitleID);
          this.itemForm.controls.ChucVuFilterCtrl.valueChanges.subscribe(() => {
            this.profilterChucVus();
          });
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
  loadPhongBan() {
    const sb2 = this.danhmuc
      .getDSPhongBan()
      .pipe(
        tap((res) => {
          this.phongBans = [...res.data.flat];
          this.filterPhongBans.next([...res.data.flat]);
          this.itemForm.controls.PhongBan.patchValue(this.itemData.DepartmentID);
          this.itemForm.controls.PhongBanFilterCtrl.valueChanges.subscribe(() => {
            this.profilterPhongBans();
          });
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
    this.subscriptions.push(sb2);
  }

  protected profilterPhongBans() {
    if (!this.itemForm.controls.PhongBan) {
      return;
    }

    let search = this.itemForm.controls.PhongBanFilterCtrl.value;
    if (!search) {
      this.filterPhongBans.next([...this.phongBans]);
      return;
    } else {
      search = search.toLowerCase();
    }
    this.filterPhongBans.next(this.phongBans.filter((item) => item.DepartmentName.toLowerCase().indexOf(search) > -1));
  }

  protected profilterChucVus() {
    if (!this.itemForm.controls.Chucvu) {
      return;
    }
    let search = this.itemForm.controls.ChucVuFilterCtrl.value;
    if (!search) {
      this.filterChucVus.next([...this.chucvus]);
      return;
    } else {
      search = search.toLowerCase();
    }
    this.filterChucVus.next(this.chucvus.filter((item) => item.Title.toLowerCase().indexOf(search) > -1));
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
    acc.Fullname = this.itemForm.controls.HoTen.value;
    acc.Email = this.itemForm.controls.Email.value;
    acc.Phonemumber = this.itemForm.controls.SoDienThoai.value;
    acc.DepartmemtID = +this.itemForm.controls.PhongBan.value;
    if (acc.DepartmemtID != 0) acc.Departmemt = this.phongBans.find((item) => item.RowID == acc.DepartmemtID).DepartmentName;
    acc.JobtitleID = +this.itemForm.controls.Chucvu.value;
    if (acc.JobtitleID != 0) acc.Jobtitle = this.chucvus.find((item) => item.RowID == acc.JobtitleID).Title;
    acc.ImageAvatar = this.itemData.AvartarImgURL;
    acc.Birthday = this.itemForm.controls.BirthDay.value != undefined ? this.format_date(this.itemForm.controls.BirthDay.value) : '';
    acc.Username = this.itemData.Username;
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

  update(acc: AccountManagementModel, withBack: boolean = false) {
    this.isLoadingSubmit$.next(true);
    const sb = this.accountManagementService
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
    this.subscriptions.push(sb);
  }
  goBack() {
    if (this.checkDataBeforeClose()) {
      this.dialogRef.close();
    } else {
      const _title = this.translate.instant('CHECKPOPUP.TITLE');
      const _description = this.translate.instant('CHECKPOPUP.DESCRIPTION');
      const _waitDesciption = this.translate.instant('CHECKPOPUP.WAITDESCRIPTION');
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
