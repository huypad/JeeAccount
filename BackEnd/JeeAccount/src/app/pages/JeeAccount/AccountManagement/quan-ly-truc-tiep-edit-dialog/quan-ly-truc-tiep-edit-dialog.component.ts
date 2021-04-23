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
import { ResultModel } from '../../_core/models/_base.model';
import { AppListDTO } from '../../AccountManagement/Model/account-management.model';
import { DanhMucChungService } from '../../_core/services/danhmuc.service';
import { BehaviorSubject, Observable, ReplaySubject } from 'rxjs';
import { COMMA, ENTER } from '@angular/cdk/keycodes';
import { MatAutocomplete, MatAutocompleteSelectedEvent } from '@angular/material/autocomplete';
import { map, startWith } from 'rxjs/operators';
import { MatChipInputEvent } from '@angular/material/chips';
import { MatSelect } from '@angular/material/select';
import { LayoutUtilsService, MessageType } from '../../_core/utils/layout-utils.service';
import { NhanVienMatchip } from '../../_core/models/danhmuc.model';
import { AccountManagementService } from '../Services/account-management.service';

@Component({
  selector: 'app-quan-ly-truc-tiep-edit-dialog',
  templateUrl: './quan-ly-truc-tiep-edit-dialog.component.html',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class QuanLytrucTiepEditDialogComponent implements OnInit {
  item: any = [];
  itemForm = this.fb.group({
    QuanLyNhom: ['' + this.item],
    FilterQuanLyNhom: [],
  });
  // ngx-mat-search area
  quanLys: NhanVienMatchip[] = [];
  filterQuanLys: ReplaySubject<NhanVienMatchip[]> = new ReplaySubject<NhanVienMatchip[]>();
  // End
  constructor(
    @Inject(MAT_DIALOG_DATA) public data: any,
    public dialogRef: MatDialogRef<QuanLytrucTiepEditDialogComponent>,
    private fb: FormBuilder,
    private accountManagementService: AccountManagementService,
    private changeDetect: ChangeDetectorRef,
    private danhmucService: DanhMucChungService,
    private layoutUtilsService: LayoutUtilsService
  ) {}

  ngOnInit(): void {
    this.item = this.data.item;
    this.danhmucService.GetMatchipNhanVien().subscribe((res: ResultModel<NhanVienMatchip>) => {
      if (res && res.status === 1) {
        // ngx
        this.quanLys = [...res.data];
        this.filterQuanLys.next([...res.data]);
        // listen for search field value changes
        this.itemForm.controls.FilterQuanLyNhom.valueChanges.subscribe(() => {
          this.filterBanks();
        });
      }
    });
  }
  onSubmit() {
    if (this.itemForm.valid) {
      if (this.item) {
      }
      const depart = this.initDataFromFB();
    } else {
      this.validateAllFormFields(this.itemForm);
    }
  }
  update() {}

  initDataFromFB(): any {}

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
    // filter the banks
    this.filterQuanLys.next(this.quanLys.filter((item) => item.Display.toLowerCase().indexOf(search) > -1));
  }
}
