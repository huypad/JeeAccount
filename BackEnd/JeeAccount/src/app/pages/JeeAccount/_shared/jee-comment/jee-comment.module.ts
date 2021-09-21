import { AvatarModule } from 'ngx-avatar';
import { TranslationModule } from './../../../../modules/i18n/translation.module';
import { JeeCommentSignalrService } from './jee-comment-signalr.service';
import { JeeCommentReactionShowComponent } from './reaction-comment-show/reaction-comment-show.component';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatIconModule } from '@angular/material/icon';
import { JeeCommentService } from './jee-comment.service';
import { JeeCommentComponent } from './jee-comment.component';
import { NgModule } from '@angular/core';
import { CommonModule } from '@angular/common';
import { MatChipsModule } from '@angular/material/chips';
import { NgxMatSelectSearchModule } from 'ngx-mat-select-search';
import { InlineSVGModule } from 'ng-inline-svg';
import { JeeCommentPostContentComponent } from './post-comment-content/post-comment-content.component';
import { JeeCommentReactionContentComponent } from './reaction-comment-content/reaction-comment-content.component';
import { MatTooltipModule } from '@angular/material/tooltip';
import { FormsModule, ReactiveFormsModule } from '@angular/forms';
import { PickerModule } from '@ctrl/ngx-emoji-mart';
import { JeeCommentEnterCommentContentComponent } from './enter-comment-content/enter-comment-content.component';
import { ClickOutsideDirective } from './enter-comment-content/click-outside.directive';
import { MatMenuModule } from '@angular/material/menu';
import { PopoverModule } from 'ngx-smart-popover';
import { TagCommentShowComponent } from './tag-comment-show/tag-comment-show.component';
import { EmojiModule } from '@ctrl/ngx-emoji-mart/ngx-emoji';
@NgModule({
  declarations: [
    JeeCommentComponent,
    JeeCommentPostContentComponent,
    JeeCommentReactionContentComponent,
    JeeCommentEnterCommentContentComponent,
    JeeCommentReactionShowComponent,
    ClickOutsideDirective,
    TagCommentShowComponent,
  ],
  imports: [
    CommonModule,
    MatChipsModule,
    NgxMatSelectSearchModule,
    InlineSVGModule,
    MatIconModule,
    MatInputModule,
    MatFormFieldModule,
    MatTooltipModule,
    FormsModule,
    PickerModule,
    MatMenuModule,
    TranslationModule,
    PopoverModule,
    AvatarModule,
    ReactiveFormsModule,
    EmojiModule,
  ],
  providers: [JeeCommentService, JeeCommentSignalrService],
  entryComponents: [
    JeeCommentComponent,
    JeeCommentPostContentComponent,
    JeeCommentReactionContentComponent,
    JeeCommentReactionShowComponent,
    TagCommentShowComponent,
  ],
  exports: [
    JeeCommentComponent,
    JeeCommentPostContentComponent,
    JeeCommentReactionContentComponent,
    JeeCommentEnterCommentContentComponent,
    JeeCommentReactionShowComponent,
    TagCommentShowComponent,
  ],
})
export class JeeCommentModule {}
