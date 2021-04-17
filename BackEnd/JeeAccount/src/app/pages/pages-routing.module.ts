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
        path: 'dashboard',
        loadChildren: () =>
          import('./JeeAccount/page-girdters-dashboard/page-girdters-dashboard.module').then((m) => m.PageGirdtersDashboardModule),
      },
      {
        path: 'AccountManagement',
        loadChildren: () => import('./JeeAccount/AccountManagement/account-management.module').then((m) => m.AccountManagementModule),
      },
      {
        path: 'DepartmentManagement',
        loadChildren: () => import('./JeeAccount/DepartmentManagement/department-management.module').
          then((m) => m.DepartmentManagementModule),
      },
      {
        path: '',
        redirectTo: '/AccountManagement',
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
