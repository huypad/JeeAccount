import { environment } from 'src/environments/environment';
import { HttpClient } from '@angular/common/http';
import { Observable, Subscription } from 'rxjs';
import { Inject, Injectable, OnDestroy } from '@angular/core';
import { HttpUtilsService } from '../../_core/utils/http-utils.service';
import { ResultObjectModel, ResultObjModel } from '../../_core/models/_base.model';
import { DepartmentManagement } from '../../Management/DepartmentManagement/Model/department-management.model';

const API_PRODUCTS_URL = environment.HOST_JEEACCOUNT_API + '/api/';

@Injectable()
export class JeeSearchFormService implements OnDestroy {
  private _subscriptions: Subscription[] = [];
  constructor(private http: HttpClient, private httpUtils: HttpUtilsService) {}

  ngOnDestroy(): void {
    this._subscriptions.forEach((sb) => sb.unsubscribe());
  }

  getDSPhongBan(): Observable<ResultObjectModel<DepartmentManagement>> {
    const httpHeaders = this.httpUtils.getHTTPHeaders();
    const url = API_PRODUCTS_URL + `accountdepartmentmanagement/GetListDepartmentManagement?query.more=true`;
    return this.http.get<any>(url, { headers: httpHeaders });
  }
}
