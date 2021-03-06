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
import { DepartmentManagementService } from '../Sevices/department-management.service';
import { AppListDTO } from '../../AccountManagement/Model/account-management.model';
import { BehaviorSubject, Observable, ReplaySubject, Subscription } from 'rxjs';
import { COMMA, ENTER } from '@angular/cdk/keycodes';
import { MatAutocomplete, MatAutocompleteSelectedEvent } from '@angular/material/autocomplete';
import { map, startWith } from 'rxjs/operators';
import { MatChipInputEvent } from '@angular/material/chips';
import { MatSelect } from '@angular/material/select';
import { DepartmentManagement, DepartmentManagementDTO, DepartmentModel } from '../Model/department-management.model';
import { NhanVienMatchip } from '../../../_core/models/danhmuc.model';
import { DanhMucChungService } from '../../../_core/services/danhmuc.service';
import { LayoutUtilsService, MessageType } from '../../../_core/utils/layout-utils.service';
import { ResultModel } from '../../../_core/models/_base.model';

@Component({
  selector: 'app-department-management-edit-dialog',
  templateUrl: './department-management-edit-dialog.component.html',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class DepartmentManagementEditDialogComponent implements OnInit {
  item: any;
  itemForm = this.fb.group({
    TenPhongBan: ['', [Validators.required]],
    QuanLyNhom: [''],
    FilterQuanLyNhom: [],
    ThanhVien: [''],
    MoTa: [''],
  });
  // Mat chip area
  visible = true;
  selectable = true;
  removable = true;
  separatorKeysCodes: number[] = [ENTER, COMMA];
  filteredThanhViens: Observable<NhanVienMatchip[]>;
  thanhViens: string[] = [];
  thanhViensDelete: string[] = [];
  allThanhviens: NhanVienMatchip[] = [];
  // End
  // ngx-mat-search area
  quanLys: NhanVienMatchip[] = [];
  filterQuanLys: ReplaySubject<NhanVienMatchip[]> = new ReplaySubject<NhanVienMatchip[]>();
  isLoadingSubmit$: BehaviorSubject<boolean>;
  isLoading$: BehaviorSubject<boolean>;
  private subscriptions: Subscription[] = [];
  // End
  @ViewChild('thanhVienInput') thanhVienInput: ElementRef<HTMLInputElement>;
  @ViewChild('autoThanhVien') matAutocomplete: MatAutocomplete;
  @ViewChild('singleSelect') singleSelect: MatSelect;
  constructor(
    @Inject(MAT_DIALOG_DATA) public data: any,
    public dialogRef: MatDialogRef<DepartmentManagementEditDialogComponent>,
    private fb: FormBuilder,
    private departmentManagementService: DepartmentManagementService,
    private danhmucService: DanhMucChungService,
    private layoutUtilsService: LayoutUtilsService,
    public cd: ChangeDetectorRef
  ) {}

  ngOnInit(): void {
    this.isLoadingSubmit$ = new BehaviorSubject(false);
    this.isLoading$ = new BehaviorSubject(true);
    this.item = this.data.item;
    if (this.item.RowID > 0) {
      this.isLoading$.next(true);
      const sb2 = this.departmentManagementService.GetDepart(this.item.RowID).subscribe(
        (res) => {
          this.thanhViens = res.ThanhVien;
          this.isLoading$.next(false);
          this.cd.detectChanges();
        },
        (error) => {
          console.log(error);
        }
      );
      this.subscriptions.push(sb2);
    }
    const sb = this.danhmucService.GetMatchipNhanVien().subscribe((res: ResultModel<NhanVienMatchip>) => {
      if (res && res.status === 1) {
        // mat-chip
        this.allThanhviens = res.data;
        this.filteredThanhViens = this.itemForm.controls.ThanhVien.valueChanges.pipe(
          startWith(null),
          map((fruit: string | null) => (fruit ? this._filter(fruit) : this.allThanhviens.slice()))
        );
        // ngx search from
        this.quanLys = [...res.data];
        this.filterQuanLys.next([...res.data]);
        // listen for search field value changes
        this.itemForm.controls.QuanLyNhom.patchValue(this.item.DepartmentManagerUsername);
        this.itemForm.controls.FilterQuanLyNhom.valueChanges.subscribe(() => {
          this.filterBanks();
        });
        this.isLoading$.next(false);
        this.initData();
        this.cd.detectChanges();
      }
    });
    this.subscriptions.push(sb);
    const sb3 = this.isLoading$.subscribe((res) => {
      if (!res) {
        if (this.thanhViens) {
          this.allThanhviens = this.allThanhviens.filter((item) => !this.thanhViens.includes(item.Username));
          this.filteredThanhViens = this.itemForm.controls.ThanhVien.valueChanges.pipe(
            startWith(null),
            map((fruit: string | null) => (fruit ? this._filter(fruit) : this.allThanhviens.slice()))
          );
        } else {
          this.thanhViens = [];
        }
      }
    });
    this.subscriptions.push(sb3);
  }

  initData() {
    this.itemForm.controls.TenPhongBan.patchValue(this.item.DepartmentName);
    this.itemForm.controls.MoTa.patchValue(this.item.Description);
  }
  onSubmit() {
    if (this.itemForm.valid) {
      const depart = this.initDataFromFB();
      if (this.item.RowID > 0) {
        this.update(depart);
      } else {
        this.create(depart);
      }
    } else {
      this.validateAllFormFields(this.itemForm);
    }
  }

  create(depart: DepartmentModel) {
    this.isLoadingSubmit$.next(true);
    this.departmentManagementService.createDepart(depart).subscribe((res) => {
      if (res && res.status === 1) {
        this.isLoadingSubmit$.next(false);
        this.dialogRef.close(res.data);
      } else {
        this.isLoadingSubmit$.next(false);
        this.layoutUtilsService.showActionNotification(res.error.message, MessageType.Read, 999999999, true, false, 3000, 'top', 0);
      }
    });
  }

  update(depart: DepartmentModel) {
    this.isLoadingSubmit$.next(true);
    this.departmentManagementService.UpdateDepart(depart).subscribe(
      (res) => {
        this.isLoadingSubmit$.next(false);
        this.dialogRef.close(res);
      },
      (error) => {
        this.isLoadingSubmit$.next(false);
        this.layoutUtilsService.showActionNotification(error.error.message, MessageType.Read, 999999999, true, false, 3000, 'top', 0);
      }
    );
  }

  initDataFromFB(): DepartmentModel {
    const depart = new DepartmentModel();
    depart.clear();
    depart.DepartmentManager = this.itemForm.controls.QuanLyNhom.value;
    depart.DepartmentName = this.itemForm.controls.TenPhongBan.value;
    depart.Description = this.itemForm.controls.MoTa.value;
    if (this.item) {
      depart.RowID = this.item.RowID;
    }
    depart.ThanhVien = this.thanhViens;
    depart.ThanhVienDelete = this.thanhViensDelete;
    return depart;
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

  add(event: MatChipInputEvent): void {
    const input = event.input;
    const value = event.value;
    if (value) {
      this.thanhViens.push(value);
      const index = this.thanhViensDelete.indexOf(value);
      if (index >= 0) {
        this.thanhViensDelete.splice(index, 1);
      }
    }
    if (input) {
      input.value = '';
    }
    this.itemForm.controls.ThanhVien.setValue(null);
  }
  remove(username: string): void {
    const index = this.thanhViens.indexOf(username);
    if (index >= 0) {
      this.thanhViens.splice(index, 1);
      this.thanhViensDelete.push(username);
    }
  }
  selected(event: MatAutocompleteSelectedEvent): void {
    this.thanhViens.push(event.option.value);
    this.thanhVienInput.nativeElement.value = '';
    this.itemForm.controls.ThanhVien.setValue(null);
  }
  private _filter(value: string): NhanVienMatchip[] {
    const filterValue = value.toLowerCase();
    return this.allThanhviens.filter((fruit) => fruit.Display.toLowerCase().indexOf(filterValue) === 0);
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
    // filter the banks
    this.filterQuanLys.next(this.quanLys.filter((item) => item.Display.toLowerCase().indexOf(search) > -1));
  }

  ngOnDestroy(): void {
    this.departmentManagementService.ngOnDestroy();
    this.subscriptions.forEach((sb) => sb.unsubscribe());
  }
}
