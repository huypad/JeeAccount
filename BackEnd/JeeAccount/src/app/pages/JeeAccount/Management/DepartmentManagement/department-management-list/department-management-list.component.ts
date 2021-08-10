import { GroupingState } from './../../../../../_metronic/shared/crud-table/models/grouping.model';
import { FormGroup } from '@angular/forms';
import { SelectionModel } from '@angular/cdk/collections';
import { ChangeDetectionStrategy, ChangeDetectorRef, Component, OnInit, ViewChild } from '@angular/core';
import { MatDialog, MatDialogRef } from '@angular/material/dialog';
import { MatPaginator } from '@angular/material/paginator';
import { MatSort } from '@angular/material/sort';
import { TranslateService } from '@ngx-translate/core';
import { BehaviorSubject, merge, Subscription } from 'rxjs';
import { tap } from 'rxjs/operators';
import { TokenStorage } from 'src/app/modules/auth/_services/token-storage.service';
import { SubheaderService } from 'src/app/_metronic/partials/layout';
import { DepartmentManagementService } from '../Sevices/department-management.service';
import { DepartmentManagementEditDialogComponent } from '../department-management-edit-dialog/department-management-edit-dialog.component';
import { DepartmentModel } from '../Model/department-management.model';
import { DepartmentChangeTinhTrangEditDialogComponent } from '../department-change-tinh-trang-edit-dialog/department-change-tinh-trang-edit-dialog.component';
import { LayoutUtilsService, MessageType } from '../../../_core/utils/layout-utils.service';
import { QueryParamsModelNew } from '../../../_core/models/query-models/query-params.model';
import { PaginatorState, SortState } from 'src/app/_metronic/shared/crud-table';

@Component({
  selector: 'app-department-management-list',
  templateUrl: './department-management-list.component.html',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class DepartmentManagementListComponent implements OnInit {
  constructor(
    public departmentManagementService: DepartmentManagementService,
    private translate: TranslateService,
    public subheaderService: SubheaderService,
    private layoutUtilsService: LayoutUtilsService,
    public dialog: MatDialog
  ) {}
  //=================PageSize Table=====================
  pageSize: number = 50;
  loadingSubject = new BehaviorSubject<boolean>(false);
  filterGroup: FormGroup;
  searchGroup: FormGroup;
  paginator: PaginatorState;
  sorting: SortState;
  grouping: GroupingState;
  imgFile: string = '';
  isLoading: boolean = false;
  isJeeHR: boolean;
  private subscriptions: Subscription[] = [];

  ngOnInit() {
    this.departmentManagementService.fetch();
    this.grouping = this.departmentManagementService.grouping;
    this.paginator = this.departmentManagementService.paginator;
    this.sorting = this.departmentManagementService.sorting;
    const sb = this.departmentManagementService.isLoading$.subscribe((res) => (this.isLoading = res));
    this.subscriptions.push(sb);
  }

  sort(column: string): void {
    const sorting = this.sorting;
    const isActiveColumn = sorting.column === column;
    if (!isActiveColumn) {
      sorting.column = column;
      sorting.direction = 'asc';
    } else {
      sorting.direction = sorting.direction === 'asc' ? 'desc' : 'asc';
    }
    this.departmentManagementService.patchState({ sorting });
  }

  paginate(paginator: PaginatorState) {
    this.departmentManagementService.patchState({ paginator });
  }

  changeKeyword(val) {
    this.search(val);
  }

  changeFilter(filter) {
    this.departmentManagementService.patchState({ filter });
  }

  search(searchTerm: string) {
    this.departmentManagementService.patchState({ searchTerm });
  }

  create() {
    const item = new DepartmentModel();
    item.clear(); // Set all defaults fields
    this.update(item);
  }

  update(item: DepartmentModel) {
    let saveMessageTranslateParam = '';
    saveMessageTranslateParam += item.RowID > 0 ? 'Cập nhật thành công' : 'Thêm thành công';
    const saveMessage = this.translate.instant(saveMessageTranslateParam);
    const messageType = item.RowID > 0 ? MessageType.Update : MessageType.Create;
    const dialogRef = this.dialog.open(DepartmentManagementEditDialogComponent, { data: { item } });
    dialogRef.afterClosed().subscribe((res) => {
      if (!res) {
        this.departmentManagementService.fetch();
      } else {
        this.layoutUtilsService.showActionNotification(saveMessage, messageType, 4000, true, false);
        this.departmentManagementService.fetch();
      }
    });
  }

  getHeight(): any {
    let tmp_height = 0;
    tmp_height = window.innerHeight - 236;
    return tmp_height + 'px';
  }

  changeTinhTrang(RowID: number) {
    let saveMessageTranslateParam = '';
    saveMessageTranslateParam += 'Thay đổi tình trạng thành công';
    const saveMessage = this.translate.instant(saveMessageTranslateParam);
    const messageType = MessageType.Create;
    const dialogRef = this.dialog.open(DepartmentChangeTinhTrangEditDialogComponent, {
      data: { RowID: RowID },
    });
    dialogRef.afterClosed().subscribe((res) => {
      if (!res) {
        this.departmentManagementService.fetch();
      } else {
        this.layoutUtilsService.showActionNotification(saveMessage, messageType, 4000, true, false);
        this.departmentManagementService.fetch();
      }
    });
  }

  delete(item) {}

  openQuanLyTrucTiepEdit(username, DirectManager) {}
}
