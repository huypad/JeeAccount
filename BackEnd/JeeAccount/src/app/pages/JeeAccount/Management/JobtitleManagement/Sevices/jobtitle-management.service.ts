import { QueryParamsModelNew } from '../../../_core/models/query-models/query-params.model';
import { HttpClient } from '@angular/common/http';
import { BehaviorSubject, Observable } from 'rxjs';
import { Inject, Injectable } from '@angular/core';
import { JobtitleModel, JobChangeTinhTrangModel, JobtitleManagementDTO } from '../Model/jobtitle-management.model';
import { environment } from 'src/environments/environment';
import { HttpUtilsService } from '../../../_core/utils/http-utils.service';
import { ITableService } from '../../../_core/services/itable.service';

const API_PRODUCTS_URL = environment.HOST_JEEACCOUNT_API + '/api/jobtitlemanagement';

@Injectable()
export class JobtitleManagementService extends ITableService<JobtitleManagementDTO[]> {
  constructor(@Inject(HttpClient) http, @Inject(HttpUtilsService) httpUtils) {
    super(http, httpUtils);
  }
  API_URL_FIND: string = API_PRODUCTS_URL + '/GetListJobtitleManagement';
  API_URL_CTEATE: string = API_PRODUCTS_URL + '/CreateJobtitle';
  API_URL_EDIT: string = API_PRODUCTS_URL + '/UpdateJobtitle';
  API_URL_DELETE: string = API_PRODUCTS_URL + '/Delete';
  API_URL: string = API_PRODUCTS_URL;

  createDepart(item: JobtitleModel): Observable<any> {
    const httpHeaders = this.httpUtils.getHTTPHeaders();
    const url = API_PRODUCTS_URL + '/CreateJobtitle';
    return this.http.post<any>(url, item, { headers: httpHeaders });
  }

  UpdateDepart(item: JobtitleModel): Observable<any> {
    const httpHeaders = this.httpUtils.getHTTPHeaders();
    const url = API_PRODUCTS_URL + '/UpdateJobtitle';
    return this.http.post<any>(url, item, { headers: httpHeaders });
  }

  changeTinhTrang(acc: JobChangeTinhTrangModel): Observable<any> {
    const httpHeaders = this.httpUtils.getHTTPHeaders();
    const url = API_PRODUCTS_URL + `/ChangeTinhTrang`;
    return this.http.post<any>(url, acc, {
      headers: httpHeaders,
    });
  }
  ngOnDestroy(): void {
    this.subscriptions.forEach((sb) => sb.unsubscribe());
  }

  GetJobtile(rowid: number): Observable<JobtitleModel> {
    const httpHeaders = this.httpUtils.getHTTPHeaders();
    const url = API_PRODUCTS_URL + `/GetJobtitle/${rowid}`;
    return this.http.get<any>(url, { headers: httpHeaders });
  }

  Delete(rowid: number): Observable<any> {
    const httpHeaders = this.httpUtils.getHTTPHeaders();
    const url = API_PRODUCTS_URL + `/Delete/${rowid}`;
    return this.http.delete<any>(url, {
      headers: httpHeaders,
    });
  }
}
