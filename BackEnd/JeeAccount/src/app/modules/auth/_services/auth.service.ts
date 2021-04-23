import { Inject, Injectable, OnDestroy } from '@angular/core';
import { Observable, BehaviorSubject, of, Subscription } from 'rxjs';
import { map, catchError, switchMap, finalize, tap } from 'rxjs/operators';
import { UserModel } from '../_models/user.model';
import { AuthModel } from '../_models/auth.model';
import { AuthHTTPService } from './auth-http';
import { environment } from 'src/environments/environment';
import { ActivatedRoute, Router } from '@angular/router';
import { TableService } from '../../../_metronic/shared/crud-table/services/table.service';
import { HttpClient, HttpHeaders, HttpParams } from '@angular/common/http';
import { DOCUMENT } from '@angular/common';
import jwt_decode from 'jwt-decode';
import { HttpUtilsService } from 'src/app/pages/JeeAccount/_core/utils/http-utils.service';
import { error } from '@angular/compiler/src/util';
import { AuthSSO } from '../_models/authSSO.model';

const redirectUrl = environment.redirectUrl;
const API_IDENTITY = `${environment.ApiIdentity}`;
const API_IDENTITY_LOGOUT = `${environment.ApiIdentity_Logout}`;
const API_IDENTITY_USER = `${environment.ApiIdentity_GetUser}`;
const API_IDENTITY_REFESHTOKEN = `${environment.ApiIdentity_Refresh}`;
@Injectable({
  providedIn: 'root',
})
export class AuthService implements OnDestroy {
  // private fields
  private unsubscribe: Subscription[] = [];
  // private authLocalStorageToken = `${environment.appVersion}-${environment.USERDATA_KEY}`;

  // public fields
  currentUser$: Observable<UserModel>;
  authSSOModel$: Observable<AuthSSO>;
  currentUserSubject: BehaviorSubject<UserModel> = new BehaviorSubject<UserModel>(undefined);
  authSSOModelSubject$: BehaviorSubject<AuthSSO> = new BehaviorSubject<AuthSSO>(undefined);
  // Private fields
  isLoading$ = new BehaviorSubject<boolean>(false);
  isFirstLoading$ = new BehaviorSubject<boolean>(true);
  errorMessage = new BehaviorSubject<string>(undefined);
  subscriptions: Subscription[] = [];
  ssoToken$: BehaviorSubject<string> = new BehaviorSubject<string>(undefined);
  authLocalStorageToken = `${environment.appVersion}-${environment.USERDATA_KEY}`;

  constructor(private http: HttpClient, private authHttpService: AuthHTTPService) {
    this.isLoading$ = new BehaviorSubject<boolean>(false);
    this.currentUser$ = this.currentUserSubject.asObservable();
    this.authSSOModel$ = this.authSSOModelSubject$.asObservable();
    this.ssoToken$.next(this.getParamsSSO());
    setInterval(() => this.autoGetUserFromSSO(), 10000);
  }

  get currentUserValue(): UserModel {
    return this.currentUserSubject.value;
  }

  set currentUserValue(user: UserModel) {
    this.currentUserSubject.next(user);
  }

  autoGetUserFromSSO() {
    const auth = this.getAuthFromLocalStorage();
    if (auth) {
      this.ssoToken$.next(this.authSSOModelSubject$.getValue().access_token);
      this.getUserDataFromSSO().subscribe(
        (data) => {
          if (data && data.access_token) {
            this.saveLocalStorageToken(this.authLocalStorageToken, data);
          }
        },
        (error) => {
          console.log('come here: ', error);
          this.refreshToken().subscribe(
            (data: AuthSSO) => {
              if (data && data.access_token) {
                this.saveLocalStorageToken(this.authLocalStorageToken, data);
              }
            },
            (error) => {
              console.log('error refresh:', error);
              localStorage.removeItem(this.authLocalStorageToken);
              this.logout();
            }
          );
        }
      );
    }
  }

