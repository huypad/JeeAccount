import { NgModule } from '@angular/core';
import { CommonModule } from '@angular/common';
import { NgApexchartsModule } from 'ng-apexcharts';
import { InlineSVGModule } from 'ng-inline-svg';
import { NgbDropdownModule } from '@ng-bootstrap/ng-bootstrap';
import { TranslationModule } from 'src/app/modules/i18n/translation.module';
import { JeeAccountModule } from 'src/app/pages/jee-account.module';
import { DropdownMenusModule } from 'src/app/_metronic/partials/content/dropdown-menus/dropdown-menus.module';
import { GridsterModule } from 'angular-gridster2';
import { DynamicModule } from 'ng-dynamic-component';
import { RouterModule } from '@angular/router';
import { MatIconModule } from '@angular/material/icon';
import { MatButtonToggleModule } from '@angular/material/button-toggle';
import { MatSlideToggleModule } from '@angular/material/slide-toggle';
import { PerfectScrollbarModule } from 'ngx-perfect-scrollbar';
import { MatTableModule } from '@angular/material/table';
import { MAT_DIALOG_DEFAULT_OPTIONS } from '@angular/material/dialog';
import { ScrollingModule } from '@angular/cdk/scrolling';
import { DemoCommentService } from './demo-comment.service';
import { DemoCommentComponent } from './demo-comment.component';

@NgModule({
  declarations: [DemoCommentComponent],
  imports: [
    CommonModule,
    DropdownMenusModule,
    InlineSVGModule,
    NgApexchartsModule,
    NgbDropdownModule,
    TranslationModule,
    JeeAccountModule,
    GridsterModule,
    DynamicModule,
    MatIconModule,
    MatButtonToggleModule,
    MatSlideToggleModule,
    PerfectScrollbarModule,
    MatTableModule,
    ScrollingModule,
    RouterModule.forChild([
      {
        path: '',
        component: DemoCommentComponent,
      },
    ]),
  ],
  providers: [DemoCommentService],
  exports: [DemoCommentComponent],
  entryComponents: [DemoCommentComponent],
})
export class DemoCommentModule { }
