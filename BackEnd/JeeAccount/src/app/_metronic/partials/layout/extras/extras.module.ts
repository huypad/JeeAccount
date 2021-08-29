import { NgxAutoScrollModule } from 'ngx-auto-scroll';
import { MatIconModule } from '@angular/material/icon';
import { NgModule } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterModule } from '@angular/router';
import { InlineSVGModule } from 'ng-inline-svg';
import { PerfectScrollbarModule } from 'ngx-perfect-scrollbar';
import { PERFECT_SCROLLBAR_CONFIG } from 'ngx-perfect-scrollbar';
import { PerfectScrollbarConfigInterface } from 'ngx-perfect-scrollbar';
import { SearchDropdownInnerComponent } from './dropdown-inner/search-dropdown-inner/search-dropdown-inner.component';
import { NotificationsDropdownInnerComponent } from './dropdown-inner/notifications-dropdown-inner/notifications-dropdown-inner.component';
import { QuickActionsDropdownInnerComponent } from './dropdown-inner/quick-actions-dropdown-inner/quick-actions-dropdown-inner.component';
import { CartDropdownInnerComponent } from './dropdown-inner/cart-dropdown-inner/cart-dropdown-inner.component';
import { UserDropdownInnerComponent } from './dropdown-inner/user-dropdown-inner/user-dropdown-inner.component';
import { SearchOffcanvasComponent } from './offcanvas/search-offcanvas/search-offcanvas.component';
import { SearchResultComponent } from './dropdown-inner/search-dropdown-inner/search-result/search-result.component';
import { NotificationsOffcanvasComponent } from './offcanvas/notifications-offcanvas/notifications-offcanvas.component';
import { QuickActionsOffcanvasComponent } from './offcanvas/quick-actions-offcanvas/quick-actions-offcanvas.component';
import { CartOffcanvasComponent } from './offcanvas/cart-offcanvas/cart-offcanvas.component';
import { QuickPanelOffcanvasComponent } from './offcanvas/quick-panel-offcanvas/quick-panel-offcanvas.component';
import { UserOffcanvasComponent } from './offcanvas/user-offcanvas/user-offcanvas.component';
import { CoreModule } from '../../../core';
import { ScrollTopComponent } from './scroll-top/scroll-top.component';
import { ToolbarComponent } from './toolbar/toolbar.component';
import { AvatarModule } from 'ngx-avatar';
import { MatTooltipModule } from '@angular/material/tooltip';
import { TranslateModule } from '@ngx-translate/core';
import { SocketioService } from 'src/app/pages/JeeAccount/_core/services/socketio.service';
import { CreateConversationUserComponent } from './create-conversation-user/create-conversation-user.component';
import { CreateConvesationGroupComponent } from './create-convesation-group/create-convesation-group.component';
import { FormsModule, ReactiveFormsModule } from '@angular/forms';
import { ConversationService } from 'src/app/pages/JeeAccount/my-chat/services/conversation.service';
import { SoundService } from 'src/app/pages/JeeAccount/my-chat/services/sound.service';
import { ChatService } from 'src/app/pages/JeeAccount/my-chat/services/chat.service';
import { MessageService } from 'src/app/pages/JeeAccount/my-chat/services/message.service';
import { PresenceService } from 'src/app/pages/JeeAccount/my-chat/services/presence.service';
import { ChatBoxComponent } from './dropdown-inner/chat-box/chat-box.component';
import { MessengerComponent } from './dropdown-inner/messenger/messenger.component';
import { MatInputModule } from '@angular/material/input';
import { MatAutocompleteModule } from '@angular/material/autocomplete';
import { MatChipsModule } from '@angular/material/chips';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatButtonModule } from '@angular/material/button';
import { CollapseModule } from 'ngx-bootstrap/collapse';
import { FilterPipe } from './filter.pipe';
import { ScrollingModule } from '@angular/cdk/scrolling';
import { RemindService } from 'src/app/pages/JeeAccount/_core/services/remind.service';

const DEFAULT_PERFECT_SCROLLBAR_CONFIG: PerfectScrollbarConfigInterface = {
  suppressScrollX: true,
};

@NgModule({
  declarations: [
    FilterPipe,
    SearchDropdownInnerComponent,
    NotificationsDropdownInnerComponent,
    QuickActionsDropdownInnerComponent,
    CartDropdownInnerComponent,
    UserDropdownInnerComponent,
    SearchOffcanvasComponent,
    SearchResultComponent,
    NotificationsOffcanvasComponent,
    QuickActionsOffcanvasComponent,
    CartOffcanvasComponent,
    QuickPanelOffcanvasComponent,
    UserOffcanvasComponent,
    ScrollTopComponent,
    ToolbarComponent,
    CreateConversationUserComponent,
    CreateConvesationGroupComponent,
    ChatBoxComponent,
    MessengerComponent,
    FilterPipe,
  ],
  imports: [
    NgxAutoScrollModule,
    CommonModule,
    InlineSVGModule,
    PerfectScrollbarModule,
    CoreModule,
    RouterModule,
    AvatarModule,
    MatTooltipModule,
    TranslateModule,
    ReactiveFormsModule,
    MatInputModule,
    PerfectScrollbarModule,
    CoreModule,
    RouterModule,
    MatFormFieldModule,
    MatChipsModule,
    MatAutocompleteModule,
    MatButtonModule,
    FormsModule,
    CollapseModule,
    MatIconModule,
    ScrollingModule,
  ],
  providers: [
    {
      provide: PERFECT_SCROLLBAR_CONFIG,
      useValue: DEFAULT_PERFECT_SCROLLBAR_CONFIG,
    },
    SocketioService,
    ConversationService,
    SoundService,
    ChatService,
    MessageService,
    PresenceService,
    RemindService,
  ],
  exports: [
    SearchDropdownInnerComponent,
    NotificationsDropdownInnerComponent,
    QuickActionsDropdownInnerComponent,
    CartDropdownInnerComponent,
    UserDropdownInnerComponent,
    SearchOffcanvasComponent,
    NotificationsOffcanvasComponent,
    QuickActionsOffcanvasComponent,
    CartOffcanvasComponent,
    QuickPanelOffcanvasComponent,
    UserOffcanvasComponent,
    ToolbarComponent,
    ScrollTopComponent,
    FilterPipe,
    CreateConversationUserComponent,
    CreateConvesationGroupComponent,
    ChatBoxComponent,
    MessengerComponent,
  ],
})
export class ExtrasModule {}
