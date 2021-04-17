// tslint:disable:variable-name
import { HttpClient, HttpHeaders } from '@angular/common/http';
import { BehaviorSubject, Observable, of, Subscription } from 'rxjs';
import { catchError, finalize, map, tap } from 'rxjs/operators';
import { PaginatorState } from '../models/paginator.model';
import { ITableState, LP_BaseModel, LP_BaseModel_Single, TableResponseModel, TableResponseModel_LandingPage } from '../models/table.model';
import { BaseModel } from '../models/base.model';
import { SortState } from '../models/sort.model';
import { GroupingState } from '../models/grouping.model';
import { environment } from '../../../../../environments/environment';
import { AuthModel } from '../../../../modules/auth/_models/auth.model';

const DEFAULT_STATE_JEEREQUEST_YCCN: ITableState = {
  filter: {},
  paginator: new PaginatorState(),
  sorting: new SortState(),
  searchTerm: '',
  grouping: new GroupingState(),
  entityId: undefined,
};
const DEFAULT_STATE_JEEREQUEST_YCCD: ITableState = {
  filter: {},
  paginator: new PaginatorState(),
  sorting: new SortState(),
  searchTerm: '',
  grouping: new GroupingState(),
  entityId: undefined,
};

const DEFAULT_RESPONSE: TableResponseModel_LandingPage<any> = {
  status: 1,
  data: [],
  error: {
    code: 0,
    msg: '',
  },
  panigator: null,
};

export abstract class TableServiceJeeRequest<T> {
  private __responseData$ = new BehaviorSubject<TableResponseModel_LandingPage<any>>(DEFAULT_RESPONSE);

  // Private fields

  private _isFirstLoading$ = new BehaviorSubject<boolean>(true);
  private _errorMessage = new BehaviorSubject<string>('');
  private _subscriptions: Subscription[] = [];

  //JeeRequest -- fix bug dùng chung (chỉ sử dụng cho jeeRequest)
  //--------------------------yêu cầu chờ duyệt
  private _tableStateJRYCCD$ = new BehaviorSubject<ITableState>(DEFAULT_STATE_JEEREQUEST_YCCD);
  private _itemsYCCD$ = new BehaviorSubject<T[]>([]);
  private _isLoadingYCCD$ = new BehaviorSubject<boolean>(false);
  //--------------------------yêu cầu cá nhân
  private _tableStateJRYCCN$ = new BehaviorSubject<ITableState>(DEFAULT_STATE_JEEREQUEST_YCCN);
  private _itemsYCCN$ = new BehaviorSubject<T[]>([]);
  private _isLoadingYCCN$ = new BehaviorSubject<boolean>(false);
  //End JeeRequest
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

  // JeeRequest paginator

  //-------yêu cầu chờ duyệt
  get JeeRequest_paginatorYCCD() {
    return this._tableStateJRYCCD$.value.paginator;
  }
  get itemsYCCD$() {
    return this._itemsYCCD$.asObservable();
  }
  get isLoadingYCCD$() {
    return this._isLoadingYCCD$.asObservable();
  }

  //-------yêu cầu cá nhân
  get JeeRequest_paginatorYCCN() {
    return this._tableStateJRYCCN$.value.paginator;
  }
  get itemsYCCN$() {
    return this._itemsYCCN$.asObservable();
  }
  get isLoadingYCCN$() {
    return this._isLoadingYCCN$.asObservable();
  }

  // End JeeRequest paginator
  get filter() {
    return;
    // return this._tableState$.value.filter;
  }
  get sorting() {
    return;
    // return this._tableState$.value.sorting;
  }
  get searchTerm() {
    return;
    // return this._tableState$.value.searchTerm;
  }
  get grouping() {
    return;
    // return this._tableState$.value.grouping;
  }

  protected http: HttpClient;
  //api jeerequest
  API_JEEREQUEST = `${environment.ApiJeeRequest}`;
  constructor(http: HttpClient) {
    this.http = http;
  }

