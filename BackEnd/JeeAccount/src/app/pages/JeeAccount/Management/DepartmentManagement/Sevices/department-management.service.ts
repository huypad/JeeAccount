import { DepartmentManagement } from './../Model/department-management.model';
import { QueryParamsModelNew } from './../../../_core/models/query-models/query-params.model';
import { HttpClient } from '@angular/common/http';
import { BehaviorSubject, Observable } from 'rxjs';
import { Inject, Injectable } from '@angular/core';
import { DepartmentModel, DepChangeTinhTrangModel } from '../Model/department-management.model';
import { environment } from 'src/environments/environment';
import { HttpUtilsService } from '../../../_core/utils/http-utils.service';
import { ITableService } from '../../../_core/services/itable.service';

const API_PRODUCTS_URL = environment.HOST_JEEACCOUNT_API + '/api/accountdepartmentmanagement';

@Injectable()
export class DepartmentManagementService extends ITableService<DepartmentManagement> {
  constructor(@Inject(HttpClient) http, @Inject(HttpUtilsService) httpUtils) {
    super(http, httpUtils);
  }
  API_URL_FIND: string = API_PRODUCTS_URL + '/GetListDepartmentManagement';
  API_URL_CTEATE: string = API_PRODUCTS_URL + '/CreateDepartment';
  API_URL_EDIT: string = API_PRODUCTS_URL + '/UpdateDepartment';
  API_URL_DELETE: string = API_PRODUCTS_URL + '/Delete';
  API_URL: string = API_PRODUCTS_URL;

  createDepart(item: DepartmentModel): Observable<any> {
    const httpHeaders = this.httpUtils.getHTTPHeaders();
    const url = API_PRODUCTS_URL + '/CreateDepartment';
    return this.http.post<any>(url, item, { headers: httpHeaders });
  }
  changeTinhTrang(acc: DepChangeTinhTrangModel): Observable<any> {
    const httpHeaders = this.httpUtils.getHTTPHeaders();
    const url = API_PRODUCTS_URL + `/ChangeTinhTrang`;

    return this.http.post<any>(url, acc, {
      headers: httpHeaders,
    });
  }
}
