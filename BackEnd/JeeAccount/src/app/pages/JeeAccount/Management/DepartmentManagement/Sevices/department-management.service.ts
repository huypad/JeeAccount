import { QueryParamsModelNew } from './../../../_core/models/query-models/query-params.model';
import { HttpClient } from '@angular/common/http';
import { BehaviorSubject, Observable } from 'rxjs';
import { Injectable } from '@angular/core';
import { DepartmentModel, DepChangeTinhTrangModel } from '../Model/department-management.model';
import { environment } from 'src/environments/environment';
import { HttpUtilsService } from '../../../_core/utils/http-utils.service';

const API_PRODUCTS_URL = environment.HOST_JEEACCOUNT_API + '/api/accountdepartmentmanagement';

@Injectable()
export class DepartmentManagementService {
  lastFilter$: BehaviorSubject<QueryParamsModelNew> = new BehaviorSubject(new QueryParamsModelNew({}, 'asc', '', 0, 50));

  constructor(private http: HttpClient, private httpUtils: HttpUtilsService) { }

  findData(queryParams: QueryParamsModelNew): Observable<any> {
    const httpHeaders = this.httpUtils.getHTTPHeaders();
    const httpParams = this.httpUtils.getFindHTTPParams(queryParams);
    const url = API_PRODUCTS_URL + '/GetListDepartment';
    return this.http.get<any>(url, {
      headers: httpHeaders,
      params: httpParams,
    });
  }
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
