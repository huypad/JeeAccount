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
import { FormsModule } from '@angular/forms';

@NgModule({
  declarations: [
    JeeCommentComponent,
    JeeCommentPostContentComponent,
    JeeCommentReactionContentComponent
  ],
  imports: [CommonModule, MatChipsModule, NgxMatSelectSearchModule, InlineSVGModule, MatIconModule, MatInputModule,
    MatFormFieldModule, MatTooltipModule, FormsModule
  ],
  providers: [
    JeeCommentService],
  entryComponents: [JeeCommentComponent, JeeCommentPostContentComponent, JeeCommentReactionContentComponent],
  exports: [JeeCommentComponent, JeeCommentPostContentComponent, JeeCommentReactionContentComponent],
})
export class JeeCommentModule { }
