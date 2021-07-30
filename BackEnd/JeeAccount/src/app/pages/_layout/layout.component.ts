import { Component, OnInit, ViewChild, ElementRef, AfterViewInit, NgZone, ChangeDetectorRef } from '@angular/core';
import { LayoutService, LayoutInitService } from '../../_metronic/core';
import KTLayoutContent from '../../../assets/js/layout/base/content';
import { ChatService } from '../JeeAccount/my-chat/services/chat.service';
import { AuthService } from 'src/app/modules/auth';
import { UserChatBox } from '../JeeAccount/my-chat/models/user-chatbox';

@Component({
  selector: 'app-layout',
  templateUrl: './layout.component.html',
  styleUrls: ['./layout.component.scss'],
})
export class LayoutComponent implements OnInit, AfterViewInit {
  //jeechat
  chatBoxUsers: UserChatBox[] = [];
  usernameActived: number;
  // Public variables
  selfLayout = 'default';
  asideSelfDisplay: true;
  asideMenuStatic: true;
  contentClasses = '';
  contentContainerClasses = '';
  subheaderDisplay = true;
  contentExtended: false;
  asideCSSClasses: string;
  asideHTMLAttributes: any = {};
  headerMobileClasses = '';
  headerMobileAttributes = {};
  footerDisplay: boolean;
  footerCSSClasses: string;
  headerCSSClasses: string;
  headerHTMLAttributes: any = {};
  // offcanvases
  extrasSearchOffcanvasDisplay = false;
  extrasNotificationsOffcanvasDisplay = false;
  extrasQuickActionsOffcanvasDisplay = false;
  extrasCartOffcanvasDisplay = false;
  extrasUserOffcanvasDisplay = false;
  extrasQuickPanelDisplay = false;
  extrasScrollTopDisplay = false;
  @ViewChild('ktAside', { static: true }) ktAside: ElementRef;
  @ViewChild('ktHeaderMobile', { static: true }) ktHeaderMobile: ElementRef;
  @ViewChild('ktHeader', { static: true }) ktHeader: ElementRef;

  constructor(
    private initService: LayoutInitService,
    private layout: LayoutService,
    private chatService: ChatService,
    private _ngZone: NgZone,
    private auth: AuthService,
    private changeDetectorRefs: ChangeDetectorRef
  ) {
    this.initService.init();
  }

  ngOnInit(): void {
    // build view by layout config settings
    this.selfLayout = this.layout.getProp('self.layout');
    this.asideSelfDisplay = this.layout.getProp('aside.self.display');
    this.asideMenuStatic = this.layout.getProp('aside.menu.static');
    this.subheaderDisplay = this.layout.getProp('subheader.display');
    this.contentClasses = this.layout.getStringCSSClasses('content');
    this.contentContainerClasses = this.layout.getStringCSSClasses('content_container');
    this.contentExtended = this.layout.getProp('content.extended');
    this.asideHTMLAttributes = this.layout.getHTMLAttributes('aside');
    this.asideCSSClasses = this.layout.getStringCSSClasses('aside');
    this.headerMobileClasses = this.layout.getStringCSSClasses('header_mobile');
    this.headerMobileAttributes = this.layout.getHTMLAttributes('header_mobile');
    this.footerDisplay = this.layout.getProp('footer.display');
    this.footerCSSClasses = this.layout.getStringCSSClasses('footer');
    this.headerCSSClasses = this.layout.getStringCSSClasses('header');
    this.headerHTMLAttributes = this.layout.getHTMLAttributes('header');
    // offcanvases
    if (this.layout.getProp('extras.search.display')) {
      this.extrasSearchOffcanvasDisplay = this.layout.getProp('extras.search.layout') === 'offcanvas';
    }

    if (this.layout.getProp('extras.notifications.display')) {
      this.extrasNotificationsOffcanvasDisplay = this.layout.getProp('extras.notifications.layout') === 'offcanvas';
    }

    if (this.layout.getProp('extras.quickActions.display')) {
      this.extrasQuickActionsOffcanvasDisplay = this.layout.getProp('extras.quickActions.layout') === 'offcanvas';
    }

    if (this.layout.getProp('extras.cart.display')) {
      this.extrasCartOffcanvasDisplay = this.layout.getProp('extras.cart.layout') === 'offcanvas';
    }

    if (this.layout.getProp('extras.user.display')) {
      this.extrasUserOffcanvasDisplay = this.layout.getProp('extras.user.layout') === 'offcanvas';
    }

    this.extrasQuickPanelDisplay = this.layout.getProp('extras.quickPanel.display');

    this.extrasScrollTopDisplay = this.layout.getProp('extras.scrolltop.display');
    this.subscribeToEvents();
  }

  ngAfterViewInit(): void {
    if (this.ktAside) {
      for (const key in this.asideHTMLAttributes) {
        if (this.asideHTMLAttributes.hasOwnProperty(key)) {
          this.ktAside.nativeElement.attributes[key] = this.asideHTMLAttributes[key];
        }
      }
    }

    if (this.ktHeaderMobile) {
      for (const key in this.headerMobileAttributes) {
        if (this.headerMobileAttributes.hasOwnProperty(key)) {
          this.ktHeaderMobile.nativeElement.attributes[key] = this.headerMobileAttributes[key];
        }
      }
    }

    if (this.ktHeader) {
      for (const key in this.headerHTMLAttributes) {
        if (this.headerHTMLAttributes.hasOwnProperty(key)) {
          this.ktHeader.nativeElement.attributes[key] = this.headerHTMLAttributes[key];
        }
      }
    }
    // Init Content
    KTLayoutContent.init('kt_content');
  }

  removeChatBox(event: number) {
    let index = this.chatBoxUsers.findIndex((x) => x.user.IdGroup);
    this.chatBoxUsers = this.chatBoxUsers.filter((x) => x.user.IdGroup !== event);

    if (this.chatBoxUsers.length === 1 && index == 0) {
      this.chatBoxUsers[index].right = 300;
      // this.chatService.reload$.next(true);
    }
    localStorage.setItem('chatboxusers', JSON.stringify(this.chatBoxUsers));
  }

  activedChatBox(event: number) {
    this.usernameActived = event;
    var u = this.chatBoxUsers.find((x) => x.user.IdGroup === event);
    if (u) {
      this.chatBoxUsers = this.chatBoxUsers.filter((x) => x.user.IdGroup !== event); //remove
      this.chatBoxUsers.push(u); // add to end of array
      // localStorage.setItem('chatboxusers', JSON.stringify(this.chatBoxUsers));
    }
  }

  private subscribeToEvents(): void {
    this._ngZone.run(() => {
      this.chatService.OpenMiniChat$.subscribe((res) => {
        const userChatBox: UserChatBox[] = JSON.parse(localStorage.getItem('chatboxusers'));
        if (userChatBox) {
          this.chatBoxUsers = userChatBox;
          this.changeDetectorRefs.detectChanges();
        } else {
          this.chatBoxUsers = [];
        }
      });
    });
  }
}
