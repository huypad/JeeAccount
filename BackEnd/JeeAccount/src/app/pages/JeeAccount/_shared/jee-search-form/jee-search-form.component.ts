import { TranslateService } from '@ngx-translate/core';
import { DanhMucChungService } from './../../_core/services/danhmuc.service';
import {
  TreeJeeHRDepartmentDTO,
  FlatJeeHRDepartmentDTO,
  DepartmentManagementDTO,
} from './../../Management/DepartmentManagement/Model/department-management.model';
import { showSearchFormModel, SelectModel } from './jee-search-form.model';
import { BehaviorSubject, of, Subject, Subscription } from 'rxjs';
import {
  Component,
  OnInit,
  ChangeDetectionStrategy,
  ChangeDetectorRef,
  ElementRef,
  Output,
  EventEmitter,
  Input,
  OnDestroy,
} from '@angular/core';
import { JeeSearchFormService } from './jee-search-form.service';
import { catchError, debounceTime, distinctUntilChanged, tap, finalize } from 'rxjs/operators';
import { FormBuilder, FormGroup } from '@angular/forms';
import { DepartmentManagement } from '../../Management/DepartmentManagement/Model/department-management.model';

@Component({
  selector: 'app-jee-search-form',
  templateUrl: './jee-search-form.component.html',
  styleUrls: ['jee-search-form.component.scss'],
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class JeeSearchFormComponent implements OnInit, OnDestroy {
  private _isLoading$ = new BehaviorSubject<boolean>(false);
  private _errorMessage$ = new BehaviorSubject<string>('');
  private subscriptions: Subscription[] = [];
  searchGroup: FormGroup;
  filterGroup: FormGroup;
  @Input() showSearch?: showSearchFormModel = new showSearchFormModel();
  @Output() keywordEvent: EventEmitter<string> = new EventEmitter<string>();
  @Output() filterEvent: EventEmitter<any> = new EventEmitter<any>();
  isAdmin: boolean = false;
  daKhoa: boolean = false;
  showFilter: boolean = false;
  isJeeHR: boolean;
  isTree: boolean;
  flatJeeHRs: FlatJeeHRDepartmentDTO[];
  flatDepartmentDtos: DepartmentManagementDTO[];
  clickSelection: boolean = false;
  datatree$: BehaviorSubject<any[]> = new BehaviorSubject<any>([]);
  lstPhongBanid: number[] = [];
  lstChucvu: SelectModel[];
  titlekeyword: string = '';
  get isLoading$() {
    return this._isLoading$.asObservable();
  }
  get errorMessage$() {
    return this._errorMessage$.asObservable();
  }

  constructor(
    public service: JeeSearchFormService,
    public cd: ChangeDetectorRef,
    public generalService: DanhMucChungService,
    private fb: FormBuilder,
    public trans: TranslateService
  ) {}

  ngOnInit() {
    this._isLoading$.next(true);
    this.titlekeyword = this.trans.instant(this.showSearch.titlekeyword);
    const sb = this.generalService
      .getDSPhongBan()
      .pipe(
        tap((res) => {
          this.initDataPhongBan(res.data);
        }),
        finalize(() => {
          const sb2 = this.generalService
            .getDSChucvu()
            .pipe(
              tap((res) => {
                this.lstChucvu = res.data;
              }),
              finalize(() => {
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
          this.subscriptions.push(sb2);
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

  initDataPhongBan(data: DepartmentManagement) {
    this.isJeeHR = data.isJeeHR;
    if (data.isTree) this.isTree = data.isTree;
    if (this.isJeeHR) {
      if (this.isTree) {
        this.datatree$.next(data.tree);
      }
      this.flatJeeHRs = data.flat;
    } else {
      this.flatDepartmentDtos = data.flat;
    }
  }

  getValueNode($event: TreeJeeHRDepartmentDTO) {
    this.lstPhongBanid = [];
    this.lstPhongBanid = this.joinRowID($event, this.lstPhongBanid);
    let join = this.lstPhongBanid.join(',');
  }

  joinRowID(tree: TreeJeeHRDepartmentDTO, list: number[]) {
    list.push(tree.RowID);
    tree.Children.forEach((element) => {
      list = this.joinRowID(element, list);
    });
    return list;
  }

  ngOnDestroy(): void {
    this.clearFilter();
    this.subscriptions.forEach((sb) => sb.unsubscribe());
  }
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
        debounceTime(300),
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
      phongbanid: [''],
      chucvuid: [''],
    });
  }

  clickSearch() {
    const filter = {};
    filter['keyword'] = this.filterGroup.controls['keyword'].value;
    filter['username'] = this.filterGroup.controls['username'].value;
    filter['tennhanvien'] = this.filterGroup.controls['tennhanvien'].value;
    filter['chucvuid'] = this.filterGroup.controls['chucvuid'].value;
    filter['isadmin'] = this.isAdmin;
    filter['dakhoa'] = this.daKhoa;
    if (!this.filterGroup.controls['phongbanid'].value) {
      if (this.lstPhongBanid.length > 0) {
        filter['phongbanid'] = this.lstPhongBanid.join(',');
      }
    } else {
      filter['phongbanid'] = this.filterGroup.controls['phongbanid'].value;
    }
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
    this.lstPhongBanid = [];
  }

  clickOutSideFilter() {
    if (!this.clickSelection) this.showFilter = false;
    setTimeout(() => {
      this.clickSelection = false;
    }, 3000);
  }
  clickSelect() {
    this.clickSelection = true;
  }
}
