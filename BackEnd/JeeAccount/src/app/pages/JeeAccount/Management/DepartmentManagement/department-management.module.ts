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
import { JeeAccountModule } from 'src/app/pages/jee-account.module';
import { ChangeTinhTrangDepartmentEditDialogComponent } from './change-tinh-trang-department-edit-dialog/change-tinh-trang-department-edit-dialog.component';
import { DepartmentQuanLytrucTiepEditDialogComponent } from './department-quan-ly-truc-tiep-edit-dialog/department-quan-ly-truc-tiep-edit-dialog.component';
import { DropdownTreeModule } from 'dps-lib';
import { CdkTreeModule } from '@angular/cdk/tree';

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
    ChangeTinhTrangDepartmentEditDialogComponent,
    DepartmentQuanLytrucTiepEditDialogComponent,
  ],
  imports: [
    CommonModule,
    RouterModule.forChild(routes),
    JeeAccountModule,
    MatChipsModule,
    NgxMatSelectSearchModule,
    InlineSVGModule,
    DropdownTreeModule,
    CdkTreeModule,
  ],
  providers: [
    DepartmentManagementService,
    {
      provide: MAT_DIALOG_DEFAULT_OPTIONS,
      useValue: { hasBackdrop: true, height: 'auto', width: '700px', panelClass: 'mat-dialog-container-wrapper', disableClose: true },
    },
  ],
  entryComponents: [
    DepartmentManagementEditDialogComponent,
    ChangeTinhTrangDepartmentEditDialogComponent,
    DepartmentQuanLytrucTiepEditDialogComponent,
  ],
  exports: [DepartmentManagementListComponent],
})
export class DepartmentManagementModule {}
