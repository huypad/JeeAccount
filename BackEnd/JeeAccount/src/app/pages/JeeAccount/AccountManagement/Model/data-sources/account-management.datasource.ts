import { of } from 'rxjs';
import { catchError, finalize, tap } from 'rxjs/operators';
import { BaseDataSource } from '../../../_core/models/data-sources/_base.datasource';
import { QueryParamsModelNew } from '../../../_core/models/query-models/query-params.model';
import { QueryResultsModel } from '../../../_core/models/query-models/query-results.model';
import { AccountManagementService } from '../../Services/account-management.service';

export class AccountManagementDataSource extends BaseDataSource {
  constructor(private accountManagementServe: AccountManagementService) {
    super();
  }

  LoadList(queryParams: QueryParamsModelNew) {
    this.accountManagementServe.lastFilter$.next(queryParams);
    this.loadingSubject.next(true);
    this.accountManagementServe
      .findData(queryParams)
      .pipe(
        tap((resultFromServer) => {
          this.entitySubject.next(resultFromServer.data);
          var totalCount = resultFromServer.page.TotalCount || resultFromServer.page.AllPage * resultFromServer.page.Size;
          this.paginatorTotalSubject.next(totalCount);
        }),
        catchError((err) => of(new QueryResultsModel([], err))),
        finalize(() => this.loadingSubject.next(false))
      )
      .subscribe((res) => {});
  }
}
