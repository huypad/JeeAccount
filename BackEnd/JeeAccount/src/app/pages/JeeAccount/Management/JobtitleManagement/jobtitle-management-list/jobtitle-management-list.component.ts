import { showSearchFormModel } from './../../../_shared/jee-search-form/jee-search-form.model';
import { environment } from 'src/environments/environment';
import { GroupingState } from '../../../../../_metronic/shared/crud-table/models/grouping.model';
import { FormGroup } from '@angular/forms';
import { ChangeDetectionStrategy, ChangeDetectorRef, Component, OnInit, ViewChild } from '@angular/core';
import { MatDialog } from '@angular/material/dialog';
import { TranslateService } from '@ngx-translate/core';
import { BehaviorSubject, merge, Subscription } from 'rxjs';
import { SubheaderService } from 'src/app/_metronic/partials/layout';
import { LayoutUtilsService, MessageType } from '../../../_core/utils/layout-utils.service';
import { PaginatorState, SortState } from 'src/app/_metronic/shared/crud-table';
import { JobtitleModel } from '../Model/jobtitle-management.model';
import { DeleteEntityDialogComponent } from '../../../_shared/delete-entity-dialog/delete-entity-dialog.component';
import { AuthService } from 'src/app/modules/auth';
import { JobtitleManagementService } from '../Sevices/jobtitle-management.service';
import { JobtitleManagementEditDialogComponent } from '../jobtitle-management-edit-dialog/jobtitle-management-edit-dialog.component';
import { ChangeTinhTrangJobtitleEditDialogComponent } from '../change-tinh-trang-jobtitle-edit-dialog/change-tinh-trang-jobtitile-edit-dialog.component';

@Component({
  selector: 'app-jobtitle-management-list',
  templateUrl: './jobtitle-management-list.component.html',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class JobtitleManagementListComponent implements OnInit {
  constructor(
    public jobtitleManagementService: JobtitleManagementService,
    private translate: TranslateService,
    public subheaderService: SubheaderService,
    private layoutUtilsService: LayoutUtilsService,
    public dialog: MatDialog,
    public auth: AuthService,
    public cd: ChangeDetectorRef
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
  isJeeHR: boolean = false;
  data: any;
  APPCODE_JEEHR = environment.APPCODE_JEEHR;
  private subscriptions: Subscription[] = [];
  showSearch = new showSearchFormModel();
  ngOnInit() {
    this.jobtitleManagementService.fetch();
    this.grouping = this.jobtitleManagementService.grouping;
    this.paginator = this.jobtitleManagementService.paginator;
    this.sorting = this.jobtitleManagementService.sorting;
    const sb = this.jobtitleManagementService.isLoading$.subscribe((res) => (this.isLoading = res));
    this.subscriptions.push(sb);
    this.checkIsJeeHR();
    this.configShowSearch();
  }
  configShowSearch() {
    this.showSearch.phongban = false;
    this.showSearch.phongbanid = false;
    this.showSearch.dakhoa = true;
    this.showSearch.isAdmin = false;
    this.showSearch.tennhanvien = false;
    this.showSearch.username = false;
    this.showSearch.titlekeyword = 'SEARCH.SEARCH3';
  }

  checkIsJeeHR() {
    this.data = this.auth.getAuthFromLocalStorage();
    const lstAppCode: string[] = this.data['user']['customData']['jee-account']['appCode'];
    if (lstAppCode) {
      if (lstAppCode.indexOf(this.APPCODE_JEEHR) != -1) {
        this.isJeeHR = true;
        this.cd.detectChanges();
      } else {
        this.isJeeHR = false;
      }
    }
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
    this.jobtitleManagementService.patchState({ sorting });
  }

  paginate(paginator: PaginatorState) {
    this.jobtitleManagementService.patchState({ paginator });
  }

  changeKeyword(val) {
    this.search(val);
  }

  changeFilter(filter) {
    this.jobtitleManagementService.patchState({ filter });
  }

  search(searchTerm: string) {
    this.jobtitleManagementService.patchState({ searchTerm });
  }

  create() {
    const item = new JobtitleModel();
    item.clear(); // Set all defaults fields
    this.update(item);
  }

  update(item) {
    let saveMessageTranslateParam = '';
    saveMessageTranslateParam += item.RowID > 0 ? 'C???p nh???t th??nh c??ng' : 'Th??m th??nh c??ng';
    const saveMessage = this.translate.instant(saveMessageTranslateParam);
    const messageType = item.RowID > 0 ? MessageType.Update : MessageType.Create;
    const dialogRef = this.dialog.open(JobtitleManagementEditDialogComponent, { data: { item } });
    dialogRef.afterClosed().subscribe((res) => {
      if (!res) {
        this.jobtitleManagementService.fetch();
      } else {
        this.layoutUtilsService.showActionNotification(saveMessage, messageType, 4000, true, false);
        this.jobtitleManagementService.fetch();
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
    saveMessageTranslateParam += 'Thay ?????i t??nh tr???ng th??nh c??ng';
    const saveMessage = this.translate.instant(saveMessageTranslateParam);
    const messageType = MessageType.Create;
    const dialogRef = this.dialog.open(ChangeTinhTrangJobtitleEditDialogComponent, {
      data: { RowID: RowID },
    });
    dialogRef.afterClosed().subscribe((res) => {
      if (!res) {
        this.jobtitleManagementService.fetch();
      } else {
        this.layoutUtilsService.showActionNotification(saveMessage, messageType, 4000, true, false);
        this.jobtitleManagementService.fetch();
      }
    });
  }

  delete(RowID: number) {
    let saveMessageTranslateParam = 'COMMOM.XOATHANHCONG';
    const saveMessage = this.translate.instant(saveMessageTranslateParam);
    const messageType = MessageType.Create;
    const dialogRef = this.dialog.open(DeleteEntityDialogComponent, {
      data: {},
      width: '450px',
    });
    const sb = dialogRef.afterClosed().subscribe((res) => {
      if (!res) {
        this.jobtitleManagementService.fetch();
      } else {
        this.jobtitleManagementService.Delete(RowID).subscribe(
          (res) => {
            this.layoutUtilsService.showActionNotification(saveMessage, messageType, 4000, true, false);
          },
          (error) => {
            this.layoutUtilsService.showActionNotification(error.error.message, MessageType.Read, 999999999, true, false, 3000, 'top', 0);
          },
          () => {
            this.jobtitleManagementService.fetch();
          }
        );
      }
    });
    this.subscriptions.push(sb);
  }
}
