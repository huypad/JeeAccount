import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable, forkJoin, BehaviorSubject, of } from 'rxjs';
import { mergeMap } from 'rxjs/operators';
import { HttpUtilsService } from '../utils/http-utils.service';
import { QueryParamsModel } from '../models/query-models/query-params.model';
import { QueryResultsModel } from '../models/query-models/query-results.model';
import { environment } from '../../../../../environments/environment';

const API_URL = environment.ApiRoot;
const API_URL_Landingpage = environment.ApiRootsLanding;
const API_PRODUCTS_URL = environment.ApiRoot + '/dashboard';
const API_URL_GENERAL = environment.ApiRoot + '/general';
@Injectable()
export class DanhMucChungService {
  lastFilter$: BehaviorSubject<QueryParamsModel> = new BehaviorSubject(new QueryParamsModel({}, 'asc', '', 0, 10));

  constructor(private http: HttpClient, private httpUtils: HttpUtilsService) {}
  
  GetListWidgets(module: string): Observable<QueryResultsModel> {
    const httpHeaders = this.httpUtils.getHTTPHeaders();
    return this.http.get<QueryResultsModel>(environment.ApiRootsLanding + `/api/widgets/get-list-widgets?module=${module}`, {
      headers: httpHeaders,
    });
  }

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
}
