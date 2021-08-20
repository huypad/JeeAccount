import { NgModule } from '@angular/core';
import { Routes, RouterModule } from '@angular/router';
import { LayoutComponent } from './_layout/layout.component';

const routes: Routes = [
  {
    path: '',
    component: LayoutComponent,
    children: [
      {
        path: 'builder',
        loadChildren: () => import('./builder/builder.module').then((m) => m.BuilderModule),
      },
      {
        path: 'comments',
        loadChildren: () => import('./JeeAccount/Management/DemoComemnt/demo-comment.module').then((m) => m.DemoCommentModule),
      },
      {
        path: 'dashboard',
        loadChildren: () =>
          import('./JeeAccount/page-girdters-dashboard/page-girdters-dashboard.module').then((m) => m.PageGirdtersDashboardModule),
      },
      {
        path: 'Management/AccountManagement',
        loadChildren: () =>
          import('./JeeAccount/Management/AccountManagement/account-management.module').then((m) => m.AccountManagementModule),
      },
      {
        path: 'Management/DepartmentManagement',
        loadChildren: () =>
          import('./JeeAccount/Management/DepartmentManagement/department-management.module').then((m) => m.DepartmentManagementModule),
      },
      {
        path: 'Management/JobtitleManagement',
        loadChildren: () =>
          import('./JeeAccount/Management/JobtitleManagement/jobtitle-management.module').then((m) => m.JobtitleManagementModule),
      },
      {
        path: 'Management/PermissionManagement',
        loadChildren: () =>
          import('./JeeAccount/Management/PermissionManagement/permission-management.module').then((m) => m.PermissionManagementModule),
      },
      {
        path: '',
        redirectTo: '/Management/AccountManagement',
        pathMatch: 'full',
      },
      {
        path: '**',
        redirectTo: 'error/404',
      },
    ],
  },
];

@NgModule({
  imports: [RouterModule.forChild(routes)],
  exports: [RouterModule],
})
export class PagesRoutingModule {}
