import { BehaviorSubject, Subject, Subscription } from 'rxjs';
import { Component, OnInit, ChangeDetectionStrategy, ChangeDetectorRef, ElementRef } from '@angular/core';
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

  search(val) {}
}
