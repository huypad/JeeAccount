import { BehaviorSubject, Subject, Subscription } from 'rxjs';
import { Component, OnInit, ChangeDetectionStrategy, ChangeDetectorRef, ElementRef, Output, EventEmitter } from '@angular/core';
import { JeeSearchFormService } from './jee-search-form.service';
import { debounceTime, distinctUntilChanged } from 'rxjs/operators';
import { FormBuilder, FormGroup } from '@angular/forms';

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
  @Output() keywordEvent: EventEmitter<string> = new EventEmitter<string>();
  @Output() filterEvent: EventEmitter<any> = new EventEmitter<any>();
  public isAdmin: boolean = false;
  public daKhoa: boolean = false;
  public showFilter: boolean = false;
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
    this.searchForm();
    this.filterForm();
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
      chucvu: [''],
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
    this.filterEvent.emit(filter);
  }

  clickShowFilter(val: boolean) {
    this.showFilter = !val;
  }

  clickOutSideFilter() {
    this.showFilter = false;
  }
}
