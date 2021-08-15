import { ChangeDetectionStrategy, ChangeDetectorRef, Component, Inject, OnDestroy, OnInit } from '@angular/core';
import { MatDialogRef, MAT_DIALOG_DATA } from '@angular/material/dialog';
import { FormBuilder, FormControl, FormGroup } from '@angular/forms';
import { ReplaySubject, BehaviorSubject, Subscription } from 'rxjs';
import { AccountManagementService } from '../Services/account-management.service';
import { AccDirectManagerModel } from '../Model/account-management.model';
import { NhanVienMatchip } from '../../../_core/models/danhmuc.model';
import { DanhMucChungService } from '../../../_core/services/danhmuc.service';
import { LayoutUtilsService, MessageType } from '../../../_core/utils/layout-utils.service';
import { ResultModel } from '../../../_core/models/_base.model';

@Component({
  selector: 'app-quan-ly-truc-tiep-edit-dialog',
  templateUrl: './quan-ly-truc-tiep-edit-dialog.component.html',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class QuanLytrucTiepEditDialogComponent implements OnInit, OnDestroy {
  itemForm = this.fb.group({
    QuanLyNhom: [],
    FilterQuanLyNhom: [],
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
    public dialogRef: MatDialogRef<QuanLytrucTiepEditDialogComponent>,
    private fb: FormBuilder,
    private accountManagementService: AccountManagementService,
    private changeDetect: ChangeDetectorRef,
    private danhmucService: DanhMucChungService,
    private layoutUtilsService: LayoutUtilsService
  ) {}

  ngOnDestroy(): void {
    this.subscriptions.forEach((sb) => sb.unsubscribe());
    this.accountManagementService.ngOnDestroy();
  }
  ngOnInit(): void {
    this.isLoadingSubmit$ = new BehaviorSubject(false);
    this.isLoading$ = new BehaviorSubject(true);
    const sb = this.danhmucService.GetMatchipNhanVien().subscribe((res: ResultModel<NhanVienMatchip>) => {
      if (res && res.status === 1) {
        // ngx
        this.quanLys = [...res.data];
        this.filterQuanLys.next([...res.data]);
        // listen for search field value changes
        this.itemForm.controls.FilterQuanLyNhom.valueChanges.subscribe(() => {
          this.filterBanks();
        });
        //  init data
        if (this.data.DirectManager) {
          this.itemForm.controls.QuanLyNhom.setValue(this.data.DirectManager);
        }
        this.isLoading$.next(false);
      }
    });
    this.subscriptions.push(sb);
  }
  onSubmit() {
    if (this.itemForm.valid) {
      const directManagement = this.initDataFromFB();
      if (!directManagement.DirectManager) {
        this.layoutUtilsService.showActionNotification(
          'Bắt buộc chọn quản lý trực tiếp',
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
      this.update(directManagement);
    } else {
      this.validateAllFormFields(this.itemForm);
    }
  }
  update(directManagement: AccDirectManagerModel) {
    this.isLoadingSubmit$.next(true);
    const sb = this.accountManagementService.UpdateDirectManager(directManagement).subscribe((res) => {
      if (res && res.status === 1) {
        this.dialogRef.close(res);
      } else {
        this.layoutUtilsService.showActionNotification(res.error.message, MessageType.Read, 999999999, true, false, 3000, 'top', 0);
      }
      this.isLoadingSubmit$.next(false);
    });
    this.subscriptions.push(sb);
  }

  initDataFromFB(): AccDirectManagerModel {
    const directManagement = new AccDirectManagerModel();
    directManagement.clear();
    directManagement.Username = this.data.Username;
    directManagement.DirectManager = this.itemForm.controls.QuanLyNhom.value;
    return directManagement;
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
    if (!this.itemForm.controls.QuanLyNhom) {
      return;
    }
    // get the search keyword
    let search = this.itemForm.controls.FilterQuanLyNhom.value;
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
