import { Injectable } from '@angular/core';
import { CanActivate, ActivatedRouteSnapshot, RouterStateSnapshot, Router } from '@angular/router';
import { AuthService } from './auth.service';
import { AuthSSO } from '../_models/authSSO.model';
import { LayoutUtilsService, MessageType } from 'src/app/pages/JeeAccount/_core/utils/layout-utils.service';
@Injectable({ providedIn: 'root' })
export class AuthGuard implements CanActivate {
  constructor(private authService: AuthService, private router: Router, private layoutUtilsService: LayoutUtilsService) { }
  canActivate(route: ActivatedRouteSnapshot, state: RouterStateSnapshot): Promise<boolean> {
    if (!this.authService.isAuthenticated()) {
      if (this.authService.ssoToken$.getValue()) {
        this.authService.accessToken$.next(this.authService.ssoToken$.getValue());
      }
      return this.canPassGuard();
    } else {
      return this.canPassGuard();
    }
  }

  canPassGuard(): Promise<boolean> {
    return new Promise<boolean>((resolve, reject) => {
      this.authService.getUserMeFromSSO()
        .subscribe(
          (data) => {
            resolve(this.canPassGuardAccessToken(data));
          },
          (error) => {
            this.authService.refreshToken().subscribe(
              (data: AuthSSO) => {
                resolve(this.canPassGuardAccessToken(data));
              },
              (error) => {
                resolve(this.unauthorizedGuard());
              }
            );
          }
        );
    });
  }

  canPassGuardAccessToken(data) {
    return new Promise<boolean>((resolve, reject) => {
      if (data && data.access_token) {
        this.authService.saveLocalStorageToken(this.authService.authLocalStorageToken, data);
        return resolve(true);
      }
    });
  }

  unauthorizedGuard() {
    return new Promise<boolean>((resolve, reject) => {
      localStorage.clear();
      this.authService.logout();
      return resolve(false);
    });
  }

}


