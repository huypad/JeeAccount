import { HttpClient } from '@angular/common/http';
import { Inject, Injectable, OnDestroy } from '@angular/core';
import { environment } from 'src/environments/environment';
import { HttpUtilsService } from '../../../_core/utils/http-utils.service';
import { ITableService } from '../../../_core/services/itable.service';
import { AccountManagementDTO } from '../../AccountManagement/Model/account-management.model';

const API_PRODUCTS_URL = environment.HOST_JEEACCOUNT_API + '/api/permissionmanagement';

@Injectable()
export class PermissionAdminAppService extends ITableService<AccountManagementDTO[]> implements OnDestroy {
  constructor(@Inject(HttpClient) http, @Inject(HttpUtilsService) httpUtils) {
    super(http, httpUtils);
  }
  API_URL_FIND: string = API_PRODUCTS_URL + '/GetListAccountAdminApp';
  API_URL_CTEATE: string = API_PRODUCTS_URL + '/CreateJobtitle';
  API_URL_EDIT: string = API_PRODUCTS_URL + '/UpdateJobtitle';
  API_URL_DELETE: string = API_PRODUCTS_URL + '/Delete';
  API_URL: string = API_PRODUCTS_URL;

  ngOnDestroy(): void {
    this.subscriptions.forEach((sb) => sb.unsubscribe());
  }
}
