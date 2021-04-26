import { HttpClient } from '@angular/common/http';
import { BehaviorSubject, Observable } from 'rxjs';
import { environment } from '../../../../../environments/environment';
import { HttpUtilsService } from '../../_core/utils/http-utils.service';
import { Injectable } from '@angular/core';
import { QueryParamsModelNew } from '../../_core/models/query-models/query-params.model';
import { ResultModel } from '../../_core/models/_base.model';
import { DepartmentModel, DepChangeTinhTrangModel } from '../Model/department-management.model';

const API_PRODUCTS_URL = environment.ApiRoot + '/departmentmanagement';

@Injectable()
export class DepartmentManagementService {
  lastFilter$: BehaviorSubject<QueryParamsModelNew> = new BehaviorSubject(new QueryParamsModelNew({}, 'asc', '', 0, 50));

  constructor(private http: HttpClient, private httpUtils: HttpUtilsService) {}

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
