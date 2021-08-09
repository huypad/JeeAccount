import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { Inject, Injectable, OnDestroy } from '@angular/core';
import {
  AccChangeTinhTrangModel,
  AccDirectManagerModel,
  AccountManagementDTO,
  AccountManagementModel,
  AppListDTO,
  CheckEditAppListByDTO,
  InfoUserDTO,
  JeeHRNhanVien,
  PostImgModel,
} from '../Model/account-management.model';
import { environment } from 'src/environments/environment';
import { HttpUtilsService } from '../../../_core/utils/http-utils.service';
import { ResultModel, ResultObjModel } from '../../../_core/models/_base.model';
import { ITableService } from '../../../_core/services/itable.service';

const API_PRODUCTS_URL = environment.HOST_JEEACCOUNT_API + '/api/accountmanagement';

@Injectable()
export class AccountManagementService extends ITableService<AccountManagementDTO> implements OnDestroy {
  API_URL_FIND: string = API_PRODUCTS_URL + '/GetListAccountManagement';
  API_URL_CTEATE: string = API_PRODUCTS_URL + '/createAccount';
  API_URL_EDIT: string = API_PRODUCTS_URL + '/createAccount';
  API_URL_DELETE: string = API_PRODUCTS_URL + '/Delete';
  API_URL: string = API_PRODUCTS_URL;

  constructor(@Inject(HttpClient) http, @Inject(HttpUtilsService) httpUtils) {
    super(http, httpUtils);
  }

  ngOnDestroy(): void {
    this.subscriptions.forEach((sb) => sb.unsubscribe());
  }

  GetListAppByCustomerID(): Observable<ResultModel<AppListDTO>> {
    const httpHeaders = this.httpUtils.getHTTPHeaders();
    const url = API_PRODUCTS_URL + `/GetListAppByCustomerID`;
    return this.http.get<any>(url, {
      headers: httpHeaders,
    });
  }

  GetListAppByUserId(userid: number): Observable<ResultModel<AppListDTO>> {
    const httpHeaders = this.httpUtils.getHTTPHeaders();
    const url = API_PRODUCTS_URL + `/GetListAppByUserID?userid=${userid}`;
    return this.http.get<any>(url, {
      headers: httpHeaders,
    });
  }

  GetEditListAppByUserIDByListCustomerId(userid: number): Observable<ResultModel<CheckEditAppListByDTO>> {
    const httpHeaders = this.httpUtils.getHTTPHeaders();
    const url = API_PRODUCTS_URL + `/GetEditListAppByUserIDByListCustomerId?userid=${userid}`;
    return this.http.get<any>(url, {
      headers: httpHeaders,
    });
  }

  GetListJeeHR(): Observable<JeeHRNhanVien[]> {
    const httpHeaders = this.httpUtils.getHTTPHeaders();
    const url = API_PRODUCTS_URL + `/GetDSJeeHR`;
    return this.http.get<any>(url, {
      headers: httpHeaders,
    });
  }
  createAccount(item: AccountManagementModel): Observable<any> {
    const httpHeaders = this.httpUtils.getHTTPHeaders();
    const url = API_PRODUCTS_URL + `/createAccount`;
    return this.http.post<any>(url, item, { headers: httpHeaders });
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
