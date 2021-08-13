import { NgModule } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterModule, Routes } from '@angular/router';
import { MAT_DIALOG_DEFAULT_OPTIONS } from '@angular/material/dialog';
import { JobtitleManagementComponent } from './Jobtitle-management.component';
import { JobtitleManagementListComponent } from './Jobtitle-management-list/Jobtitle-management-list.component';
import { JobtitleManagementEditDialogComponent } from './Jobtitle-management-edit-dialog/Jobtitle-management-edit-dialog.component';
import { MatChipsModule } from '@angular/material/chips';
import { NgxMatSelectSearchModule } from 'ngx-mat-select-search';
import { InlineSVGModule } from 'ng-inline-svg';
import { JeeAccountModule } from 'src/app/pages/jee-account.module';
import { ChangeTinhTrangJobtitleEditDialogComponent } from './change-tinh-trang-jobtitle-edit-dialog/change-tinh-trang-jobtitile-edit-dialog.component';
import { JobtitleManagementService } from './Sevices/jobtitle-management.service';

const routes: Routes = [
  {
    path: '',
    component: JobtitleManagementComponent,
    children: [
      {
        path: '',
        component: JobtitleManagementListComponent,
      },
    ],
  },
];

@NgModule({
  declarations: [
    JobtitleManagementComponent,
    JobtitleManagementListComponent,
    JobtitleManagementEditDialogComponent,
    ChangeTinhTrangJobtitleEditDialogComponent,
  ],
  imports: [CommonModule, RouterModule.forChild(routes), JeeAccountModule, MatChipsModule, NgxMatSelectSearchModule, InlineSVGModule],
  providers: [
    JobtitleManagementService,
    {
      provide: MAT_DIALOG_DEFAULT_OPTIONS,
      useValue: { hasBackdrop: true, height: 'auto', width: '700px', panelClass: 'mat-dialog-container-wrapper', disableClose: true },
    },
  ],
  entryComponents: [JobtitleManagementEditDialogComponent, ChangeTinhTrangJobtitleEditDialogComponent],
  exports: [JobtitleManagementListComponent],
})
export class JobtitleManagementModule {}
