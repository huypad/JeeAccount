import { ChangeDetectionStrategy, ChangeDetectorRef, Component, Inject, OnDestroy, OnInit } from '@angular/core';
import { MatDialogRef, MAT_DIALOG_DATA } from '@angular/material/dialog';
import { FormBuilder, FormControl, FormGroup } from '@angular/forms';
import { ReplaySubject, BehaviorSubject, Subscription } from 'rxjs';
import { NhanVienMatchip } from '../../../_core/models/danhmuc.model';
import { DanhMucChungService } from '../../../_core/services/danhmuc.service';
import { LayoutUtilsService, MessageType } from '../../../_core/utils/layout-utils.service';
import { ResultModel } from '../../../_core/models/_base.model';
import { DepartmentManagementService } from '../Sevices/department-management.service';
import { DepDirectManagerModel } from '../Model/department-management.model';

@Component({
  selector: 'app-department-quan-ly-truc-tiep-edit-dialog',
  templateUrl: './department-quan-ly-truc-tiep-edit-dialog.component.html',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class DepartmentQuanLytrucTiepEditDialogComponent implements OnInit, OnDestroy {
  itemForm = this.fb.group({
    QuanLyNhom: [''],
    FilterQuanLyNhom: [],
  });
  // ngx-mat-search area
  quanLys: NhanVienMatchip[] = [];
  filterQuanLys: ReplaySubject<NhanVienMatchip[]> = new ReplaySubject<NhanVienMatchip[]>();
  isLoadingSubmit$: BehaviorSubject<boolean>;
  isLoading$: BehaviorSubject<boolean>;
  item: DepDirectManagerModel = new DepDirectManagerModel();;
  // End
  private subscriptions: Subscription[] = [];
  constructor(
    @Inject(MAT_DIALOG_DATA) public data: any,
    public dialogRef: MatDialogRef<DepartmentQuanLytrucTiepEditDialogComponent>,
    private fb: FormBuilder,
    private departmentManagementService: DepartmentManagementService,
    private danhmucService: DanhMucChungService,
    private layoutUtilsService: LayoutUtilsService
  ) {}

  ngOnInit(): void {
    this.isLoadingSubmit$ = new BehaviorSubject(false);
    this.isLoading$ = new BehaviorSubject(true);
    if (this.data.DepartmentManager === '') {
      this.item = new DepDirectManagerModel();
    } else {
      this.item.RowID = +this.data.RowID;
      this.item.DepartmentManager = this.data.DepartmentManager;
    }
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
        if (this.data.DepartmentManager) {
          const index = this.quanLys.findIndex((x) => x.Username === this.item.DepartmentManager);
          this.itemForm.controls.QuanLyNhom.setValue(this.quanLys[index].Username);
        }
        this.isLoading$.next(false);
      }
    });
    this.subscriptions.push(sb);
  }
  onSubmit() {
    if (this.itemForm.valid) {
      const directManagement = this.initDataFromFB();
      if (!directManagement.DepartmentManager) {
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
  update(directManagement: DepDirectManagerModel) {
    this.isLoadingSubmit$.next(true);
    const sb = this.departmentManagementService.UpdateDepartmentManager(directManagement).subscribe(
      (res) => {
        this.dialogRef.close(res);
        this.isLoadingSubmit$.next(false);
      },
      (error) => {
        this.layoutUtilsService.showActionNotification(error.error.message, MessageType.Read, 999999999, true, false, 3000, 'top', 0);
        this.isLoadingSubmit$.next(false);
      }
    );
    this.subscriptions.push(sb);
  }

  initDataFromFB(): DepDirectManagerModel {
    const directManagement = new DepDirectManagerModel();
    directManagement.clear();
    directManagement.RowID = this.data.RowID;
    directManagement.DepartmentManager = this.itemForm.controls.QuanLyNhom.value;
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
  ngOnDestroy(): void {
    this.departmentManagementService.ngOnDestroy();
    this.subscriptions.forEach((sb) => sb.unsubscribe());
  }
}
