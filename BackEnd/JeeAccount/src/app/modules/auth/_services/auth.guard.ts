import { Inject, Injectable } from "@angular/core";
import {
  Router,
  CanActivate,
  ActivatedRouteSnapshot,
  RouterStateSnapshot,
  ActivatedRoute,
} from "@angular/router";
import { AuthService } from "./auth.service";
import { environment } from "./../../../../environments/environment";
import { catchError, delay, first } from "rxjs/operators";
import { BehaviorSubject, Observable, of, Subscription } from "rxjs";
import jwt_decode from "jwt-decode";
import { HttpParams } from "@angular/common/http";
import { DOCUMENT } from "@angular/common";
const sso_token = environment.sso;

@Injectable({ providedIn: "root" })
export class AuthGuard implements CanActivate {
  sso_token: any;
  private subscriptions: Subscription[] = [];
  currentUserSubject: BehaviorSubject<any>;
  decoded: any;

  constructor(
    private authService: AuthService,
    @Inject(DOCUMENT) private document: Document
  ) {
    this.sso_token = this.getParamValueQueryString(sso_token);
  }

  canActivate(route: ActivatedRouteSnapshot, state: RouterStateSnapshot):Observable<boolean>|Promise<any> {
    return this.kiemTraDangNhap() 
    
  }
  async kiemTraDangNhap(){ 
    const currentUser = this.authService.getAuthFromLocalStorage();
    if (currentUser && this.isTokenExpired()) {
      return true;
    }
    else{ 
      if (!this.sso_token) {
        this.authService.logout();
        return false;
      } else { 
        return new Promise(res => {
          this.authService.getDataUser_LandingPage(this.authService.ldp_loadDataUser,this.sso_token)
          .subscribe(
              (resData: any) => {
          if (resData && resData.access_token) {
            localStorage.setItem(this.authService.authLocalStorageToken, JSON.stringify(resData));  
            res(true);
          } else { 
            this.authService.logout();
            res(false);
          }
        }
      );
      }) 
      }
    }
    
  }

  isTokenExpired(token?: string): boolean {
    const auth = this.authService.getAuthFromLocalStorage();
    if (auth === null) return false;

    if (!token) token = auth.access_token;
    if (!token) return false;

    const date = this.getTokenExpirationDate(token);
    if (date === undefined) return false;
    return date.valueOf() > new Date().valueOf();
  }

  getTokenExpirationDate(token: string): Date {
    // token = atob(token);
    this.decoded = jwt_decode(token);

    if (this.decoded.exp === undefined) return null;

    const date = new Date(0);
    date.setUTCSeconds(this.decoded.exp);
    return date;
  }

  getParamValueQueryString(paramName) {
    const url = window.location.href;
    let paramValue;
    if (url.includes("?")) {
      const httpParams = new HttpParams({ fromString: url.split("?")[1] });
      paramValue = httpParams.get(paramName);
    }
    return paramValue;
  }
}
