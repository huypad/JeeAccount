import { SelectModel } from './../../../_shared/jee-search-form/jee-search-form.model';
import { ChangeDetectionStrategy, ChangeDetectorRef, Component, Inject, OnInit, OnDestroy, HostListener } from '@angular/core';
import { MatDialogRef, MAT_DIALOG_DATA } from '@angular/material/dialog';
import { FormArray, FormBuilder, FormControl, FormGroup, Validators } from '@angular/forms';
import { AccountManagementModel, AppListDTO } from '../Model/account-management.model';
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
import { JobtitleManagementDTO } from '../../JobtitleManagement/Model/jobtitle-management.model';

@Component({
  selector: 'app-account-management-edit-dialog',
  templateUrl: './account-management-edit-dialog.component.html',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class AccountManagementEditDialogComponent implements OnInit, OnDestroy {
  item: AccountManagementModel;
  itemForm = this.fb.group({
    AnhDaiDien: [''],
    HoTen: ['', Validators.compose([Validators.required, Validators.minLength(3), Validators.maxLength(100)])],
    Email: ['', Validators.compose([Validators.email])],
    PhongBan: ['', [Validators.required]],
    TenDangNhap: ['', [Validators.required]],
    MatKhau: ['', [Validators.required]],
    NhapLaiMatKhau: ['', [Validators.required]],
    SoDienThoai: ['', Validators.compose([Validators.pattern(/^-?(0|[0-9]\d*)?$/)])],
    AppsCheckbox: new FormArray([]),
    file: [],
    PhongBanFilterCtrl: [],
    Chucvu: ['', [Validators.required]],
    ChucVuFilterCtrl: [],
    BirthDay: [''],
  });
  listApp: AppListDTO[] = [];
  CompanyCode: string;
  imgFile = '../assets/media/Img/NoImage.jpg';
  // ngx-mat-search area
  phongBans: DepartmentManagementDTO[] = [];
  filterPhongBans: ReplaySubject<DepartmentManagementDTO[]> = new ReplaySubject<DepartmentManagementDTO[]>();
  chucvus: JobtitleManagementDTO[] = [];
  filterChucVus: ReplaySubject<JobtitleManagementDTO[]> = new ReplaySubject<JobtitleManagementDTO[]>();
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
    public dialogRef: MatDialogRef<AccountManagementEditDialogComponent>,
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

  ngOnInit(): void {
    this._isFirstLoading$.next(true);
    if (this.data.item) {
      this.item = this.data.item;
    } else {
      this.item = new AccountManagementModel();
    }
    const sb = this.accountManagementService
      .GetListAppByCustomerID()
      .pipe(
        tap((res: ResultModel<AppListDTO>) => {
          if (res) {
            this.listApp = res.data;
            this.addCheckboxes();
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

  get AppsFromArray() {
    return this.itemForm.get('AppsCheckbox') as FormArray;
  }

  private addCheckboxes() {
    this.listApp.forEach((item) => this.AppsFromArray.push(new FormControl(item.IsDefaultApp)));
  }

  private setValueCheckboxes() {
    const lst = this.listApp.map((item) => item.IsDefaultApp);
    this.AppsFromArray.setValue(lst);
  }

  loadChucVu() {
    const sb3 = this.danhmuc
      .getDSChucvu()
      .pipe(
        tap((res) => {
          this.chucvus = [...res.data];
          this.filterChucVus.next([...res.data]);
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
      if (this.itemForm.controls.MatKhau.value !== this.itemForm.controls.NhapLaiMatKhau.value) {
        this.layoutUtilsService.showActionNotification(
          'Mật khẩu không trùng khớp',
          MessageType.Read,
          999999999,
          true,
          false,
          3000,
          'top',
          0
        );
        return;
      }
      const acc = this.prepareDataFromFB();
      console.log(acc);
      //this.create(acc, withBack);
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
    acc.Username = `${this.CompanyCode}.${this.itemForm.controls.TenDangNhap.value}`;
    acc.Password = this.itemForm.controls.MatKhau.value;
    acc.ImageAvatar = this.imgFile ? this.imgFile.split(',')[1] : '';
    acc.Birthday = this.itemForm.controls.BirthDay.value != undefined ? this.format_date(this.itemForm.controls.BirthDay.value) : '';
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
  create(acc: AccountManagementModel, withBack: boolean = false) {
    this.isLoadingSubmit$.next(true);
    const sb = this.accountManagementService
      .createAccount(acc)
      .pipe(
        tap((res) => {
          this.isLoadingSubmit$.next(false);
          if (withBack) {
            this.dialogRef.close(res);
          } else {
            let saveMessageTranslateParam = '';
            saveMessageTranslateParam += 'COMMOM.THEMTHANHCONG';
            const saveMessage = 'Thêm thành công';
            const messageType = MessageType.Create;
            this.layoutUtilsService.showActionNotification(saveMessage, messageType, 4000, true, false);
            this.itemForm.reset();
            this.setValueCheckboxes();
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
      empty.AppCode = this.listApp.filter((item) => item.IsDefaultApp).map((item) => item.AppCode);
      empty.AppID = this.listApp.filter((item) => item.IsDefaultApp).map((item) => item.AppID);
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
