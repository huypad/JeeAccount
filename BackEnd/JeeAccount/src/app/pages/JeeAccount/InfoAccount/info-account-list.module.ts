import { NgModule } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterModule, Routes } from '@angular/router';
import { MAT_DIALOG_DEFAULT_OPTIONS, MatDialogModule } from '@angular/material/dialog';
import { NgxMatSelectSearchModule } from 'ngx-mat-select-search';
import { InlineSVGModule } from 'ng-inline-svg';
import { InfoAccountListComponent } from './info-account-list.component';
import { AccountManagementService } from '../Management/AccountManagement/Services/account-management.service';
import { JeeAccountModule } from '../../jee-account.module';

const routes: Routes = [
  {
    path: '',
    component: InfoAccountListComponent,
  },
];

@NgModule({
  declarations: [InfoAccountListComponent],
  imports: [CommonModule, RouterModule.forChild(routes), JeeAccountModule, NgxMatSelectSearchModule, InlineSVGModule],
  providers: [
    AccountManagementService,
    {
      provide: MAT_DIALOG_DEFAULT_OPTIONS,
      useValue: { hasBackdrop: true, height: 'auto', width: '900px', panelClass: 'mat-dialog-container-wrapper', disableClose: true },
    },
  ],
  entryComponents: [],
})
export class InfoAccountModule {}
