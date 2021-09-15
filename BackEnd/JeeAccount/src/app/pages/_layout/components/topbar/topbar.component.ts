import { RemindService } from './../../../JeeAccount/_core/services/remind.service';
import { MenuServices } from 'src/app/_metronic/core/services/menu.service';
import { Component, OnInit, AfterViewInit, ChangeDetectionStrategy, ChangeDetectorRef } from '@angular/core';
import { Observable } from 'rxjs';
import { LayoutService } from '../../../../_metronic/core';
import { AuthService } from '../../../../modules/auth/_services/auth.service';
import { UserModel } from '../../../../modules/auth/_models/user.model';
import KTLayoutQuickSearch from '../../../../../assets/js/layout/extended/quick-search';
import KTLayoutQuickNotifications from '../../../../../assets/js/layout/extended/quick-notifications';
import KTLayoutQuickActions from '../../../../../assets/js/layout/extended/quick-actions';
import KTLayoutQuickCartPanel from '../../../../../assets/js/layout/extended/quick-cart';
import KTLayoutQuickPanel from '../../../../../assets/js/layout/extended/quick-panel';
import KTLayoutQuickUser from '../../../../../assets/js/layout/extended/quick-user';
import KTLayoutHeaderTopbar from '../../../../../assets/js/layout/base/header-topbar';
import { KTUtil } from '../../../../../assets/js/components/util';
import { SocketioService } from 'src/app/pages/JeeAccount/_core/services/socketio.service';
import { UserOffcanvasComponent } from 'src/app/_metronic/partials/layout/extras/offcanvas/user-offcanvas/user-offcanvas.component';
@Component({
  selector: 'app-topbar',
  templateUrl: './topbar.component.html',
  styleUrls: ['./topbar.component.scss'],
})
export class TopbarComponent implements OnInit, AfterViewInit {
  user$: Observable<UserModel>;
  numberInfo: number;
  // tobbar extras
  extraSearchDisplay: boolean;
  extrasSearchLayout: 'offcanvas' | 'dropdown';
  extrasNotificationsDisplay: boolean;
  extrasNotificationsLayout: 'offcanvas' | 'dropdown';
  extrasQuickActionsDisplay: boolean;
  extrasQuickActionsLayout: 'offcanvas' | 'dropdown';
  extrasCartDisplay: boolean;
  extrasCartLayout: 'offcanvas' | 'dropdown';
  extrasQuickPanelDisplay: boolean;
  extrasLanguagesDisplay: boolean;
  extrasUserDisplay: boolean;
  extrasUserLayout: 'offcanvas' | 'dropdown';
  SoLuongNhacNho: number = 0;

  constructor(
    private layout: LayoutService,
    private auth: AuthService,
    public socket: SocketioService,
    private changeDetectorRefs: ChangeDetectorRef,
    private userOffcanvasComponent: UserOffcanvasComponent,
    private menuServices: MenuServices,
    private remind_service: RemindService
  ) {
    this.user$ = this.auth.getAuthFromLocalStorage();
  }

  ngOnInit(): void {
    // topbar extras
    // this.extraSearchDisplay = this.layout.getProp('extras.search.display');
    this.extraSearchDisplay = false;
    this.extrasSearchLayout = this.layout.getProp('extras.search.layout');
    this.extrasNotificationsDisplay = this.layout.getProp('extras.notifications.display');
    this.extrasNotificationsLayout = this.layout.getProp('extras.notifications.layout');
    this.extrasQuickActionsDisplay = this.layout.getProp('extras.quickActions.display');
    this.extrasQuickActionsLayout = this.layout.getProp('extras.quickActions.layout');
    this.extrasCartDisplay = this.layout.getProp('extras.cart.display');
    this.extrasCartLayout = this.layout.getProp('extras.cart.layout');
    this.extrasLanguagesDisplay = this.layout.getProp('extras.languages.display');
    this.extrasUserDisplay = this.layout.getProp('extras.user.display');
    this.extrasUserLayout = this.layout.getProp('extras.user.layout');
    this.extrasQuickPanelDisplay = this.layout.getProp('extras.quickPanel.display');
    this.EventNhacNho();
    this.LoadDataNhacNho();
  }

  ngAfterViewInit(): void {
    KTUtil.ready(() => {
      // Called after ngAfterContentInit when the component's view has been initialized. Applies to components only.
      // Add 'implements AfterViewInit' to the class.
      if (this.extraSearchDisplay && this.extrasSearchLayout === 'offcanvas') {
        KTLayoutQuickSearch.init('kt_quick_search');
      }

      if (this.extrasNotificationsDisplay && this.extrasNotificationsLayout === 'offcanvas') {
        // Init Quick Notifications Offcanvas Panel
        KTLayoutQuickNotifications.init('kt_quick_notifications');
      }

      if (this.extrasQuickActionsDisplay && this.extrasQuickActionsLayout === 'offcanvas') {
        // Init Quick Actions Offcanvas Panel
        KTLayoutQuickActions.init('kt_quick_actions');
      }

      if (this.extrasCartDisplay && this.extrasCartLayout === 'offcanvas') {
        // Init Quick Cart Panel
        KTLayoutQuickCartPanel.init('kt_quick_cart');
      }

      if (this.extrasQuickPanelDisplay) {
        // Init Quick Offcanvas Panel
        KTLayoutQuickPanel.init('kt_quick_panel');
      }

      if (this.extrasUserDisplay && this.extrasUserLayout === 'offcanvas') {
        // Init Quick User Panel
        KTLayoutQuickUser.init('kt_quick_user');
      }

      // Init Header Topbar For Mobile Mode
      KTLayoutHeaderTopbar.init('kt_header_mobile_topbar_toggle');
    });
  }
  updateNumberNoti(value) {
    if (value == true) {
      this.getNotiUnread();
    }
  }

  getNotiUnread() {
    this.socket.getNotificationList('unread').subscribe((res) => {
      let dem = 0;
      res.forEach((x) => dem++);
      this.numberInfo = dem;
      this.changeDetectorRefs.detectChanges();
    });
  }

  onClick() {
    this.menuServices.Get_DSNhacNho().subscribe((res) => {
      if (res.status == 1) {
        this.menuServices.data_share$.next(res.data);
        this.changeDetectorRefs.detectChanges();
      }
    });
  }

  EventNhacNho() {
    this.remind_service.NewMess$.subscribe((res) => {
      this.LoadDataNhacNho();
    });
  }

  LoadDataNhacNho() {
    this.menuServices.Count_SoLuongNhacNho().subscribe((res) => {
      if (res) {
        this.SoLuongNhacNho = res.data;
        this.changeDetectorRefs.detectChanges();
      }
    });
  }
}
