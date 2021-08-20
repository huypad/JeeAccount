import { NhanVienMatchip } from './../../../_core/models/danhmuc.model';
import { Observable } from 'rxjs';
import { HttpClient } from '@angular/common/http';
import { Inject, Injectable } from '@angular/core';
import { environment } from 'src/environments/environment';
import { HttpUtilsService } from '../../../_core/utils/http-utils.service';
import { ITableService } from '../../../_core/services/itable.service';
import { AccountManagementDTO } from '../../AccountManagement/Model/account-management.model';
import { AccModel } from '../model/permision-management.model';

const API_PRODUCTS_URL = environment.HOST_JEEACCOUNT_API + '/api/permissionmanagement';

@Injectable()
export class PermissionAdminHeThongService extends ITableService<AccountManagementDTO[]> {
  constructor(@Inject(HttpClient) http, @Inject(HttpUtilsService) httpUtils) {
    super(http, httpUtils);
  }
  API_URL_FIND: string = API_PRODUCTS_URL + '/GetListAccountAdminHeThong';
  API_URL_CTEATE: string = API_PRODUCTS_URL + '/CreateJobtitle';
  API_URL_EDIT: string = API_PRODUCTS_URL + '/UpdateJobtitle';
  API_URL_DELETE: string = API_PRODUCTS_URL + '/Delete';
  API_URL: string = API_PRODUCTS_URL;

  ngOnDestroy(): void {
    this.subscriptions.forEach((sb) => sb.unsubscribe());
  }

  createAdminHeThong(item: NhanVienMatchip): Observable<any> {
    const httpHeaders = this.httpUtils.getHTTPHeaders();
    const url = API_PRODUCTS_URL + `/CreateAdminHeThong`;
    return this.http.post<any>(url, item, { headers: httpHeaders });
  }

  RemoveAdminHeThong(userid: number): Observable<any> {
    const httpHeaders = this.httpUtils.getHTTPHeaders();
    const url = API_PRODUCTS_URL + `/RemoveAdminHeThong/${userid}`;
    return this.http.delete<any>(url, { headers: httpHeaders });
  }
}
