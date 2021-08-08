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

import { JeeAccountModule } from 'src/app/pages/jee-account.module';
import { AccountManagementChinhSuaNoJeeHRDialogComponent } from './account-management-chinhsua-nojeehr-dialog/account-management-chinhsua-nojeehr-dialog.component';
import { AccountManagementChinhSuaJeeHRDialogComponent } from './account-management-chinhsua-jeehr-dialog/account-management-chinhsua-jeehr-dialog.component';
import { AccountManagementEditJeeHRDialogComponent } from './account-management-edit-jeehr-dialog/account-management-edit-jeehr-dialog.component';
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
    AccountManagementChinhSuaNoJeeHRDialogComponent,
    AccountManagementChinhSuaJeeHRDialogComponent,
    AccountManagementEditJeeHRDialogComponent,
  ],
  imports: [CommonModule, RouterModule.forChild(routes), JeeAccountModule, NgxMatSelectSearchModule, InlineSVGModule],
  providers: [
    AccountManagementService,
    {
      provide: MAT_DIALOG_DEFAULT_OPTIONS,
      useValue: { hasBackdrop: true, height: 'auto', width: '900px', panelClass: 'mat-dialog-container-wrapper', disableClose: true },
    },
  ],
  entryComponents: [
    AccountManagementEditDialogComponent,
    QuanLytrucTiepEditDialogComponent,
    ChangeTinhTrangEditDialogComponent,
    AccountManagementChinhSuaNoJeeHRDialogComponent,
    AccountManagementChinhSuaJeeHRDialogComponent,
    AccountManagementEditJeeHRDialogComponent,
  ],
})
export class AccountManagementModule {}
