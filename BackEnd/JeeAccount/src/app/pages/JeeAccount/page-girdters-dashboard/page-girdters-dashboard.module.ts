import { NgModule } from '@angular/core';
import { CommonModule } from '@angular/common';
import { NgApexchartsModule } from 'ng-apexcharts';
import { InlineSVGModule } from 'ng-inline-svg';
import { NgbDropdownModule } from '@ng-bootstrap/ng-bootstrap';
import { TranslationModule } from 'src/app/modules/i18n/translation.module';
import { JeeAccountModule } from 'src/app/pages/jee-account.module';
import { PageGirdtersDashboardService } from './Services/page-girdters-dashboard.service';
import { DropdownMenusModule } from 'src/app/_metronic/partials/content/dropdown-menus/dropdown-menus.module';
import { PageGidtersDashboardComponent } from './page-girdters-dashboard.component';
import { GridsterModule } from 'angular-gridster2';
import { DynamicModule } from 'ng-dynamic-component';
import { RouterModule } from '@angular/router';
import { MatIconModule } from '@angular/material/icon';
import { DanhMucChungService } from '../_core/services/danhmuc.service';
import { MatButtonToggleModule } from '@angular/material/button-toggle';
import { MatSlideToggleModule } from '@angular/material/slide-toggle';
import { AddCloseWidgetDialogComponent } from './widgets/add-close-widget-dialog/add-close-widget-dialog.component';
import { DanhSachTruyCapNhanhWidgetComponent } from './widgets/danh-sach-truy-cap-nhanh/danh-sach-truy-cap-nhanh.component';
import { PerfectScrollbarModule } from 'ngx-perfect-scrollbar';
import { MatTableModule } from '@angular/material/table';
import { DepartmentManagementService } from '../Management/DepartmentManagement/Sevices/department-management.service';
import { AccountManagementService } from '../Management/AccountManagement/Services/account-management.service';
import { MAT_DIALOG_DEFAULT_OPTIONS } from '@angular/material/dialog';
import { DemoCommentService } from './Services/demo-comment.service';

@NgModule({
  declarations: [PageGidtersDashboardComponent, AddCloseWidgetDialogComponent, DanhSachTruyCapNhanhWidgetComponent],
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
    RouterModule.forChild([
      {
        path: '',
        component: PageGidtersDashboardComponent,
      },
    ]),
  ],
  providers: [PageGirdtersDashboardService, DanhMucChungService, DepartmentManagementService,
    AccountManagementService, DemoCommentService,
    { provide: MAT_DIALOG_DEFAULT_OPTIONS, useValue: { hasBackdrop: true, height: 'auto', width: '900px' } },
  ],
  exports: [PageGidtersDashboardComponent, AddCloseWidgetDialogComponent, DanhSachTruyCapNhanhWidgetComponent],
  entryComponents: [AddCloseWidgetDialogComponent, DanhSachTruyCapNhanhWidgetComponent],
})
export class PageGirdtersDashboardModule { }