  public getAuthFromLocalStorage(): any {
    try {
      const authData = JSON.parse(localStorage.getItem(this.authLocalStorageToken));
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
      Authorization: `Bearer ${auth != null ? auth.access_token : ''}`,
    });
    return p;
  }

  public patchStateWithoutFetch_YCCD(patch: Partial<ITableState>) {
    const newState = Object.assign(this._tableStateJRYCCD$.value, patch);
    this._tableStateJRYCCD$.next(newState);
  }
  public patchStateWithoutFetch_YCCN(patch: Partial<ITableState>) {
    const newState = Object.assign(this._tableStateJRYCCN$.value, patch);
    this._tableStateJRYCCN$.next(newState);
  }

  //JeeRequest
  //yêu cầu chờ duyệt
  public fetch_JeeRequest_YeuCauChoDuyet(apiRoute: string = '', nameKey: string = 'id') {
    var resItems: any = [];
    var resTotalRow: number = 0;
    this._isLoadingYCCD$.next(true);
    this._errorMessage.next('');
    const request = this.find_JeeRequest(this._tableStateJRYCCD$.value, apiRoute)
      .pipe(
        tap((res: any) => {
          if (res && res.status == 1) {
            resItems = res.data;
            resTotalRow = res.page.TotalCount;
          }
          this._itemsYCCD$.next(resItems);
          this.__responseData$.next(res);
          this.patchStateWithoutFetch_YCCD({
            paginator: this._tableStateJRYCCD$.value.paginator.recalculatePaginator(resTotalRow),
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
          this._isLoadingYCCD$.next(false);
          const itemIds = this._itemsYCCD$.value.map((el: T) => {
            const item = (el as unknown) as BaseModel;
            return item[nameKey];
          });
          this.patchStateWithoutFetch_YCCD({
            grouping: this._tableStateJRYCCD$.value.grouping.clearRows(itemIds),
          });
        })
      )
      .subscribe();
    this._subscriptions.push(request);
  }
  // READ (Returning filtered list of entities)
  find_JeeRequest(tableState: ITableState, routeFind: string = ''): Observable<any> {
    const url = this.API_JEEREQUEST + routeFind;
    const httpHeader = this.getHttpHeaders();
    this._errorMessage.next('');
    return this.http
      .post<any>(url, tableState, { headers: httpHeader })
      .pipe(
        catchError((err) => {
          this._errorMessage.next(err);
          console.error('FIND ITEMS', err);
          return of({ status: 0, data: [], panigator: null, error: null });
        })
      );
  }
  public patchStateJeeRequest_YeuCauChoDuyet(patch: Partial<ITableState>, apiRoute: string = '') {
    this.patchStateWithoutFetch_YCCD(patch);
    this.fetch_JeeRequest_YeuCauChoDuyet(apiRoute);
  }
  //yêu cầu cá nhân
  public fetch_JeeRequest_YeuCauCaNhan(apiRoute: string = '', nameKey: string = 'id') {
    var resItems: any = [];
    var resTotalRow: number = 0;
    this._isLoadingYCCN$.next(true);
    this._errorMessage.next('');
    const request = this.find_JeeRequest(this._tableStateJRYCCN$.value, apiRoute)
      .pipe(
        tap((res: any) => {
          if (res && res.status == 1) {
            resItems = res.data;
            resTotalRow = res.page.TotalCount;
          }
          this._itemsYCCN$.next(resItems);
          this.__responseData$.next(res);
          this.patchStateWithoutFetch_YCCN({
            paginator: this._tableStateJRYCCN$.value.paginator.recalculatePaginator(resTotalRow),
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
          this._isLoadingYCCN$.next(false);
          const itemIds = this._itemsYCCN$.value.map((el: T) => {
            const item = (el as unknown) as BaseModel;
            return item[nameKey];
          });
          this.patchStateWithoutFetch_YCCN({
            grouping: this._tableStateJRYCCN$.value.grouping.clearRows(itemIds),
          });
        })
      )
      .subscribe();
    this._subscriptions.push(request);
  }
  public patchStateJeeRequest_YeuCauCaNhan(patch: Partial<ITableState>, apiRoute: string = '') {
    this.patchStateWithoutFetch_YCCN(patch);
    this.fetch_JeeRequest_YeuCauCaNhan(apiRoute);
  }
  //End API JeeRequest
}
