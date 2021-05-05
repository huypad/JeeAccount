import { HttpClient } from '@angular/common/http';
import { BehaviorSubject, Observable } from 'rxjs';
import { Injectable } from '@angular/core';
import {
  AccChangeTinhTrangModel,
  AccDirectManagerModel,
  AccountManagementDTO,
  AccountManagementModel,
  AppListDTO,
  InfoUserDTO,
  PostImgModel,
} from '../Model/account-management.model';
import { DepartmentModel } from '../../DepartmentManagement/Model/department-management.model';
import { environment } from 'src/environments/environment';
import { QueryParamsModelNew } from '../../../_core/models/query-models/query-params.model';
import { HttpUtilsService } from '../../../_core/utils/http-utils.service';
import { ResultModel, ResultObjModel } from '../../../_core/models/_base.model';

const API_PRODUCTS_URL = environment.ApiRoot + '/accountmanagement';

@Injectable()
export class AccountManagementService {
  lastFilter$: BehaviorSubject<QueryParamsModelNew> = new BehaviorSubject(new QueryParamsModelNew({}, 'asc', '', 0, 50));

  constructor(private http: HttpClient, private httpUtils: HttpUtilsService) {}

  findData(queryParams: QueryParamsModelNew): Observable<any> {
    const httpHeaders = this.httpUtils.getHTTPHeaders();
    const httpParams = this.httpUtils.getFindHTTPParams(queryParams);
    const url = API_PRODUCTS_URL + '/GetListAccountManagement';
    return this.http.get<any>(url, {
      headers: httpHeaders,
      params: httpParams,
    });
  }

  GetListAppByCustomerID(): Observable<ResultModel<AppListDTO>> {
    const httpHeaders = this.httpUtils.getHTTPHeaders();
    const url = API_PRODUCTS_URL + `/GetListAppByCustomerID`;
    return this.http.get<any>(url, {
      headers: httpHeaders,
    });
  }
  createAccount(item: AccountManagementModel): Observable<any> {
    const httpHeaders = this.httpUtils.getHTTPHeaders();
    const url = API_PRODUCTS_URL + '/createAccount';
    return this.http.post<any>(url, item, { headers: httpHeaders });
  }

  UpdateAvatar(img: PostImgModel): Observable<any> {
    const httpHeaders = this.httpUtils.getHTTPHeaders();
    const url = API_PRODUCTS_URL + `/UpdateAvatar`;
    return this.http.post<any>(url, img, { headers: httpHeaders });
  }

  UpdateAvatarWithChangeUrlAvatar(img: PostImgModel): Observable<any> {
    const httpHeaders = this.httpUtils.getHTTPHeaders();
    const url = API_PRODUCTS_URL + `/UpdateAvatarWithChangeUrlAvatar`;
    return this.http.post<any>(url, img, { headers: httpHeaders });
  }

  UpdateDirectManager(acc: AccDirectManagerModel): Observable<any> {
    const httpHeaders = this.httpUtils.getHTTPHeaders();
    const url = API_PRODUCTS_URL + `/UpdateDirectManager`;
    return this.http.post<any>(url, acc, {
      headers: httpHeaders,
    });
  }

  changeTinhTrang(acc: AccChangeTinhTrangModel): Observable<any> {
    const httpHeaders = this.httpUtils.getHTTPHeaders();
    const url = API_PRODUCTS_URL + `/ChangeTinhTrang`;
    return this.http.post<any>(url, acc, {
      headers: httpHeaders,
    });
  }

  GetInfoByUsername(username: string): Observable<ResultObjModel<InfoUserDTO>> {
    const httpHeaders = this.httpUtils.getHTTPHeaders();
    const url = API_PRODUCTS_URL + `/GetInfoByUsername/username=${username}`;
    return this.http.get<any>(url, {
      headers: httpHeaders,
    });
  }
}
