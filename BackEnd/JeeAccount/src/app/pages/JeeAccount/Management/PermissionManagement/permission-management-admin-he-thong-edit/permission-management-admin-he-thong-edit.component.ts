import { PermissionAdminHeThongService } from './../services/permision-admin-hethong.service';
import { AccountManagementService } from './../../AccountManagement/Services/account-management.service';
import { ChangeDetectionStrategy, ChangeDetectorRef, Component, Inject, OnDestroy, OnInit } from '@angular/core';
import { MatDialogRef, MAT_DIALOG_DATA } from '@angular/material/dialog';
import { FormBuilder, FormControl, FormGroup } from '@angular/forms';
import { ReplaySubject, BehaviorSubject, Subscription, of } from 'rxjs';
import { NhanVienMatchip } from '../../../_core/models/danhmuc.model';
import { DanhMucChungService } from '../../../_core/services/danhmuc.service';
import { LayoutUtilsService, MessageType } from '../../../_core/utils/layout-utils.service';
import { ResultModel } from '../../../_core/models/_base.model';
import { AccDirectManagerModel } from '../../AccountManagement/Model/account-management.model';
import { AccModel } from '../model/permision-management.model';
import { tap, finalize, catchError } from 'rxjs/operators';

@Component({
  selector: 'app-permission-management-admin-he-thong-edit-dialog',
  templateUrl: './permission-management-admin-he-thong-edit.component.html',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class PermissionManagementAdminHeThongEditDialogComponent implements OnInit, OnDestroy {
  itemForm = this.fb.group({
    NhanVien: [],
    FilterNhanVien: [],
  });
  // ngx-mat-search area
  quanLys: NhanVienMatchip[] = [];
  filterQuanLys: ReplaySubject<NhanVienMatchip[]> = new ReplaySubject<NhanVienMatchip[]>();
  isLoadingSubmit$: BehaviorSubject<boolean>;
  isLoading$: BehaviorSubject<boolean>;
  // End
  private subscriptions: Subscription[] = [];

  constructor(
    @Inject(MAT_DIALOG_DATA) public data: any,
    public dialogRef: MatDialogRef<PermissionManagementAdminHeThongEditDialogComponent>,
    private fb: FormBuilder,
    private service: PermissionAdminHeThongService,
    private danhmucService: DanhMucChungService,
    private layoutUtilsService: LayoutUtilsService
  ) {}

  ngOnDestroy(): void {
    this.subscriptions.forEach((sb) => sb.unsubscribe());
    this.service.ngOnDestroy();
  }
  ngOnInit(): void {
    this.isLoadingSubmit$ = new BehaviorSubject(false);
    this.isLoading$ = new BehaviorSubject(true);
    const sb = this.danhmucService.GetMatchipNhanvienNotAdminHeThong().subscribe((res: ResultModel<NhanVienMatchip>) => {
      if (res && res.status === 1) {
        // ngx
        this.quanLys = [...res.data];
        this.filterQuanLys.next([...res.data]);
        // listen for search field value changes
        this.itemForm.controls.FilterNhanVien.valueChanges.subscribe(() => {
          this.filterBanks();
        });
        //  init data
        if (this.data.DirectManager) {
          this.itemForm.controls.NhanVien.setValue(this.data.DirectManager);
        }
        this.isLoading$.next(false);
      }
    });
    this.subscriptions.push(sb);
  }
  onSubmit() {
    if (this.itemForm.valid) {
      const username = this.initDataFromFB();
      if (!username) {
        this.layoutUtilsService.showActionNotification('Bắt buộc chọn nhân viên', MessageType.Read, 999999999, true, false, 3000, 'top', 0);
        return;
      }
      this.create(username);
    } else {
      this.validateAllFormFields(this.itemForm);
    }
  }
  create(item: NhanVienMatchip) {
    this.isLoadingSubmit$.next(true);
    const sb = this.service
      .createAdminHeThong(item)
      .pipe(
        tap((res) => {
          this.dialogRef.close(res);
        }),
        finalize(() => {
          this.isLoadingSubmit$.next(false);
        }),
        catchError((err) => {
          console.log('???');
          this.layoutUtilsService.showActionNotification(err.error.message, MessageType.Read, 999999999, true, false, 3000, 'top', 0);
          return of();
        })
      )
      .subscribe();
    this.subscriptions.push(sb);
  }

  initDataFromFB(): NhanVienMatchip {
    return this.quanLys.find((nv) => nv.Username == this.itemForm.controls.NhanVien.value);
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

  protected filterBanks() {
    if (!this.itemForm.controls.NhanVien) {
      return;
    }
    // get the search keyword
    let search = this.itemForm.controls.FilterNhanVien.value;
    if (!search) {
      this.filterQuanLys.next([...this.quanLys]);
      return;
    } else {
      search = search.toLowerCase();
    }
    // filter
    this.filterQuanLys.next(this.quanLys.filter((item) => item.Display.toLowerCase().indexOf(search) > -1));
  }
}