import { environment } from './../../../../environments/environment.stage';
import { Injectable } from '@angular/core';
import { CanActivate, ActivatedRouteSnapshot, RouterStateSnapshot, Router } from '@angular/router';
import { AuthService } from './auth.service';
import { AuthSSO } from '../_models/authSSO.model';
import { LayoutUtilsService, MessageType } from 'src/app/pages/JeeAccount/_core/utils/layout-utils.service';
@Injectable({ providedIn: 'root' })
export class AuthGuard implements CanActivate {
  constructor(private authService: AuthService, private router: Router, private layoutUtilsService: LayoutUtilsService) {}
  appCode = environment.APPCODE;
  canActivate(route: ActivatedRouteSnapshot, state: RouterStateSnapshot): Promise<boolean> {
    return new Promise<boolean>((resolve, reject) => {
      if (!this.authService.isAuthenticated()) {
        if (this.authService.ssoToken$.getValue()) {
          this.authService.accessToken$.next(this.authService.ssoToken$.getValue());
        }
        resolve(this.canPassGuard());
      } else {
        resolve(this.canPassGuard());
      }
    });
  }

  canPassGuard(): Promise<boolean> {
    return new Promise<boolean>((resolve, reject) => {
      this.authService.getUserMeFromSSO().subscribe(
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
        const lstAppCode: string[] = data['user']['customData']['jee-account']['appCode'];
        if (lstAppCode) {
          if (lstAppCode.indexOf(this.appCode) === -1) {
            return resolve(this.unAppCodeAuthorizedGuard());
          } else {
            return resolve(true);
          }
        } else {
          return resolve(this.unAppCodeAuthorizedGuard());
        }
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

  unAppCodeAuthorizedGuard() {
    return new Promise<boolean>((resolve, reject) => {
      const popup = this.layoutUtilsService.showActionNotification(
        'Bạn không có quyền truy cập trang này',
        MessageType.Read,
        999999999,
        true,
        false,
        3000,
        'top',
        0
      );
      this.authService.logoutToSSO().subscribe(() => {
        localStorage.clear();
        popup.afterDismissed().subscribe((res) => {
          this.authService.logout();
          return resolve(false);
        });
      });
    });
  }
}
