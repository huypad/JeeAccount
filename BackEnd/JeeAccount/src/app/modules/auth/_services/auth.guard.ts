import { Injectable } from '@angular/core';
import { CanActivate, ActivatedRouteSnapshot, RouterStateSnapshot, Router } from '@angular/router';
import { AuthService } from './auth.service';
import { AuthSSO } from '../_models/authSSO.model';
@Injectable({ providedIn: 'root' })
export class AuthGuard implements CanActivate {
  constructor(private authService: AuthService, private router: Router) {}
  canActivate(route: ActivatedRouteSnapshot, state: RouterStateSnapshot): Promise<boolean> {
    return new Promise<boolean>((resolve, reject) => {
      if (!this.authService.isAuthenticated()) {
        if (this.authService.ssoToken$.getValue()) {
          this.authService.accessToken$.next(this.authService.ssoToken$.getValue());
          this.authService.getUserMeFromSSO().subscribe(
            (data: AuthSSO) => {
              if (data && data.access_token) {
                this.authService.saveLocalStorageToken(this.authService.authLocalStorageToken, data);
                return resolve(true);
              }
            },
            (error) => {
              localStorage.clear();
              this.authService.logout();
              return resolve(false);
            }
          );
        } else {
          localStorage.clear();
          this.authService.logout();
          return resolve(false);
        }
      } else {
        return resolve(true);
      }
    });
  }
}
