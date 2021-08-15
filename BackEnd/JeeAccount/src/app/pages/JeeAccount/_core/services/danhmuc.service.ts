import { IsAdmin } from './../models/danhmuc.model';
import { JobtitleManagementDTO } from './../../Management/JobtitleManagement/Model/jobtitle-management.model';
import { DepartmentManagement } from './../../Management/DepartmentManagement/Model/department-management.model';
import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable, BehaviorSubject, of } from 'rxjs';
import { HttpUtilsService } from '../utils/http-utils.service';
import { QueryParamsModel } from '../models/query-models/query-params.model';
import { environment } from '../../../../../environments/environment';
import { ResultObjectModel } from '../models/_base.model';
import { DatePipe } from '@angular/common';
import { CommonInfo } from '../models/danhmuc.model';

const API_URL = environment.HOST_JEEACCOUNT_API + '/api';
const API_PRODUCTS_URL = API_URL + '/dashboard';
const API_URL_GENERAL = API_URL + '/general';
@Injectable()
export class DanhMucChungService {
  lastFilter$: BehaviorSubject<QueryParamsModel> = new BehaviorSubject(new QueryParamsModel({}, 'asc', '', 0, 10));

  constructor(private http: HttpClient, private httpUtils: HttpUtilsService, public datepipe: DatePipe) {}

  //=================Dùng trong trang truy cập nhanh============================================
  Insert_TruyCapNhanh(item: any): Observable<any> {
    const httpHeaders = this.httpUtils.getHTTPHeaders();
    return this.http.post(API_PRODUCTS_URL + '/Insert_TruyCapNhanh', item, { headers: httpHeaders });
  }
  Check_TruyCapNhanh(id: number, type: string = 'sub'): Observable<any> {
    const httpHeaders = this.httpUtils.getHTTPHeaders();
    const url = `${API_PRODUCTS_URL}/Check_TruyCapNhanh?id=${id}&type=${type}`;
    return this.http.get<any>(url, { headers: httpHeaders });
  }
  Delete_TruyCapNhanh(id: string, type: string = 'sub'): Observable<any> {
    const httpHeaders = this.httpUtils.getHTTPHeaders();
    const url = `${API_PRODUCTS_URL}/Delete_TruyCapNhanh?id=${id}&type=${type}`;
    return this.http.get<any>(url, { headers: httpHeaders });
  }
  // =================END============================================

  // =================ALL============================================
  GetMatchipNhanVien(): Observable<any> {
    const httpHeaders = this.httpUtils.getHTTPHeaders();
    const url = `${API_URL_GENERAL}/GetMatchipNhanVien`;
    return this.http.get<any>(url, { headers: httpHeaders });
  }

  GetSelectionDepartment(): Observable<any> {
    const httpHeaders = this.httpUtils.getHTTPHeaders();
    const url = `${API_URL_GENERAL}/GetSelectionDepartMent`;
    return this.http.get<any>(url, { headers: httpHeaders });
  }
  // =================END============================================

  getDSPhongBan(): Observable<ResultObjectModel<DepartmentManagement>> {
    const httpHeaders = this.httpUtils.getHTTPHeaders();
    const url = API_URL + `/accountdepartmentmanagement/GetListDepartmentManagement?query.more=true`;
    return this.http.get<any>(url, { headers: httpHeaders });
  }

  getDSChucvu(): Observable<ResultObjectModel<JobtitleManagementDTO[]>> {
    const httpHeaders = this.httpUtils.getHTTPHeaders();
    const url = API_URL + `/jobtitlemanagement/GetListJobtitleManagement?query.more=true`;
    return this.http.get<any>(url, { headers: httpHeaders });
  }

  getCompanyCode(): Observable<any> {
    const httpHeaders = this.httpUtils.getHTTPHeaders();
    const url = API_URL + `/customermanagement/GetCompanyCode`;
    return this.http.get<any>(url, { headers: httpHeaders });
  }

  isAdminHeThong(userid: number): Observable<IsAdmin> {
    const httpHeaders = this.httpUtils.getHTTPHeaders();
    const url = API_URL + `/accountmanagement/CheckAdminHeThong/${userid}`;
    return this.http.get<any>(url, { headers: httpHeaders });
  }
  isAdminApp(userid: number, appid: number): Observable<IsAdmin> {
    const httpHeaders = this.httpUtils.getHTTPHeaders();
    const url = API_URL + `/accountmanagement/CheckAdminApp/${userid}/${appid}`;
    return this.http.get<any>(url, { headers: httpHeaders });
  }
  sortObject(obj) {
    return Object.keys(obj)
      .sort()
      .reduce(function (result, key) {
        result[key] = obj[key];
        return result;
      }, {});
  }

  isEqual(object, otherObject) {
    return Object.entries(this.sortObject(object)).toString() === Object.entries(this.sortObject(otherObject)).toString();
  }

  f_number(value: any) {
    return Number((value + '').replace(/,/g, ''));
  }

  f_currency(value: any, args?: any): any {
    let nbr = Number((value + '').replace(/,|-/g, ''));
    return (nbr + '').replace(/(\d)(?=(\d{3})+(?!\d))/g, '$1,');
  }

  format_date(value: any, args?: any): any {
    let latest_date = this.datepipe.transform(value, 'dd/MM/yyyy');
    return latest_date;
  }

  f_string_date(value: string): Date {
    return new Date(value.split('/')[2] + '-' + value.split('/')[1] + '-' + value.split('/')[0]);
  }

  GetCommonAccount(userid: number): Observable<CommonInfo> {
    const httpHeaders = this.httpUtils.getHTTPHeaders();
    const url = API_URL + `/accountmanagement/GetCommonAccount/${userid}`;
    return this.http.get<any>(url, {
      headers: httpHeaders,
    });
  }
}
