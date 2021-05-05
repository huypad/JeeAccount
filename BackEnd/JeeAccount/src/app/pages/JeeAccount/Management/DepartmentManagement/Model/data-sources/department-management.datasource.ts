import { of } from 'rxjs';
import { catchError, finalize, tap } from 'rxjs/operators';
import { BaseDataSource } from 'src/app/pages/JeeAccount/_core/models/data-sources/_base.datasource';
import { QueryParamsModelNew } from 'src/app/pages/JeeAccount/_core/models/query-models/query-params.model';
import { QueryResultsModel } from 'src/app/pages/JeeAccount/_core/models/query-models/query-results.model';
import { DepartmentManagementService } from '../../Sevices/department-management.service';

export class DepartmentManagementDatasource extends BaseDataSource {
  constructor(private departmentManagementService: DepartmentManagementService) {
    super();
  }
  LoadList(queryParams: QueryParamsModelNew) {
    this.departmentManagementService.lastFilter$.next(queryParams);
    this.loadingSubject.next(true);
    this.departmentManagementService
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
