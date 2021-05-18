import { NgModule } from '@angular/core';
import { CommonModule } from '@angular/common';
import { JeeAccountModule } from '../../jee-account.module';
import { RouterModule, Routes } from '@angular/router';
import { AccountManagementLístComponent } from './account-management-list/account-management-list.component';
import { AccountManagementService } from './Services/account-management.service';
import { AccountManagementComponent } from './account-management.component';
import { AccountManagementEditDialogComponent } from './account-management-edit-dialog/account-management-edit-dialog.component';
import { MAT_DIALOG_DEFAULT_OPTIONS } from '@angular/material/dialog';
import {NgxMatSelectSearchModule} from 'ngx-mat-select-search';
import {InlineSVGModule} from 'ng-inline-svg';

const routes: Routes = [
  {
    path: '',
    component: AccountManagementComponent,
    children: [
      {
        path: '',
        component: AccountManagementLístComponent,
      },
    ],
  },
];

@NgModule({
  declarations: [AccountManagementLístComponent, AccountManagementComponent, AccountManagementEditDialogComponent],
  imports: [CommonModule, RouterModule.forChild(routes), JeeAccountModule, NgxMatSelectSearchModule, InlineSVGModule],
  exports: [AccountManagementEditDialogComponent],
  providers: [
    AccountManagementService,
    { provide: MAT_DIALOG_DEFAULT_OPTIONS, useValue: { hasBackdrop: true, height: 'auto', width: '900px' } },
  ],
  entryComponents: [AccountManagementEditDialogComponent],
})
export class AccountManagementModule {}
