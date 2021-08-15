import { MatSelect } from '@angular/material/select';
import { MatAutocomplete, MatAutocompleteSelectedEvent } from '@angular/material/autocomplete';
import { ChangeDetectionStrategy, Component, ElementRef, Inject, OnDestroy, OnInit, ViewChild, ChangeDetectorRef } from '@angular/core';
import { MatDialogRef, MAT_DIALOG_DATA } from '@angular/material/dialog';
import { FormBuilder, FormControl, FormGroup, Validators } from '@angular/forms';
import { JobtitleManagementService } from '../Sevices/jobtitle-management.service';
import { BehaviorSubject, Observable, Subscription } from 'rxjs';
import { COMMA, ENTER } from '@angular/cdk/keycodes';
import { JobtitleModel } from '../Model/jobtitle-management.model';
import { LayoutUtilsService, MessageType } from '../../../_core/utils/layout-utils.service';
import { NhanVienMatchip } from '../../../_core/models/danhmuc.model';
import { MatChipInputEvent } from '@angular/material/chips';
import { map, startWith } from 'rxjs/operators';
import { DanhMucChungService } from '../../../_core/services/danhmuc.service';
import { ResultModel } from '../../../_core/models/_base.model';

@Component({
  selector: 'app-jobtitle-management-edit-dialog',
  templateUrl: './jobtitle-management-edit-dialog.component.html',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class JobtitleManagementEditDialogComponent implements OnInit, OnDestroy {
  item: JobtitleModel;
  itemForm = this.fb.group({
    TenChucVu: ['', [Validators.required]],
    MoTa: [''],
    ThanhVien: [''],
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
  @ViewChild('thanhVienInput') thanhVienInput: ElementRef<HTMLInputElement>;
  @ViewChild('autoThanhVien') matAutocomplete: MatAutocomplete;
  @ViewChild('singleSelect') singleSelect: MatSelect;
  // ngx-mat-search area
  isLoadingSubmit$: BehaviorSubject<boolean>;
  isLoading$: BehaviorSubject<boolean>;
  private subscriptions: Subscription[] = [];
  // End
  constructor(
    @Inject(MAT_DIALOG_DATA) public data: any,
    public dialogRef: MatDialogRef<JobtitleManagementEditDialogComponent>,
    private fb: FormBuilder,
    private jobtitleManagementService: JobtitleManagementService,
    private layoutUtilsService: LayoutUtilsService,
    public cd: ChangeDetectorRef,
    private danhmucService: DanhMucChungService
  ) {}

  ngOnDestroy(): void {
    this.jobtitleManagementService.ngOnDestroy();
    this.subscriptions.forEach((sb) => sb.unsubscribe());
  }
  ngOnInit(): void {
    this.isLoadingSubmit$ = new BehaviorSubject(false);
    this.isLoading$ = new BehaviorSubject(true);
    this.item = this.data.item;
    if (this.item.RowID > 0) {
      this.isLoading$.next(true);
      const sb2 = this.jobtitleManagementService.GetJobtile(this.item.RowID).subscribe(
        (res) => {
          this.item = res;
          this.thanhViens = res.ThanhVien;
          this.isLoading$.next(false);
          this.cd.detectChanges();
          this.initData();
        },
        (error) => {
          console.log(error);
        }
      );
      this.subscriptions.push(sb2);
      this.initData();
    } else {
      this.item = new JobtitleModel();
    }
    const sb = this.danhmucService.GetMatchipNhanVien().subscribe((res: ResultModel<NhanVienMatchip>) => {
      if (res && res.status === 1) {
        // mat-chip
        this.allThanhviens = res.data;
        this.filteredThanhViens = this.itemForm.controls.ThanhVien.valueChanges.pipe(
          startWith(null),
          map((fruit: string | null) => (fruit ? this._filter(fruit) : this.allThanhviens.slice()))
        );
        this.isLoading$.next(false);
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
    this.itemForm.controls.TenChucVu.patchValue(this.item.JobtitleName);
    this.itemForm.controls.MoTa.patchValue(this.item.Description);
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

  onSubmit() {
    if (this.itemForm.valid) {
      const job = this.initDataFromFB();
      if (this.item.RowID > 0) {
        this.update(job);
      } else {
        this.create(job);
      }
    } else {
      this.validateAllFormFields(this.itemForm);
    }
  }
  update(depart: JobtitleModel) {
    this.isLoadingSubmit$.next(true);
    this.jobtitleManagementService.UpdateDepart(depart).subscribe(
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
  create(jobtitle: JobtitleModel) {
    this.isLoadingSubmit$.next(true);
    this.jobtitleManagementService.createDepart(jobtitle).subscribe((res) => {
      if (res && res.status === 1) {
        this.isLoadingSubmit$.next(false);
        this.dialogRef.close(res.data);
      } else {
        this.isLoadingSubmit$.next(false);
        this.layoutUtilsService.showActionNotification(res.error.message, MessageType.Read, 999999999, true, false, 3000, 'top', 0);
      }
    });
  }

  initDataFromFB(): JobtitleModel {
    const job = new JobtitleModel();
    job.clear();
    job.JobtitleName = this.itemForm.controls.TenChucVu.value;
    job.Description = this.itemForm.controls.MoTa.value;
    if (this.item) {
      job.RowID = this.item.RowID;
    }
    job.ThanhVien = this.thanhViens;
    job.ThanhVienDelete = this.thanhViensDelete;
    return job;
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
