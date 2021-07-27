import { NgModule } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterModule, Routes } from '@angular/router';
import { AccountManagementListComponent } from './account-management-list/account-management-list.component';
import { AccountManagementService } from './Services/account-management.service';
import { AccountManagementComponent } from './account-management.component';
import { AccountManagementEditDialogComponent } from './account-management-edit-dialog/account-management-edit-dialog.component';
import { MAT_DIALOG_DEFAULT_OPTIONS } from '@angular/material/dialog';
import { NgxMatSelectSearchModule } from 'ngx-mat-select-search';
import { InlineSVGModule } from 'ng-inline-svg';
import { QuanLytrucTiepEditDialogComponent } from './quan-ly-truc-tiep-edit-dialog/quan-ly-truc-tiep-edit-dialog.component';
import { ChangeTinhTrangEditDialogComponent } from './change-tinh-trang-edit-dialog/change-tinh-trang-edit-dialog.component';
import { AccountManagementEditNoJeeHRDialogComponent } from './account-management-edit-no-jeehr-dialog/account-management-edit-no-jeehr-dialog.component';
import { JeeAccountModule } from 'src/app/pages/jee-account.module';
const routes: Routes = [
  {
    path: '',
    component: AccountManagementComponent,
    children: [
      {
        path: '',
        component: AccountManagementListComponent,
      },
    ],
  },
];

@NgModule({
  declarations: [
    AccountManagementListComponent,
    AccountManagementComponent,
    AccountManagementEditDialogComponent,
    QuanLytrucTiepEditDialogComponent,
    ChangeTinhTrangEditDialogComponent,
    AccountManagementEditNoJeeHRDialogComponent,
  ],
  imports: [CommonModule, RouterModule.forChild(routes), JeeAccountModule, NgxMatSelectSearchModule, InlineSVGModule],
  providers: [
    AccountManagementService,
    { provide: MAT_DIALOG_DEFAULT_OPTIONS, useValue: { hasBackdrop: true, height: 'auto', width: '900px' } },
  ],
  entryComponents: [
    AccountManagementEditDialogComponent,
    QuanLytrucTiepEditDialogComponent,
    ChangeTinhTrangEditDialogComponent,
    AccountManagementEditNoJeeHRDialogComponent,
  ],
})
export class AccountManagementModule { }
