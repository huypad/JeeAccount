import { SortState } from './../../../../../_metronic/shared/crud-table/models/sort.model';
import { ChangeDetectionStrategy, Component, OnDestroy, OnInit } from '@angular/core';
import { FormGroup } from '@angular/forms';
import { BehaviorSubject, Subscription } from 'rxjs';
import { GroupingState, PaginatorState } from 'src/app/_metronic/shared/crud-table';
import { TranslateService } from '@ngx-translate/core';
import { MatDialog } from '@angular/material/dialog';
import { AuthService } from 'src/app/modules/auth';
import { DeleteEntityDialogComponent } from '../../../_shared/delete-entity-dialog/delete-entity-dialog.component';
import { ChangeTinhTrangEditDialogComponent } from '../../AccountManagement/change-tinh-trang-edit-dialog/change-tinh-trang-edit-dialog.component';
import { AccountManagementService } from '../../AccountManagement/Services/account-management.service';
import { LayoutUtilsService, MessageType } from '../../../_core/utils/layout-utils.service';
import { DanhMucChungService } from '../../../_core/services/danhmuc.service';
import { PermissionAdminHeThongService } from '../services/permision-admin-hethong.service';
import { PermissionManagementAdminHeThongEditDialogComponent } from '../permission-management-admin-he-thong-edit/permission-management-admin-he-thong-edit.component';
import { showSearchFormModel } from '../../../_shared/jee-search-form/jee-search-form.model';

@Component({
  selector: 'app-permission-management-admin-he-thong-list',
  templateUrl: './permission-management-admin-he-thong-list.component.html',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class PermissionManagementAdminHeThongComponent implements OnInit, OnDestroy {
  loadingSubject = new BehaviorSubject<boolean>(false);
  filterGroup: FormGroup;
  searchGroup: FormGroup;
  paginator: PaginatorState;
  sorting: SortState;
  grouping: GroupingState;
  imgFile: string = '';
  isLoading: boolean = false;
  isJeeHR: boolean;
  isAdminHeThong: boolean;
  showSearch = new showSearchFormModel();

  private subscriptions: Subscription[] = [];

  constructor(
    public service: PermissionAdminHeThongService,
    public accountManagementService: AccountManagementService,
    private translate: TranslateService,
    private layoutUtilsService: LayoutUtilsService,
    public dialog: MatDialog,
    public danhmuc: DanhMucChungService,
    private auth: AuthService
  ) {}
  //=================PageSize Table=====================
  pageSize: number = 50;

  ngOnInit() {
    this.grouping = this.service.grouping;
    this.paginator = this.service.paginator;
    this.sorting = this.service.sorting;
    const sb = this.service.isLoading$.subscribe((res) => (this.isLoading = res));
    this.subscriptions.push(sb);
    const userid = +this.auth.getAuthFromLocalStorage()['user']['customData']['jee-account']['userID'];
    this.danhmuc.isAdminHeThong(userid).subscribe((res) => {
      this.isAdminHeThong = res.IsAdmin;
    });
    this.configShowSearch();
  }
  configShowSearch() {
    this.showSearch.dakhoa = true;
    this.showSearch.isAdmin = false;
  }
  create() {
    let saveMessageTranslateParam = '';
    saveMessageTranslateParam += 'Thêm thành công';
    const saveMessage = this.translate.instant(saveMessageTranslateParam);
    const messageType = MessageType.Create;
    const dialogRef = this.dialog.open(PermissionManagementAdminHeThongEditDialogComponent, {
      data: {},
    });
    const sb = dialogRef.afterClosed().subscribe((res) => {
      if (!res) {
        this.service.fetch();
      } else {
        this.layoutUtilsService.showActionNotification(saveMessage, messageType, 4000, true, false);
        this.service.fetch();
      }
    });
    this.subscriptions.push(sb);
  }

  delete(UserID: number) {
    let saveMessageTranslateParam = 'COMMOM.XOATHANHCONG';
    const saveMessage = this.translate.instant(saveMessageTranslateParam);
    const messageType = MessageType.Create;
    const dialogRef = this.dialog.open(DeleteEntityDialogComponent, {
      data: {},
      width: '450px',
    });
    const sb = dialogRef.afterClosed().subscribe((res) => {
      if (!res) {
        this.service.fetch();
      } else {
        this.service.RemoveAdminHeThong(UserID).subscribe(
          (res) => {
            this.layoutUtilsService.showActionNotification(saveMessage, messageType, 4000, true, false);
          },
          (error) => {
            this.layoutUtilsService.showActionNotification(error.error.message, MessageType.Read, 999999999, true, false, 3000, 'top', 0);
          },
          () => {
            this.service.fetch();
          }
        );
      }
    });
    this.subscriptions.push(sb);
  }

  ngOnDestroy(): void {
    this.subscriptions.forEach((sb) => sb.unsubscribe());
    this.service.ngOnDestroy();
    this.accountManagementService.ngOnDestroy();
  }

  getHeight(): any {
    let tmp_height = 0;
    tmp_height = window.innerHeight - 236;
    return tmp_height + 'px';
  }

  changeKeyword(val) {
    this.search(val);
  }

  changeFilter(filter) {
    this.service.patchState({ filter });
  }

  search(searchTerm: string) {
    this.service.patchState({ searchTerm });
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
    this.service.patchState({ sorting });
  }

  paginate(paginator: PaginatorState) {
    this.service.patchState({ paginator });
  }

  changeTinhTrang(Username: string) {
    let saveMessageTranslateParam = '';
    saveMessageTranslateParam += 'Thay đổi tình trạng thành công';
    const saveMessage = this.translate.instant(saveMessageTranslateParam);
    const messageType = MessageType.Create;
    const dialogRef = this.dialog.open(ChangeTinhTrangEditDialogComponent, {
      data: { Username: Username },
    });
    const sb = dialogRef.afterClosed().subscribe((res) => {
      if (!res) {
        this.service.fetch();
      } else {
        this.layoutUtilsService.showActionNotification(saveMessage, messageType, 4000, true, false);
        this.service.fetch();
      }
    });
    this.subscriptions.push(sb);
  }
}
 