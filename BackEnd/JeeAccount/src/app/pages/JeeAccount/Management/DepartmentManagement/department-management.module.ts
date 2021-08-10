import { NgModule } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterModule, Routes } from '@angular/router';
import { MAT_DIALOG_DEFAULT_OPTIONS } from '@angular/material/dialog';
import { DepartmentManagementComponent } from './department-management.component';
import { DepartmentManagementListComponent } from './department-management-list/department-management-list.component';
import { DepartmentManagementService } from './Sevices/department-management.service';
import { DepartmentManagementEditDialogComponent } from './department-management-edit-dialog/department-management-edit-dialog.component';
import { MatChipsModule } from '@angular/material/chips';
import { NgxMatSelectSearchModule } from 'ngx-mat-select-search';
import { InlineSVGModule } from 'ng-inline-svg';
import { DepartmentChangeTinhTrangEditDialogComponent } from './department-change-tinh-trang-edit-dialog/department-change-tinh-trang-edit-dialog.component';
import { JeeAccountModule } from 'src/app/pages/jee-account.module';

const routes: Routes = [
  {
    path: '',
    component: DepartmentManagementComponent,
    children: [
      {
        path: '',
        component: DepartmentManagementListComponent,
      },
    ],
  },
];

@NgModule({
  declarations: [
    DepartmentManagementComponent,
    DepartmentManagementListComponent,
    DepartmentManagementEditDialogComponent,
    DepartmentChangeTinhTrangEditDialogComponent,
  ],
  imports: [CommonModule, RouterModule.forChild(routes), JeeAccountModule, MatChipsModule, NgxMatSelectSearchModule, InlineSVGModule],
  providers: [DepartmentManagementService],
  entryComponents: [DepartmentManagementEditDialogComponent, DepartmentChangeTinhTrangEditDialogComponent],
  exports: [DepartmentManagementListComponent],
})
export class DepartmentManagementModule {}
