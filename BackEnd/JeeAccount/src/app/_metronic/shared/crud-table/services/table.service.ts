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

const DEFAULT_STATE: ITableState = {
  filter: {},
  paginator: new PaginatorState(),
  sorting: new SortState(),
  searchTerm: '',
  grouping: new GroupingState(),
  entityId: undefined,
};
const DEFAULT_STATE_JEEREQUEST: ITableState = {
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

export abstract class TableService<T> {
  private __responseData$ = new BehaviorSubject<TableResponseModel_LandingPage<any>>(DEFAULT_RESPONSE);

  // Private fields
  private _items$ = new BehaviorSubject<T[]>([]);
  private _isLoading$ = new BehaviorSubject<boolean>(false);
  private _isFirstLoading$ = new BehaviorSubject<boolean>(true);
  private _tableState$ = new BehaviorSubject<ITableState>(DEFAULT_STATE);
  private _errorMessage = new BehaviorSubject<string>('');
  private _subscriptions: Subscription[] = [];
  //JeeRequest -- fix bug (chỉ sử dụng cho jeeRequest, trương h)
  private _tableStateJR$ = new BehaviorSubject<ITableState>(DEFAULT_STATE_JEEREQUEST);

  public authLocalStorageToken = `${environment.appVersion}-${environment.USERDATA_KEY}`;
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

  protected http: HttpClient;
  // API URL has to be overrided
  API_URL = `${environment.apiUrl}/endpoint`;
  API_JeeWork = `${environment.ApiJeeWork}`;
  API_IDENTITY = `${environment.ApiIdentity}`;
  constructor(http: HttpClient) {
    this.http = http;
  }

  // CREATE
  // server should return the object with ID
  create(item: BaseModel): Observable<BaseModel> {
    this._isLoading$.next(true);
    this._errorMessage.next('');
    return this.http.post<BaseModel>(this.API_URL, item).pipe(
      catchError((err) => {
        this._errorMessage.next(err);
        console.error('CREATE ITEM', err);
        return of({ id: undefined });
      }),
      finalize(() => this._isLoading$.next(false))
    );
  }

  // READ (Returning filtered list of entities)
  find(tableState: ITableState): Observable<TableResponseModel<T>> {
    const url = this.API_URL + '/find';
    this._errorMessage.next('');
    return this.http.post<TableResponseModel<T>>(url, tableState).pipe(
      catchError((err) => {
        this._errorMessage.next(err);
        console.error('FIND ITEMS', err);
        return of({ items: [], total: 0 });
      })
    );
  }

  getItemById(id: number): Observable<BaseModel> {
    this._isLoading$.next(true);
    this._errorMessage.next('');
    const url = `${this.API_URL}/${id}`;
    return this.http.get<BaseModel>(url).pipe(
      catchError((err) => {
        this._errorMessage.next(err);
        console.error('GET ITEM BY IT', id, err);
        return of({ id: undefined });
      }),
      finalize(() => this._isLoading$.next(false))
    );
  }

  // UPDATE
  update(item: BaseModel): Observable<any> {
    const url = `${this.API_URL}/${item.id}`;
    this._isLoading$.next(true);
    this._errorMessage.next('');
    return this.http.put(url, item).pipe(
      catchError((err) => {
        this._errorMessage.next(err);
        console.error('UPDATE ITEM', item, err);
        return of(item);
      }),
      finalize(() => this._isLoading$.next(false))
    );
  }

  // UPDATE Status
  updateStatusForItems(ids: number[], status: number): Observable<any> {
    this._isLoading$.next(true);
    this._errorMessage.next('');
    const body = { ids, status };
    const url = this.API_URL + '/updateStatus';
    return this.http.put(url, body).pipe(
      catchError((err) => {
        this._errorMessage.next(err);
        console.error('UPDATE STATUS FOR SELECTED ITEMS', ids, status, err);
        return of([]);
      }),
      finalize(() => this._isLoading$.next(false))
    );
  }

  // DELETE
  delete(id: any): Observable<any> {
    this._isLoading$.next(true);
    this._errorMessage.next('');
    const url = `${this.API_URL}/${id}`;
    return this.http.delete(url).pipe(
      catchError((err) => {
        this._errorMessage.next(err);
        console.error('DELETE ITEM', id, err);
        return of({});
      }),
      finalize(() => this._isLoading$.next(false))
    );
  }

  // delete list of items
  deleteItems(ids: number[] = []): Observable<any> {
    this._isLoading$.next(true);
    this._errorMessage.next('');
    const url = this.API_URL + '/deleteItems';
    const body = { ids };
    return this.http.put(url, body).pipe(
      catchError((err) => {
        this._errorMessage.next(err);
        console.error('DELETE SELECTED ITEMS', ids, err);
        return of([]);
      }),
      finalize(() => this._isLoading$.next(false))
    );
  }

  public fetch() {
    this._isLoading$.next(true);
    this._errorMessage.next('');
    const request = this.find(this._tableState$.value)
      .pipe(
        tap((res: TableResponseModel<T>) => {
          this._items$.next(res.items);
          this.patchStateWithoutFetch({
            paginator: this._tableState$.value.paginator.recalculatePaginator(res.total),
          });
        }),
        catchError((err) => {
          this._errorMessage.next(err);
          return of({
            items: [],
            total: 0,
          });
        }),
        finalize(() => {
          this._isLoading$.next(false);
          const itemIds = this._items$.value.map((el: T) => {
            const item = (el as unknown) as BaseModel;
            return item.id;
          });
          this.patchStateWithoutFetch({
            grouping: this._tableState$.value.grouping.clearRows(itemIds),
          });
        })
      )
      .subscribe();
    this._subscriptions.push(request);
  }

  public setDefaults() {
    this.patchStateWithoutFetch({ filter: {} });
    this.patchStateWithoutFetch({ sorting: new SortState() });
    this.patchStateWithoutFetch({ grouping: new GroupingState() });
    this.patchStateWithoutFetch({ searchTerm: '' });
    this.patchStateWithoutFetch({
      paginator: new PaginatorState(),
    });
    this._isFirstLoading$.next(true);
    this._isLoading$.next(true);
    this._tableState$.next(DEFAULT_STATE);
    this._errorMessage.next('');
  }

  // Base Methods
  public patchState(patch: Partial<ITableState>) {
    this.patchStateWithoutFetch(patch);
    this.fetch();
  }

  public patchStateWithoutFetch(patch: Partial<ITableState>) {
    const newState = Object.assign(this._tableState$.value, patch);
    this._tableState$.next(newState);
  }

  // LandingPage
  public fetch_Project_Team(apiRoute: string = '', nameKey: string = 'id') {
    apiRoute = 'api/project-team/List';
    var resItems: any = [];
    var resTotalRow: number = 0;
    this._isLoading$.next(true);
    this._errorMessage.next('');
    const request = this.find_Project_Team_Post(this._tableState$.value, apiRoute)
      .pipe(
        tap((res: TableResponseModel_LandingPage<T>) => {
          if (res && res.status == 1) {
            resItems = res.data;
            resTotalRow = res.panigator.total;
          }
          this._items$.next(resItems);
          this.__responseData$.next(res);
          this.patchStateWithoutFetch({
            paginator: this._tableState$.value.paginator.recalculatePaginator(resTotalRow),
          });
        }),
        catchError((err) => {
          this._errorMessage.next(err);
          return of({
            status: 0,
            data: [],
            panigator: null,
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

  private setAuthFromLocalStorage(auth: AuthModel): boolean {
    if (auth && auth.accessToken) {
      localStorage.setItem(this.authLocalStorageToken, JSON.stringify(auth));
      return true;
    }
    return false;
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
      'Accept-Language': `Bearer ${auth != null ? auth.access_token : ''}`,
    });
    return p;
  }

  // READ (Returning filtered list of entities)
  find_Project_Team_Get(tableState: ITableState, routeFind: string = ''): Observable<TableResponseModel_LandingPage<T>> {
    // routeFind = "";
    const url = this.API_JeeWork + routeFind;
    const httpHeader = this.getHttpHeaders();
    this._errorMessage.next('');
    return this.http
      .get<TableResponseModel_LandingPage<T>>(url, { headers: httpHeader })
      .pipe(
        catchError((err) => {
          this._errorMessage.next(err);
          console.error('FIND ITEMS', err);
          return of({ status: 0, data: [], panigator: null, error: null });
        })
      );
  }
  find_Project_Team_Post(tableState: ITableState, routeFind: string = ''): Observable<TableResponseModel_LandingPage<T>> {
    // routeFind = "";
    const url = this.API_JeeWork + routeFind;
    const httpHeader = this.getHttpHeaders();
    this._errorMessage.next('');
    return this.http
      .post<TableResponseModel_LandingPage<T>>(url, tableState, { headers: httpHeader })
      .pipe(
        catchError((err) => {
          this._errorMessage.next(err);
          console.error('FIND ITEMS', err);
          return of({ status: 0, data: [], panigator: null, error: null });
        })
      );
  }
  // Base Methods
  public patchStateLandingPage(patch: Partial<ITableState>, apiRoute: string = '') {
    this.patchStateWithoutFetch(patch);
    this.fetch_Project_Team(apiRoute);
  }

  getItemById_LandingPage(id: number, routeGet: string = ''): Observable<BaseModel> {
    this._isLoading$.next(true);
    this._errorMessage.next('');
    const httpHeader = this.getHttpHeaders();
    // const url = `${this.API_URL}/${id}`;
    const url = this.API_JeeWork + routeGet + `${id}`;
    return this.http
      .get<BaseModel>(url, { headers: httpHeader })
      .pipe(
        tap((resID) => {}),
        catchError((err) => {
          this._errorMessage.next(err);
          console.error('GET ITEM BY IT', id, err);
          return of({ id: undefined });
        }),
        finalize(() => this._isLoading$.next(false))
      );
  }

  getItemById_LandingPage_POST(id: number, routePost: string = ''): Observable<LP_BaseModel<T>> {
    this._isLoading$.next(true);
    this._errorMessage.next('');
    const httpHeader = this.getHttpHeaders();
    const url = this.API_JeeWork + routePost;
    return this.http.post<LP_BaseModel<T>>(url, { htid: +id }, { headers: httpHeader }).pipe(
      tap((resID) => {
        this.__responseData$.next(resID);
      }),
      catchError((err) => {
        this._errorMessage.next(err);
        console.error('FIND BY ID ITEMS', err);
        return of({ status: 0, data: [], panigator: null, error: null });
      }),
      finalize(() => this._isLoading$.next(false))
    );
  }

  // CREATE
  // server should return the object with ID
  createLandingPage(item: any, routePost: string = ''): Observable<LP_BaseModel<T>> {
    this._isLoading$.next(true);
    this._errorMessage.next('');
    const httpHeader = this.getHttpHeaders();
    const url = this.API_JeeWork + routePost;
    return this.http
      .post<LP_BaseModel<T>>(url, item, { headers: httpHeader })
      .pipe(
        tap((res: TableResponseModel_LandingPage<T>) => {
          this.__responseData$.next(res);
        }),
        catchError((err) => {
          this._errorMessage.next(err);
          console.error('CREATE ITEM', err);
          return of({ status: 1, data: [], panigator: null, error: null });
        }),
        finalize(() => this._isLoading$.next(false))
      );
  }

  // UPDATE
  updateLandingPage(item: any, routePost: string = ''): Observable<LP_BaseModel_Single<any>> {
    this._isLoading$.next(true);
    this._errorMessage.next('');
    const httpHeader = this.getHttpHeaders();
    const url = this.API_JeeWork + routePost;
    return this.http
      .post<LP_BaseModel_Single<any>>(url, item, { headers: httpHeader })
      .pipe(
        tap((res: TableResponseModel_LandingPage<T>) => {
          this.__responseData$.next(res);
        }),
        catchError((err) => {
          this._errorMessage.next(err);
          console.error('UPDATE ITEM', item, err);
          return of({ status: 1, data: null, panigator: null, error: null });
        }),
        finalize(() => this._isLoading$.next(false))
      );
  }

  // DELETE
  deleteLandingPage(id: number, routePost: string = ''): Observable<LP_BaseModel_Single<any>> {
    this._isLoading$.next(true);
    this._errorMessage.next('');
    const httpHeader = this.getHttpHeaders();
    const url = this.API_JeeWork + routePost;
    return this.http.post<LP_BaseModel_Single<any>>(url, { htid: id }, { headers: httpHeader }).pipe(
      tap((res: TableResponseModel_LandingPage<T>) => {
        this.__responseData$.next(res);
      }),
      catchError((err) => {
        this._errorMessage.next(err);
        console.error('DELETE ITEM', id, err);
        return of({ status: 0, data: [], panigator: null, error: null });
      }),
      finalize(() => this._isLoading$.next(false))
    );
  }

  // delete list of items
  deleteItemsLandingPage(ids: number[] = [], routePost: string = ''): Observable<LP_BaseModel_Single<any>> {
    this._isLoading$.next(true);
    this._errorMessage.next('');
    const httpHeader = this.getHttpHeaders();
    const url = this.API_JeeWork + routePost;
    ids = [...ids.map((k) => +k)];
    return this.http.post<LP_BaseModel_Single<any>>(url, { htids: ids }, { headers: httpHeader }).pipe(
      tap((res: TableResponseModel_LandingPage<T>) => {
        this.__responseData$.next(res);
      }),
      catchError((err) => {
        this._errorMessage.next(err);
        console.error('DELETE SELECTED ITEMS', ids, err);
        return of({ status: 0, data: [], panigator: null, error: null });
      }),
      finalize(() => this._isLoading$.next(false))
    );
  }

  // Đăng nhập
  // lay jwt + data tu sso_token
  getDataUser_LandingPage(routeFind: string = '', sso_token: string = ''): Observable<BaseModel> {
    const url = this.API_IDENTITY + routeFind;
    const httpHeader = new HttpHeaders({
      'Content-Type': 'application/json',
      Authorization: sso_token,
    });
    return this.http
      .get<BaseModel>(url, { headers: httpHeader })
      .pipe(
        tap((res) => {}),
        catchError((err) => {
          this._errorMessage.next(err);
          console.error('lỗi lấy data', err);
          return of({ id: undefined });
        })
      );
  }
  // logout
  logOutUser_LandingPage(routeFind: string = ''): Observable<BaseModel> {
    const url = this.API_IDENTITY + routeFind;
    const httpHeader = this.getHttpHeaders();
    return this.http
      .post<BaseModel>(url, null, { headers: httpHeader })
      .pipe(
        tap((res) => {}),
        catchError((err) => {
          this._errorMessage.next(err);
          console.error('lỗi logout', err);
          return of({ id: undefined });
        })
      );
  }
}
