// tslint:disable:variable-name
import { HttpClient, HttpHeaders } from '@angular/common/http';
import { BehaviorSubject, Observable, of, Subscription } from 'rxjs';
import { catchError, finalize, map, tap } from 'rxjs/operators';
import { PaginatorState } from '../models/paginator.model';
import { ITableState, TableResponseModel_LandingPage } from '../models/table.model';
import { BaseModel } from '../models/base.model';
import { SortState } from '../models/sort.model';
import { GroupingState } from '../models/grouping.model';
import { environment } from '../../../../../environments/environment';

const DEFAULT_STATE_JeeAdmin: ITableState = {
  filter: {},
  paginator: new PaginatorState(),
  sorting: new SortState(),
  searchTerm: '',
  grouping: new GroupingState(),
  entityId: undefined
};

const DEFAULT_RESPONSE: TableResponseModel_LandingPage<any> = {
  status: 1,
  data: [],
  error: {
    code: 0,
    msg: ""
  },
  panigator: null
};


export abstract class TableServiceJeeAdmin<T> {
  private __responseData$ = new BehaviorSubject<TableResponseModel_LandingPage<any>>(DEFAULT_RESPONSE);

  // Private fields
  private _isFirstLoading$ = new BehaviorSubject<boolean>(true);
  private _errorMessage = new BehaviorSubject<string>('');
  private _subscriptions: Subscription[] = [];

  private _tableState$ = new BehaviorSubject<ITableState>(DEFAULT_STATE_JeeAdmin);
  private _items$ = new BehaviorSubject<T[]>([]);
  private _isLoading$ = new BehaviorSubject<boolean>(false);

  public authLocalStorageToken = `${environment.appVersion}-${environment.USERDATA_KEY}`;
  // Getters 
  get isFirstLoading$() {
    return this._isFirstLoading$.asObservable();
  }
  get errorMessage$() {
    return this._errorMessage.asObservable();
  }
  get subscriptions() {
    return this._subscriptions;
  }

  // JeeAdmin paginator
  get JeeAdmin_paginator() {
    return this._tableState$.value.paginator;
  }
  get items$() {
    return this._items$.asObservable();
  }
  get isLoading$() {
    return this._isLoading$.asObservable();
  }

  // State getters
  get paginator() {
    return this._tableState$.value.paginator;
  }
  get filter() {
    return this._tableState$.value.filter;
  }
  get sorting() {
    return this._tableState$.value.sorting;
  }
  get searchTerm() {
    return this._tableState$.value.searchTerm;
  }
  get grouping() {
    return this._tableState$.value.grouping;
  }

  protected http: HttpClient;

  API_JeeAdmin = `${environment.ApiJeeAdmin}`;
  constructor(http: HttpClient) {
    this.http = http;
  }

  public getAuthFromLocalStorage(): any {
    try {
      const authData = JSON.parse(
        localStorage.getItem(this.authLocalStorageToken)
      );
      return authData;
    } catch (error) {
      console.error(error);
      return undefined;
    }
  }

  getHttpHeaders() {
    const auth = this.getAuthFromLocalStorage();
    var p = new HttpHeaders({
      'Content-Type': 'application/json',
      "Authorization": `Bearer ${auth != null ? auth.access_token : ''}`
    });
    return p;
  }

  public patchStateWithoutFetch(patch: Partial<ITableState>) {
    const newState = Object.assign(this._tableState$.value, patch);
    this._tableState$.next(newState);
  }

  //JeeAdmin 
  public fetch_JeeAdmin(apiRoute: string = '', nameKey: string = 'id') {
    var resItems: any = [];
    var resTotalRow: number = 0;
    this._isLoading$.next(true);
    this._errorMessage.next('');
    const request = this.find_JeeAdmin(this._tableState$.value, apiRoute)
      .pipe(
        tap((res: TableResponseModel_LandingPage<T>) => {
          if (res && res.status == 1) {
            resItems = res.data;
            resTotalRow = res.panigator.total;
          }
          this._items$.next(resItems);
          this.__responseData$.next(res);
          this.patchStateWithoutFetch({
            paginator: this._tableState$.value.paginator.recalculatePaginator(
              resTotalRow
            ),
          });

        }),
        catchError((err) => {
          this._errorMessage.next(err);
          return of({
            status: 0,
            data: [],
            paginator: null,
            error: null,
          });
        }),
        finalize(() => {
          this._isLoading$.next(false);
          const itemIds = this._items$.value.map((el: T) => {
            const item = (el as unknown) as BaseModel;
            return item[nameKey];
          });
          this.patchStateWithoutFetch({
            grouping: this._tableState$.value.grouping.clearRows(itemIds),
          });
        })
      )
      .subscribe();
    this._subscriptions.push(request);
  }

  // Read (Returning filtered list of entities)
  find_JeeAdmin(tableState: ITableState, routeFind: string = ''): Observable<TableResponseModel_LandingPage<T>> {
    const url = this.API_JeeAdmin + routeFind;
    const httpHeader = this.getHttpHeaders();
    this._errorMessage.next('');
    return this.http.post<TableResponseModel_LandingPage<T>>(url, tableState, { headers: httpHeader }).pipe(
      catchError(err => {
        this._errorMessage.next(err);
        console.error('Find items: ', err);
        return of({ status: 0, data: [], panigator: null, error: null });
      })
    );
  }
  
  public patchStateJeeAdmin(patch: Partial<ITableState>, apiRoute: string = '') {
    this.patchStateWithoutFetch(patch);
    this.fetch_JeeAdmin(apiRoute);
  }
}
