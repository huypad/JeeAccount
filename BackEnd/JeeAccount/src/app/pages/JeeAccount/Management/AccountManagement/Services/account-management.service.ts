import { GroupingState } from './../../../../../_metronic/shared/crud-table/grouping.model';
import { HttpClient } from '@angular/common/http';
import { BehaviorSubject, Observable, of, Subscription } from 'rxjs';
import { Injectable, OnDestroy } from '@angular/core';
import {
  AccChangeTinhTrangModel,
  AccDirectManagerModel,
  AccountManagementDTO,
  AccountManagementModel,
  AppListDTO,
  InfoUserDTO,
  PostImgModel,
} from '../Model/account-management.model';
import { environment } from 'src/environments/environment';
import { QueryParamsModelNew } from '../../../_core/models/query-models/query-params.model';
import { HttpUtilsService } from '../../../_core/utils/http-utils.service';
import { ResultModel, ResultObjModel } from '../../../_core/models/_base.model';
import { ITableState, PaginatorState, SortState } from 'src/app/_metronic/shared/crud-table';
import { catchError, finalize, tap } from 'rxjs/operators';

const API_PRODUCTS_URL = environment.HOST_JEEACCOUNT_API + '/api/accountmanagement';
const DEFAULT_STATE: ITableState = {
  filter: {},
  paginator: new PaginatorState(),
  sorting: new SortState(),
  searchTerm: '',
  grouping: new GroupingState(),
  entityId: undefined,
};

@Injectable()
export class AccountManagementService implements OnDestroy {
  // Private fields
  private _items$ = new BehaviorSubject<AccountManagementDTO[]>([]);
  private _isLoading$ = new BehaviorSubject<boolean>(false);
  private _isFirstLoading$ = new BehaviorSubject<boolean>(true);
  private _tableState$ = new BehaviorSubject<ITableState>(DEFAULT_STATE);
  private _errorMessage = new BehaviorSubject<string>('');
  private _subscriptions: Subscription[] = [];

  // Getters
  get items$() {
    return this._items$.asObservable();
  }
  get isLoading$() {
    return this._isLoading$.asObservable();
  }
  get isFirstLoading$() {
    return this._isFirstLoading$.asObservable();
  }
  get errorMessage$() {
    return this._errorMessage.asObservable();
  }
  get subscriptions() {
    return this._subscriptions;
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

  constructor(private http: HttpClient, private httpUtils: HttpUtilsService) {}
  ngOnDestroy(): void {
    this.subscriptions.forEach((sb) => sb.unsubscribe());
  }

  public fetch() {
    this._isLoading$.next(true);
    this._errorMessage.next('');
    const request = this.findData(this._tableState$.value)
      .pipe(
        tap((res: ResultModel<AccountManagementDTO>) => {
          this._items$.next(res.data);
          this._tableState$.value.paginator.total = res.panigator.TotalCount;
        }),
        catchError((res) => {
          this._errorMessage.next(res.error.message);
          return of({
            items: [],
            total: 0,
          });
        }),
        finalize(() => {
          this._isLoading$.next(false);
        })
      )
      .subscribe();
    this._subscriptions.push(request);
  }

  public fetchStateSort(patch: Partial<ITableState>) {
    this.patchStateWithoutFetch(patch);
    this.fetch();
  }

  public patchStateWithoutFetch(patch: Partial<ITableState>) {
    const newState = Object.assign(this._tableState$.value, patch);
    this._tableState$.next(newState);
  }

  // Base Methods
  public patchState(patch: Partial<ITableState>) {
    this.patchStateWithoutFetch(patch);
    this.fetch();
  }

  findData(tableState: ITableState): Observable<any> {
    const httpHeaders = this.httpUtils.getHTTPHeaders();
    const url = API_PRODUCTS_URL + '/GetListAccountManagement';
    return this.http.post<any>(url, tableState, {
      headers: httpHeaders,
    });
  }

  GetListAppByCustomerID(): Observable<ResultModel<AppListDTO>> {
    const httpHeaders = this.httpUtils.getHTTPHeaders();
    const url = API_PRODUCTS_URL + `/GetListAppByCustomerID`;
    return this.http.get<any>(url, {
      headers: httpHeaders,
    });
  }
  createAccount(item: AccountManagementModel): Observable<any> {
    const httpHeaders = this.httpUtils.getHTTPHeaders();
    const url = API_PRODUCTS_URL + '/createAccount';
    return this.http.post<any>(url, item, { headers: httpHeaders });
  }

  UpdateAvatarWithChangeUrlAvatar(img: PostImgModel): Observable<any> {
    const httpHeaders = this.httpUtils.getHTTPHeaders();
    const url = API_PRODUCTS_URL + `/UpdateAvatarWithChangeUrlAvatar`;
    return this.http.post<any>(url, img, { headers: httpHeaders });
  }

  UpdateDirectManager(acc: AccDirectManagerModel): Observable<any> {
    const httpHeaders = this.httpUtils.getHTTPHeaders();
    const url = API_PRODUCTS_URL + `/UpdateDirectManager`;
    return this.http.post<any>(url, acc, {
      headers: httpHeaders,
    });
  }

  changeTinhTrang(acc: AccChangeTinhTrangModel): Observable<any> {
    const httpHeaders = this.httpUtils.getHTTPHeaders();
    const url = API_PRODUCTS_URL + `/ChangeTinhTrang`;
    return this.http.post<any>(url, acc, {
      headers: httpHeaders,
    });
  }

  GetInfoByUsername(username: string): Observable<ResultObjModel<InfoUserDTO>> {
    const httpHeaders = this.httpUtils.getHTTPHeaders();
    const url = API_PRODUCTS_URL + `/GetInfoByUsername/username=${username}`;
    return this.http.get<any>(url, {
      headers: httpHeaders,
    });
  }
}
