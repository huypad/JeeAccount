import { Inject, Injectable, OnDestroy } from '@angular/core';
import { Observable, BehaviorSubject, of, Subscription } from 'rxjs';
import { map, catchError, switchMap, finalize } from 'rxjs/operators';
import { UserModel } from '../_models/user.model';
import { AuthModel } from '../_models/auth.model';
import { AuthHTTPService } from './auth-http';
import { environment } from 'src/environments/environment';
import { Router } from '@angular/router';
import { TableService } from '../../../_metronic/shared/crud-table/services/table.service';
import { HttpClient } from '@angular/common/http';
import { DOCUMENT } from '@angular/common';

const redirectUrl = environment.redirectUrl
@Injectable({
  providedIn: 'root',
})
export class AuthService extends TableService<any> implements OnDestroy {
  // private fields
  private unsubscribe: Subscription[] = []; // Read more: => https://brianflove.com/2016/12/11/anguar-2-unsubscribe-observables/
  // private authLocalStorageToken = `${environment.appVersion}-${environment.USERDATA_KEY}`;

  // public fields
  currentUser$: Observable<UserModel>;
  // isLoading$: Observable<boolean>;
  currentUserSubject: BehaviorSubject<UserModel>;
  isLoadingSubject: BehaviorSubject<boolean>;


  get currentUserValue(): UserModel {
    return this.currentUserSubject.value;
  }

  set currentUserValue(user: UserModel) {
    this.currentUserSubject.next(user);
  }
  public ldp_loadDataUser: string = '/user/me';
  public ldp_logOutUser: string = '/user/logout';
  
  constructor(@Inject(HttpClient) http,private router: Router, private authHttpService: AuthHTTPService,@Inject(DOCUMENT) private document: Document,
  ) {
    super(http);
     this.isLoadingSubject = new BehaviorSubject<boolean>(false);
    this.currentUserSubject = new BehaviorSubject<UserModel>(undefined);
    this.currentUser$ = this.currentUserSubject.asObservable();
    // this.isLoading$ = this.isLoadingSubject.asObservable();
    // const subscr = this.getUserByToken().subscribe();
    // this.unsubscribe.push(subscr);
  } 

  // public methods
  login(email: string, password: string): Observable<UserModel> {
    // this.isLoadingSubject.next(true);
    // return this.authHttpService.login(email, password).pipe(
    //   map((auth: AuthModel) => {
    //     const result = this.setAuthFromLocalStorage(auth);
    //     return result;
    //   }),
    //   switchMap(() => this.getUserByToken()),
    //   catchError((err) => {
    //     console.error('err', err);
    //     return of(undefined);
    //   }),
    //   finalize(() => this.isLoadingSubject.next(false))
    // );
    return 
  }

  logout() {   
    this.logOutUser_LandingPage(this.ldp_logOutUser).subscribe(
      (resData: any) => {
    //bên kia trả về null
    localStorage.removeItem(this.authLocalStorageToken); 
    // Chuyển hướng người dùng đến Single Sign On
    this.document.location.href = redirectUrl 
    + document.location.protocol +'//'
    + document.location.hostname + ':' 
    + document.location.port;
 
}
    )}

  getUserByToken(): Observable<UserModel> {
    const auth = this.getAuthFromLocalStorage();
    if (!auth || !auth.accessToken) {
      return of(undefined);
    }

    this.isLoadingSubject.next(true);
    return this.authHttpService.getUserByToken(auth.accessToken).pipe(
      map((user: UserModel) => {
        if (user) {
          this.currentUserSubject = new BehaviorSubject<UserModel>(user);
        } else {
          this.logout();
        }
        return user;
      }),
      finalize(() => this.isLoadingSubject.next(false))
    ); 
  }

  // need create new user then login
  registration(user: UserModel): Observable<any> {
    this.isLoadingSubject.next(true);
    return this.authHttpService.createUser(user).pipe(
      map(() => {
        this.isLoadingSubject.next(false);
      }),
      switchMap(() => this.login(user.email, user.password)),
      catchError((err) => {
        console.error('err', err);
        return of(undefined);
      }),
      finalize(() => this.isLoadingSubject.next(false))
    );
  }

  forgotPassword(email: string): Observable<boolean> {
    this.isLoadingSubject.next(true);
    return this.authHttpService
      .forgotPassword(email)
      .pipe(finalize(() => this.isLoadingSubject.next(false)));
  }

  // private methods
  // private setAuthFromLocalStorage(auth: AuthModel): boolean {
  //   // store auth accessToken/refreshToken/epiresIn in local storage to keep user logged in between page refreshes
  //   if (auth && auth.accessToken) {
  //     localStorage.setItem(this.authLocalStorageToken, JSON.stringify(auth));
  //     return true;
  //   }
  //   return false;
  // }

  // private getAuthFromLocalStorage(): AuthModel {
  //   try {
  //     const authData = JSON.parse(
  //       localStorage.getItem(this.authLocalStorageToken)
  //     );
  //     return authData;
  //   } catch (error) {
  //     console.error(error);
  //     return undefined;
  //   }
  // }

  ngOnDestroy() {
    this.unsubscribe.forEach((sb) => sb.unsubscribe());
  }
}
