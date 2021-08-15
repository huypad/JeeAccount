import { Injectable, OnDestroy } from '@angular/core';
import { Observable, BehaviorSubject, of, Subscription } from 'rxjs';
import { map, finalize } from 'rxjs/operators';
import { UserModel } from '../_models/user.model';
import { AuthHTTPService } from './auth-http';
import { environment } from 'src/environments/environment';
import { HttpClient, HttpHeaders, HttpParams } from '@angular/common/http';
import jwt_decode from 'jwt-decode';
import { AuthSSO } from '../_models/authSSO.model';

const redirectUrl = environment.HOST_PORTAL_API + '/?redirectUrl=';
const API_IDENTITY = `${environment.HOST_IDENTITYSERVER_API}`;
const API_IDENTITY_LOGOUT = `${API_IDENTITY}/user/logout`;
const API_IDENTITY_USER = `${API_IDENTITY}/user/me`;
const API_IDENTITY_REFESHTOKEN = `${API_IDENTITY}/refresh`;
@Injectable({
  providedIn: 'root',
})
export class AuthService implements OnDestroy {
  // private fields
  private unsubscribe: Subscription[] = [];

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
  accessToken$ = new BehaviorSubject<string>(undefined);
  refreshToken$ = new BehaviorSubject<string>(undefined);

  constructor(private http: HttpClient, private authHttpService: AuthHTTPService) {
    this.isLoading$ = new BehaviorSubject<boolean>(false);
    this.currentUser$ = this.currentUserSubject.asObservable();
    this.authSSOModel$ = this.authSSOModelSubject$.asObservable();
    this.ssoToken$.next(this.getParamsSSO());
    setInterval(() => this.autoGetUserFromSSO(), 60000);
  }

  get currentUserValue(): UserModel {
    return this.currentUserSubject.value;
  }

  set currentUserValue(user: UserModel) {
    this.currentUserSubject.next(user);
  }
  getUserId() {
    var auth = this.getAuthFromLocalStorage();
    return auth.user.customData['jee-account'].userID;
  }
  autoGetUserFromSSO() {
    const auth = this.getAuthFromLocalStorage();
    if (auth) {
      this.saveNewUserMe();
    }
  }

  saveNewUserMe(access_token?: string, refresh_token?: string) {
    if (access_token) this.accessToken$.next(access_token);
    if (refresh_token) this.refreshToken$.next(refresh_token);
    this.getUserMeFromSSO().subscribe(
      (data) => {
        if (data && data.access_token) {
          this.saveLocalStorageToken(this.authLocalStorageToken, data);
        }
      },
      (error) => {
        this.refreshToken().subscribe(
          (data: AuthSSO) => {
            if (data && data.access_token) {
              this.saveLocalStorageToken(this.authLocalStorageToken, data);
            }
          },
          (error) => {
            localStorage.removeItem(this.authLocalStorageToken);
            this.logout();
          }
        );
      }
    );
  }

  isAuthenticated(): boolean {
    const auth = this.getAuthFromLocalStorage();
    if (auth) {
      if (this.isTokenExpired(auth.access_token)) {
        this.saveAuthSSOModelAccessTokenRefreshToken(auth);
        return true;
      }
      if (this.isTokenExpired(auth.refresh_token)) {
        this.saveAuthSSOModelAccessTokenRefreshToken(auth);
        return true;
      }
    }
    return false;
  }

  isTokenExpired(token: string): boolean {
    const date = this.getTokenExpirationDate(token);
    if (!date) return false;
    return date.valueOf() > new Date().valueOf();
  }

  saveAuthSSOModelAccessTokenRefreshToken(auth) {
    if (!this.authSSOModelSubject$.getValue()) {
      const authSSOModel = new AuthSSO();
      authSSOModel.setAuthSSO(auth);
      this.authSSOModelSubject$.next(authSSOModel);
      this.authSSOModel$ = this.authSSOModelSubject$.asObservable();
    }
    if (!this.accessToken$.getValue()) this.accessToken$.next(auth.access_token);
    if (!this.refreshToken$.getValue()) this.refreshToken$.next(auth.refresh_token);
  }

  getTokenExpirationDate(auth: string): Date {
    let decoded: any = jwt_decode(auth);
    if (!decoded.exp) return null;
    const date = new Date(0);
    date.setUTCSeconds(decoded.exp);
    return date;
  }

  logout() {
    let url = redirectUrl + document.location.protocol + '//' + document.location.hostname + ':' + document.location.port;
    window.location.href = url;
  }

  getParamsSSO() {
    const url = window.location.href;
    let paramValue = undefined;
    if (url.includes('?')) {
      const httpParams = new HttpParams({ fromString: url.split('?')[1] });
      paramValue = httpParams.get('sso_token');
    }
    return paramValue;
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

  saveLocalStorageToken(key: string, value: any) {
    localStorage.setItem(key, JSON.stringify(value));
    this.authSSOModelSubject$.next(value);
    this.authSSOModel$ = this.authSSOModelSubject$.asObservable();
    this.accessToken$.next(value.access_token);
    this.refreshToken$.next(value.refresh_token);
  }

  ngOnDestroy() {
    this.unsubscribe.forEach((sb) => sb.unsubscribe());
  }

  // call api identity server
  getUserMeFromSSO(): Observable<any> {
    const accessToken = this.accessToken$.getValue();
    const url = API_IDENTITY_USER;
    const httpHeader = new HttpHeaders({
      'Content-Type': 'application/json',
      Authorization: `Bearer ${accessToken}`,
    });
    return this.http.get<any>(url, { headers: httpHeader });
  }

  refreshToken(): Observable<any> {
    const url = API_IDENTITY_REFESHTOKEN;
    const httpHeader = new HttpHeaders({
      'Content-Type': 'application/json',
      Authorization: `Bearer ${this.refreshToken$.getValue()}`,
    });
    return this.http.post<any>(url, null, { headers: httpHeader });
  }

  logoutToSSO(): Observable<any> {
    const url = API_IDENTITY_LOGOUT;
    const accessToken = this.accessToken$.getValue();
    const httpHeader = new HttpHeaders({
      'Content-Type': 'application/json',
      Authorization: `Bearer ${accessToken}`,
    });
    return this.http.post<any>(url, null, { headers: httpHeader });
  }

  // end call api identity server

  // method metronic call
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

  forgotPassword(value: any): Observable<any> {
    throw new Error('Method not implemented.');
  }
  registration(newUser: UserModel): Observable<any> {
    throw new Error('Method not implemented.');
  }
  // end method metronic call
}
