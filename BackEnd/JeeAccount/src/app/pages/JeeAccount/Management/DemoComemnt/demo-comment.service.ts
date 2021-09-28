import { HttpUtilsService } from './../../_core/utils/http-utils.service';
import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../../../../environments/environment';

const API_JEEACCOUNT_URL = environment.HOST_JEEACCOUNT_API;

@Injectable()
export class DemoCommentService {
  constructor(private http: HttpClient, private httpUtils: HttpUtilsService) {}

  public getTopicObjectIDByComponentName(componentName: string): Observable<string> {
    const httpHeaders = this.httpUtils.getHTTPHeaders();
    const url = API_JEEACCOUNT_URL + `/api/comments/getByComponentName/${componentName}`;
    return this.http.get(url, {
      headers: httpHeaders,
      responseType: 'text',
    });
  }

  public getImgUrls(limit: number): Observable<any> {
    const httpHeaders = this.httpUtils.getHTTPHeaders();
    const url = `https://picsum.photos/v2/list?page=2&limit=${limit}`;
    return this.http.get(url, {
      headers: httpHeaders,
    });
  }
}
