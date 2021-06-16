import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatIconModule } from '@angular/material/icon';
import { JeeAccountModule } from './../../../jee-account.module';
import { JeeCommentService } from './jee-comment.service';
import { JeeCommentComponent } from './jee-comment.component';
import { NgModule } from '@angular/core';
import { CommonModule } from '@angular/common';
import { MAT_DIALOG_DEFAULT_OPTIONS } from '@angular/material/dialog';
import { MatChipsModule } from '@angular/material/chips';
import { NgxMatSelectSearchModule } from 'ngx-mat-select-search';
import { InlineSVGModule } from 'ng-inline-svg';


@NgModule({
  declarations: [
    JeeCommentComponent,
  ],
  imports: [CommonModule, MatChipsModule, NgxMatSelectSearchModule, InlineSVGModule, MatIconModule, MatInputModule, MatFormFieldModule
  ],
  providers: [
    JeeCommentService,
    { provide: MAT_DIALOG_DEFAULT_OPTIONS, useValue: { hasBackdrop: true, height: 'auto', width: '900px' } },
  ],
  entryComponents: [JeeCommentComponent],
  exports: [JeeCommentComponent],
})
export class JeeCommentModule { }
