import { MatSelectModule } from '@angular/material/select';
import { FormsModule } from '@angular/forms';
import { AccountManagementService } from './../AccountManagement/Services/account-management.service';
import { PermissionAdminHeThongService } from './services/permision-admin-hethong.service';
import { PermissionManagementAdminAppComponent } from './permission-management-admin-app-list/permission-management-admin-app-list.component';
import { PermissionManagementAdminHeThongComponent } from './permission-management-admin-he-thong-list/permission-management-admin-he-thong-list.component';
import { PermissionManagementComponent } from './permission-management.component';
import { NgModule } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterModule, Routes } from '@angular/router';
import { MAT_DIALOG_DEFAULT_OPTIONS } from '@angular/material/dialog';
import { MatChipsModule } from '@angular/material/chips';
import { NgxMatSelectSearchModule } from 'ngx-mat-select-search';
import { InlineSVGModule } from 'ng-inline-svg';
import { JeeAccountModule } from 'src/app/pages/jee-account.module';
import { MatTabsModule } from '@angular/material/tabs';
import { PermissionAdminAppService } from './services/permision-admin-app.service';
import { PermissionManagementAdminAppEditDialogComponent } from './permission-management-admin-app-edit/permission-management-admin-app-edit.component';
import { PermissionManagementAdminHeThongEditDialogComponent } from './permission-management-admin-he-thong-edit/permission-management-admin-he-thong-edit.component';

const routes: Routes = [
  {
    path: '',
    component: PermissionManagementComponent,
  },
];
@NgModule({
  declarations: [
    PermissionManagementComponent,
    PermissionManagementAdminHeThongComponent,
    PermissionManagementAdminAppComponent,
    PermissionManagementAdminAppEditDialogComponent,
    PermissionManagementAdminHeThongEditDialogComponent
  ],
  imports: [
    RouterModule.forChild(routes),
    CommonModule,
    JeeAccountModule,
    MatChipsModule,
    NgxMatSelectSearchModule,
    InlineSVGModule,
    MatTabsModule,
    FormsModule,
    MatSelectModule,
  ],
  providers: [
    {
      provide: MAT_DIALOG_DEFAULT_OPTIONS,
      useValue: { hasBackdrop: true, height: 'auto', width: '700px', panelClass: 'mat-dialog-container-wrapper', disableClose: true },
    },
    PermissionAdminHeThongService,
    PermissionAdminAppService,
    AccountManagementService,
  ],
  entryComponents: [
    PermissionManagementAdminAppComponent,
    PermissionManagementAdminHeThongComponent,
    PermissionManagementAdminAppEditDialogComponent,
    PermissionManagementAdminHeThongEditDialogComponent,
  ],
  exports: [PermissionManagementComponent, PermissionManagementAdminAppComponent, PermissionManagementAdminHeThongComponent],
})
export class PermissionManagementModule {}
