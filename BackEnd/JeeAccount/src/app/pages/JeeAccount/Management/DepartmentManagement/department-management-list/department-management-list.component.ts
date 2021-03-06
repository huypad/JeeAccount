import { environment } from 'src/environments/environment';
import { ChangeTinhTrangDepartmentEditDialogComponent } from './../change-tinh-trang-department-edit-dialog/change-tinh-trang-department-edit-dialog.component';
import { GroupingState } from './../../../../../_metronic/shared/crud-table/models/grouping.model';
import { FormGroup } from '@angular/forms';
import { ChangeDetectionStrategy, Component, OnInit, ViewChild, OnDestroy, ChangeDetectorRef } from '@angular/core';
import { MatDialog } from '@angular/material/dialog';
import { TranslateService } from '@ngx-translate/core';
import { BehaviorSubject, merge, Subscription } from 'rxjs';
import { SubheaderService } from 'src/app/_metronic/partials/layout';
import { DepartmentManagementService } from '../Sevices/department-management.service';
import { DepartmentManagementEditDialogComponent } from '../department-management-edit-dialog/department-management-edit-dialog.component';
import { DepartmentManagementDTO, DepartmentModel, TreeJeeHRDepartmentDTO } from '../Model/department-management.model';
import { LayoutUtilsService, MessageType } from '../../../_core/utils/layout-utils.service';
import { PaginatorState, SortState } from 'src/app/_metronic/shared/crud-table';
import { DepartmentQuanLytrucTiepEditDialogComponent } from '../department-quan-ly-truc-tiep-edit-dialog/department-quan-ly-truc-tiep-edit-dialog.component';
import { showSearchFormModel } from '../../../_shared/jee-search-form/jee-search-form.model';
import { DeleteEntityDialogComponent } from '../../../_shared/delete-entity-dialog/delete-entity-dialog.component';
import { AuthService } from 'src/app/modules/auth';
import { ArrayDataSource } from '@angular/cdk/collections';
import { NestedTreeControl } from '@angular/cdk/tree';

@Component({
  selector: 'app-department-management-list',
  templateUrl: './department-management-list.component.html',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class DepartmentManagementListComponent implements OnInit, OnDestroy {
  constructor(
    public departmentManagementService: DepartmentManagementService,
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
  showSearch = new showSearchFormModel();
  private subscriptions: Subscription[] = [];
  datatree$: BehaviorSubject<any[]> = new BehaviorSubject<any>([]);
  isTree: boolean = false;

  treeControl = new NestedTreeControl<TreeJeeHRDepartmentDTO>((node) => node.Children);
  dataSource: any;

  hasChild = (_: number, node: TreeJeeHRDepartmentDTO) => !!node.Children && node.Children.length > 0;
  ngOnInit() {
    this.departmentManagementService.fetch();
    this.grouping = this.departmentManagementService.grouping;
    this.paginator = this.departmentManagementService.paginator;
    this.sorting = this.departmentManagementService.sorting;
    const sb = this.departmentManagementService.isLoading$.subscribe((res) => (this.isLoading = res));
    this.subscriptions.push(sb);
    this.configShowSearch();
    this.checkIsJeeHR();
    this.departmentManagementService.items$.subscribe((res) => {
      if (res) {
        if (res.isTree) this.isTree = res.isTree;
        this.dataSource = new ArrayDataSource(res.tree);
        this.cd.detectChanges();
      }
    });
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
  configShowSearch() {
    this.showSearch.chucvu = false;
    this.showSearch.chucvuid = false;
    this.showSearch.dakhoa = true;
    this.showSearch.isAdmin = false;
    this.showSearch.tennhanvien = false;
    this.showSearch.username = false;
    this.showSearch.titlekeyword = 'SEARCH.SEARCH2';
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
    let saveMessageTranslateParam = '';
    saveMessageTranslateParam += item.RowID > 0 ? 'C???p nh???t th??nh c??ng' : 'Th??m th??nh c??ng';
    const saveMessage = this.translate.instant(saveMessageTranslateParam);
    const messageType = item.RowID > 0 ? MessageType.Update : MessageType.Create;
    const dialogRef = this.dialog.open(DepartmentManagementEditDialogComponent, { data: { item } });
    const sb = dialogRef.afterClosed().subscribe((res) => {
      if (!res) {
        this.departmentManagementService.fetch();
      } else {
        this.layoutUtilsService.showActionNotification(saveMessage, messageType, 4000, true, false);
        this.departmentManagementService.fetch();
      }
    });
    this.subscriptions.push(sb);
  }

  update(item: DepartmentManagementDTO) {
    let saveMessageTranslateParam = '';
    saveMessageTranslateParam += item.RowID > 0 ? 'C???p nh???t th??nh c??ng' : 'Th??m th??nh c??ng';
    const saveMessage = this.translate.instant(saveMessageTranslateParam);
    const messageType = item.RowID > 0 ? MessageType.Update : MessageType.Create;
    const dialogRef = this.dialog.open(DepartmentManagementEditDialogComponent, { data: { item } });
    const sb = dialogRef.afterClosed().subscribe((res) => {
      if (!res) {
        this.departmentManagementService.fetch();
      } else {
        this.layoutUtilsService.showActionNotification(saveMessage, messageType, 4000, true, false);
        this.departmentManagementService.fetch();
      }
    });
    this.subscriptions.push(sb);
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
    const dialogRef = this.dialog.open(ChangeTinhTrangDepartmentEditDialogComponent, {
      data: { RowID: RowID },
    });
    const sb = dialogRef.afterClosed().subscribe((res) => {
      if (!res) {
        this.departmentManagementService.fetch();
      } else {
        this.layoutUtilsService.showActionNotification(saveMessage, messageType, 4000, true, false);
        this.departmentManagementService.fetch();
      }
    });
    this.subscriptions.push(sb);
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
        this.departmentManagementService.fetch();
      } else {
        this.departmentManagementService.Delete(RowID).subscribe(
          (res) => {
            this.layoutUtilsService.showActionNotification(saveMessage, messageType, 4000, true, false);
          },
          (error) => {
            this.layoutUtilsService.showActionNotification(error.error.message, MessageType.Read, 999999999, true, false, 3000, 'top', 0);
          },
          () => {
            this.departmentManagementService.fetch();
          }
        );
      }
    });
    this.subscriptions.push(sb);
  }

  openQuanLyTrucTiepEdit(RowID: number, DepartmentManager: string) {
    let saveMessageTranslateParam = '';
    DepartmentManager === '' ? (saveMessageTranslateParam += 'Th??m th??nh c??ng') : (saveMessageTranslateParam += 'C???p nh???t th??nh c??ng');
    const saveMessage = this.translate.instant(saveMessageTranslateParam);

    const dialogRef = this.dialog.open(DepartmentQuanLytrucTiepEditDialogComponent, {
      data: { RowID, DepartmentManager },
    });
    const sb = dialogRef.afterClosed().subscribe((res) => {
      if (!res) {
        this.departmentManagementService.fetch();
      } else {
        this.layoutUtilsService.showActionNotification(saveMessage, MessageType.Create, 4000, true, false);
        this.departmentManagementService.fetch();
      }
    });
    this.subscriptions.push(sb);
  }

  ngOnDestroy(): void {
    this.departmentManagementService.ngOnDestroy();
    this.subscriptions.forEach((sb) => sb.unsubscribe());
  }
}
