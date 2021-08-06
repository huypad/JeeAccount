import {
  TreeJeeHRDepartmentDTO,
  FlatJeeHRDepartmentDTO,
  DepartmentManagementDTO,
} from './../../Management/DepartmentManagement/Model/department-management.model';
import { showSearchFormModel } from './jee-search-form.model';
import { BehaviorSubject, of, Subject, Subscription } from 'rxjs';
import { Component, OnInit, ChangeDetectionStrategy, ChangeDetectorRef, ElementRef, Output, EventEmitter, Input } from '@angular/core';
import { JeeSearchFormService } from './jee-search-form.service';
import { catchError, debounceTime, distinctUntilChanged, tap } from 'rxjs/operators';
import { FormBuilder, FormGroup } from '@angular/forms';
import { DepartmentManagement } from '../../Management/DepartmentManagement/Model/department-management.model';

@Component({
  selector: 'app-jee-search-form',
  templateUrl: './jee-search-form.component.html',
  styleUrls: ['jee-search-form.component.scss'],
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class JeeSearchFormComponent implements OnInit {
  private _isLoading$ = new BehaviorSubject<boolean>(false);
  private _errorMessage$ = new BehaviorSubject<string>('');
  private subscriptions: Subscription[] = [];
  searchGroup: FormGroup;
  filterGroup: FormGroup;
  @Input() showSearch?: showSearchFormModel = new showSearchFormModel();
  @Output() keywordEvent: EventEmitter<string> = new EventEmitter<string>();
  @Output() filterEvent: EventEmitter<any> = new EventEmitter<any>();
  public isAdmin: boolean = false;
  public daKhoa: boolean = false;
  public showFilter: boolean = false;
  public isJeeHR: boolean;
  public isTree: boolean;
  public treeJeeHR: TreeJeeHRDepartmentDTO;
  public flatJeeHRs: FlatJeeHRDepartmentDTO[];
  public flatDepartmentDtos: DepartmentManagementDTO[];
  public clickSelection: boolean = true;
  get isLoading$() {
    return this._isLoading$.asObservable();
  }
  get errorMessage$() {
    return this._errorMessage$.asObservable();
  }

  constructor(
    public service: JeeSearchFormService,
    public cd: ChangeDetectorRef,
    private elementRef: ElementRef,
    private fb: FormBuilder
  ) {}

  ngOnInit() {
    this._isLoading$.next(true);
    const sb = this.service
      .getDSPhongBan()
      .pipe(
        tap((res) => {
          console.log(res);
          this.initDataPhongBanId(res.data);
          this.searchForm();
          this.filterForm();
          this._isLoading$.next(false);
        }),
        catchError((err) => {
          console.log(err);
          this._errorMessage$.next(err);
          return of();
        })
      )
      .subscribe();
    this.subscriptions.push(sb);
  }

  initDataPhongBanId(data: DepartmentManagement) {
    this.isJeeHR = data.isJeeHR;
    this.isTree = data.isTree;
    if (this.isJeeHR) {
      this.treeJeeHR = data.tree;
      this.flatJeeHRs = data.flat;
    } else {
      this.flatDepartmentDtos = data.flat;
      console.log(this.flatDepartmentDtos);
    }
  }
  ngOnDestroy(): void {}
  // search
  searchForm() {
    this.searchGroup = this.fb.group({
      keyword: [''],
    });
    const searchEvent = this.searchGroup.controls['keyword'].valueChanges
      .pipe(
        /*
      The user can type quite quickly in the input box, and that could trigger a lot of server requests. With this operator,
      we are limiting the amount of server requests emitted to a maximum of one every 150ms
      */
        debounceTime(150),
        distinctUntilChanged()
      )
      .subscribe((val) => {
        this.search(val);
      });
    this.subscriptions.push(searchEvent);
  }

  search(val) {
    this.keywordEvent.emit(val);
  }

  filterForm() {
    this.filterGroup = this.fb.group({
      keyword: [''],
      username: [''],
      tennhanvien: [''],
      phongban: [''],
      phongbanid: [''],
      chucvu: [''],
      chucvuid: [''],
    });
  }

  clickSearch() {
    const filter = {};
    filter['keyword'] = this.filterGroup.controls['keyword'].value;
    filter['username'] = this.filterGroup.controls['username'].value;
    filter['tennhanvien'] = this.filterGroup.controls['tennhanvien'].value;
    filter['phongban'] = this.filterGroup.controls['phongban'].value;
    filter['chucvu'] = this.filterGroup.controls['chucvu'].value;
    filter['isadmin'] = this.isAdmin;
    filter['dakhoa'] = this.daKhoa;
    filter['phongbanid'] = this.filterGroup.controls['phongbanid'].value;
    console.log(filter);
    this.filterEvent.emit(filter);
  }

  clickShowFilter(val: boolean) {
    this.showFilter = !val;
  }
  clearFilter() {
    this.searchGroup.reset();
    this.filterGroup.reset();
    this.clickSearch();
    this.showFilter = false;
  }

  clickOutSideFilter() {
    console.log('why');
    if (!this.clickSelection) this.showFilter = false;
    setTimeout(() => {
      this.clickSelection = false;
    }, 3000);
  }
  clickSelect() {
    console.log('click');
    this.clickSelection = true;
  }
}
