import { SortState } from './../../../../../_metronic/shared/crud-table/models/sort.model';
import { ChangeDetectionStrategy, Component, OnDestroy, OnInit } from '@angular/core';
import { FormBuilder, FormGroup } from '@angular/forms';
import { BehaviorSubject, ReplaySubject, Subscription } from 'rxjs';
import { GroupingState, PaginatorState } from 'src/app/_metronic/shared/crud-table';
import { AppListDTO } from '../../AccountManagement/Model/account-management.model';
import { PermissionAdminAppService } from '../services/permision-admin-app.service';
import { TranslateService } from '@ngx-translate/core';
import { LayoutUtilsService, MessageType } from '../../../_core/utils/layout-utils.service';
import { MatDialog } from '@angular/material/dialog';
import { AuthService } from 'src/app/modules/auth';
import { DanhMucChungService } from '../../../_core/services/danhmuc.service';
import { tap } from 'rxjs/operators';
import { ResultModel } from '../../../_core/models/_base.model';
import { AccountManagementService } from '../../AccountManagement/Services/account-management.service';
import { DeleteEntityDialogComponent } from '../../../_shared/delete-entity-dialog/delete-entity-dialog.component';
import { ChangeTinhTrangEditDialogComponent } from '../../AccountManagement/change-tinh-trang-edit-dialog/change-tinh-trang-edit-dialog.component';
import { environment } from 'src/environments/environment';
import { PermissionManagementAdminAppEditDialogComponent } from '../permission-management-admin-app-edit/permission-management-admin-app-edit.component';

@Component({
  selector: 'app-permission-management-admin-app-list',
  templateUrl: './permission-management-admin-app-list.component.html',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class PermissionManagementAdminAppComponent implements OnInit, OnDestroy {
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
  listApp: AppListDTO[] = [];
  filterListApps: ReplaySubject<AppListDTO[]> = new ReplaySubject<AppListDTO[]>();
  itemForm = this.fb.group({
    ListApp: [''],
    ListAppFilterCtrl: [],
  });
  appCode = environment.APPCODE;
  private subscriptions: Subscription[] = [];
  private _isFirstLoading$ = new BehaviorSubject<boolean>(true);
  get isFirstLoading$() {
    return this._isFirstLoading$.asObservable();
  }
  constructor(
    public service: PermissionAdminAppService,
    private accountManagementService: AccountManagementService,
    private translate: TranslateService,
    private layoutUtilsService: LayoutUtilsService,
    public dialog: MatDialog,
    public danhmuc: DanhMucChungService,
    private auth: AuthService,
    private fb: FormBuilder
  ) {}
  //=================PageSize Table=====================
  pageSize: number = 50;

  ngOnInit() {
    const sb1 = this.accountManagementService
      .GetListAppByCustomerID()
      .pipe(
        tap((res: ResultModel<AppListDTO>) => {
          if (res) {
            this.listApp = res.data;
            const index = this.listApp.findIndex((item) => item.AppCode === this.appCode);
            this.listApp.splice(index, 1);
            this.filterListApps.next([...res.data]);
            this.itemForm.controls.ListAppFilterCtrl.valueChanges.subscribe(() => {
              this.profilterListApps();
            });
            this.itemForm.controls.ListApp.setValue(this.listApp[0].AppID);
            const url = this.service.API_URL_FIND;
            this.service.API_URL_FIND = url + '/' + this.listApp[0].AppID;
            this.itemForm.controls.ListApp.valueChanges.subscribe((res) => {
              this.service.API_URL_FIND = url + '/' + res;
              this.service.fetch();
            });
            this._isFirstLoading$.next(false);
            this.grouping = this.service.grouping;
            this.paginator = this.service.paginator;
            this.sorting = this.service.sorting;
            const sb = this.service.isLoading$.subscribe((res) => (this.isLoading = res));
            this.subscriptions.push(sb);
          }
        })
      )
      .subscribe();
    this.subscriptions.push(sb1);
    const userid = +this.auth.getAuthFromLocalStorage()['user']['customData']['jee-account']['userID'];
    this.danhmuc.isAdminHeThong(userid).subscribe((res) => {
      this.isAdminHeThong = res.IsAdmin;
    });
  }

  protected profilterListApps() {
    if (!this.itemForm.controls.ListApp) {
      return;
    }
    let search = this.itemForm.controls.ListAppFilterCtrl.value;
    if (!search) {
      this.filterListApps.next([...this.listApp]);
      return;
    } else {
      search = search.toLowerCase();
    }
    this.filterListApps.next(this.listApp.filter((item) => item.AppName.toLowerCase().indexOf(search) > -1));
  }

  create() {
    const appid = +this.itemForm.controls.ListApp.value;
    let saveMessageTranslateParam = '';
    saveMessageTranslateParam += 'Thêm thành công';
    const saveMessage = this.translate.instant(saveMessageTranslateParam);
    const messageType = MessageType.Create;
    const dialogRef = this.dialog.open(PermissionManagementAdminAppEditDialogComponent, {
      data: { AppID: appid },
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
    const appid = +this.itemForm.controls.ListApp.value;
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
        this.service.RemoveAdminApp(UserID, appid).subscribe(
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
    this.accountManagementService.ngOnDestroy();
    this.service.ngOnDestroy();
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