  refreshToken(): Observable<any> {
    console.log('refresh_token: ', this.authSSOModelSubject$.getValue().refresh_token);
    const url = API_IDENTITY_REFESHTOKEN;
    const httpHeader = new HttpHeaders({
      'Content-Type': 'application/json',
      Authorization: `Bearer ${this.authSSOModelSubject$.getValue().refresh_token}`,
    });
    return this.http.post<any>(url, { headers: httpHeader });
  }

  isAuthenticated(): boolean {
    const auth = this.getAuthFromLocalStorage();
    if (auth && this.isTokenExpired()) return true;
    return false;
  }

  isTokenExpired(): boolean {
    const auth = this.getAuthFromLocalStorage();
    if (!auth || !auth.access_token) return false;
    const date = this.getTokenExpirationDate(auth.access_token);
    if (!date) return false;
    return date.valueOf() > new Date().valueOf();
  }

  getTokenExpirationDate(auth: string): Date {
    let decoded: any = jwt_decode(auth);
    if (!decoded.exp) return null;
    const date = new Date(0);
    date.setUTCSeconds(decoded.exp);
    return date;
  }

  getUserDataFromSSO(): Observable<any> {
    console.log('ssoToken: ', this.ssoToken$.getValue());
    const ssoToken = this.ssoToken$.getValue();
    const url = API_IDENTITY_USER;
    const httpHeader = new HttpHeaders({
      'Content-Type': 'application/json',
      Authorization: `Bearer ${ssoToken}`,
    });
    return this.http.get<any>(url, { headers: httpHeader });
  }

  logout() {
    let url = redirectUrl + document.location.protocol + '//' + document.location.hostname + ':' + document.location.port;
    window.location.href = url;
  }

  getUserByToken(): Observable<UserModel> {
    const auth = this.getAuthFromLocalStorage();
    if (!auth || !auth.accessToken) {
      return of(undefined);
    }
    this.isLoading$.next(true);
    return this.authHttpService.getUserByToken(auth.accessToken).pipe(
      map((user: UserModel) => {
        if (user) {
          this.currentUserSubject = new BehaviorSubject<UserModel>(user);
        } else {
          this.logout();
        }
        return user;
      }),
      finalize(() => this.isLoading$.next(false))
    );
  }

  logoutToSSO(): Observable<any> {
    const url = API_IDENTITY_LOGOUT;
    const httpHeader = this.getHTTPHeaders();
    return this.http.post<any>(url, null, { headers: httpHeader });
  }

  saveLocalStorageToken(key: string, value: any) {
    localStorage.setItem(key, JSON.stringify(value));
    this.authSSOModelSubject$.next(value);
    this.authSSOModel$ = this.authSSOModelSubject$.asObservable();
    this.ssoToken$.next(value.access_token);
    console.log('access_token value', value.access_token);
    console.log('value', value);
  }

  getParamsSSO() {
    const url = window.location.href;
    let paramValue = undefined;
    if (url.includes('?')) {
      const httpParams = new HttpParams({ fromString: url.split('?')[1] });
      paramValue = httpParams.get('sso_token').split('Bearer ')[1];
    }
    return paramValue;
  }

  ngOnDestroy() {
    this.unsubscribe.forEach((sb) => sb.unsubscribe());
  }

  getAuthFromLocalStorage() {
    try {
      const authData = JSON.parse(localStorage.getItem(this.authLocalStorageToken));
      if (authData === null) return undefined;
      return authData;
    } catch (error) {
      return undefined;
    }
  }

  getHTTPHeaders(): HttpHeaders {
    const auth = this.getAuthFromLocalStorage();
    let result = new HttpHeaders({
      'Content-Type': 'application/json',
      Authorization: `Bearer ${auth.access_token}`,
      'Access-Control-Allow-Origin': '*',
      'Access-Control-Allow-Headers': 'Content-Type',
    });
    return result;
  }

  forgotPassword(value: any): Observable<any> {
    throw new Error('Method not implemented.');
  }
  registration(newUser: UserModel): Observable<any> {
    throw new Error('Method not implemented.');
  }
}
