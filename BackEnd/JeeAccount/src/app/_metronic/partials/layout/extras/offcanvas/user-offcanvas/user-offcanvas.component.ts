import { RemindService } from './../../../../../../pages/JeeAccount/_core/services/remind.service';
import { ChangeDetectorRef, Component, OnInit } from '@angular/core';
import { LayoutService } from '../../../../../core';
import { Observable } from 'rxjs';
import { UserModel } from '../../../../../../modules/auth/_models/user.model';
import { AuthService } from '../../../../../../modules/auth/_services/auth.service';
import { TranslateService } from '@ngx-translate/core';
import { MenuServices } from 'src/app/_metronic/core/services/menu.service';
import { Router } from '@angular/router';
import { environment } from 'src/environments/environment';

const HOST_JEELANDINGPAGE = environment.HOST_JEELANDINGPAGE;
@Component({
  selector: 'app-user-offcanvas',
  templateUrl: './user-offcanvas.component.html',
  styleUrls: ['./user-offcanvas.component.scss'],
})
export class UserOffcanvasComponent implements OnInit {
  extrasUserOffcanvasDirection = 'offcanvas-right';
  user$: Observable<UserModel>;
  listNhacNho: any[] = [];
  AppCode: string = environment.APPCODE;

  constructor(
    private layout: LayoutService,
    private auth: AuthService,
    public translate: TranslateService,
    private menuServices: MenuServices,
    private changeDetectorRefs: ChangeDetectorRef,
    private router: Router,
    private remindService: RemindService
  ) {}

  ngOnInit(): void {
    this.extrasUserOffcanvasDirection = `offcanvas-${this.layout.getProp('extras.user.offcanvas.direction')}`;
    this.user$ = this.auth.getAuthFromLocalStorage();
    setTimeout(() => {
      this.remindService.connectToken();
    }, 500);
    this.LoadDataNhacNho();
    this.EventNhacNho();
  }

  public EventNhacNho() {
    this.remindService.NewMess$.subscribe((res) => {
      if (res) {
        this.LoadDataNhacNho();
      }
    });
  }

  public LoadDataNhacNho() {
    this.menuServices.Get_DSNhacNho().subscribe((res) => {
      if (res.status == 1) {
        this.listNhacNho = res.data;
        this.changeDetectorRefs.detectChanges();
      }
    });
  }

  quanlytaikhoan() {
    window.open(`${HOST_JEELANDINGPAGE}/ThongTinCaNhan`, '_blank');
  }

  logout() {
    this.auth.logoutToSSO().subscribe((res) => {
      localStorage.removeItem(this.auth.authLocalStorageToken);
      this.auth.logout();
    });
  }

  getNameUser(value: string) {
    return value.substring(0, 1).toUpperCase();
  }

  getColorNameUser(value: any) {
    let result = '';
    switch (value) {
      case 'A':
        return (result = 'rgb(51 152 219)');
      case 'Ă':
        return (result = 'rgb(241, 196, 15)');
      case 'Â':
        return (result = 'rgb(142, 68, 173)');
      case 'B':
        return (result = '#0cb929');
      case 'C':
        return (result = 'rgb(91, 101, 243)');
      case 'D':
        return (result = 'rgb(44, 62, 80)');
      case 'Đ':
        return (result = 'rgb(127, 140, 141)');
      case 'E':
        return (result = 'rgb(26, 188, 156)');
      case 'Ê':
        return (result = 'rgb(51 152 219)');
      case 'G':
        return (result = 'rgb(241, 196, 15)');
      case 'H':
        return (result = 'rgb(248, 48, 109)');
      case 'I':
        return (result = 'rgb(142, 68, 173)');
      case 'K':
        return (result = '#2209b7');
      case 'L':
        return (result = 'rgb(44, 62, 80)');
      case 'M':
        return (result = 'rgb(127, 140, 141)');
      case 'N':
        return (result = 'rgb(197, 90, 240)');
      case 'O':
        return (result = 'rgb(51 152 219)');
      case 'Ô':
        return (result = 'rgb(241, 196, 15)');
      case 'Ơ':
        return (result = 'rgb(142, 68, 173)');
      case 'P':
        return (result = '#02c7ad');
      case 'Q':
        return (result = 'rgb(211, 84, 0)');
      case 'R':
        return (result = 'rgb(44, 62, 80)');
      case 'S':
        return (result = 'rgb(127, 140, 141)');
      case 'T':
        return (result = '#bd3d0a');
      case 'U':
        return (result = 'rgb(51 152 219)');
      case 'Ư':
        return (result = 'rgb(241, 196, 15)');
      case 'V':
        return (result = '#759e13');
      case 'X':
        return (result = 'rgb(142, 68, 173)');
      case 'W':
        return (result = 'rgb(211, 84, 0)');
    }
    return result;
  }

  ChangeLink(item) {
    if (item.WebAppLink != null && item.WebAppLink != '') {
      if (this.AppCode == item.AppCode) {
        this.router.navigate([item.WebAppLink]);
      } else {
        let link = environment.HOST_JEELANDINGPAGE + item.WebAppLink;
        window.open(link, '_blank');
      }
    } else {
      window.open(environment.HOST_JEELANDINGPAGE, '_blank');
    }
  }
}
