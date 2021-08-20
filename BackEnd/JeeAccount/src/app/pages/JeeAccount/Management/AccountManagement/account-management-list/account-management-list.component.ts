import { AccountManagementChinhSuaNoJeeHRDialogComponent } from './../account-management-chinhsua-nojeehr-dialog/account-management-chinhsua-nojeehr-dialog.component';
import { AccountManagementEditJeeHRDialogComponent } from './../account-management-edit-jeehr-dialog/account-management-edit-jeehr-dialog.component';
import { ChangeDetectionStrategy, Component, OnInit, OnDestroy } from '@angular/core';
import { MatDialog } from '@angular/material/dialog';
import { TranslateService } from '@ngx-translate/core';
import { BehaviorSubject, of, Subscription } from 'rxjs';
import { AccountManagementEditDialogComponent } from '../account-management-edit-dialog/account-management-edit-dialog.component';
import { AppListDTO, PostImgModel } from '../Model/account-management.model';
import { AccountManagementService } from '../Services/account-management.service';
import { QuanLytrucTiepEditDialogComponent } from '../quan-ly-truc-tiep-edit-dialog/quan-ly-truc-tiep-edit-dialog.component';
import { ChangeTinhTrangEditDialogComponent } from '../change-tinh-trang-edit-dialog/change-tinh-trang-edit-dialog.component';
import { LayoutUtilsService, MessageType } from '../../../_core/utils/layout-utils.service';
import { DanhMucChungService } from '../../../_core/services/danhmuc.service';
import { DeleteEntityDialogComponent } from '../../../_shared/delete-entity-dialog/delete-entity-dialog.component';
import { FormGroup, FormBuilder } from '@angular/forms';
import { GroupingState, PaginatorState } from 'src/app/_metronic/shared/crud-table';
import { SortState } from './../../../../../_metronic/shared/crud-table/models/sort.model';
import { catchError, tap } from 'rxjs/operators';
import { ResultModel } from '../../../_core/models/_base.model';
import { AccountManagementChinhSuaJeeHRDialogComponent } from '../account-management-chinhsua-jeehr-dialog/account-management-chinhsua-jeehr-dialog.component';
import { AuthService } from 'src/app/modules/auth';

@Component({
  selector: 'app-account-management-list',
  templateUrl: './account-management-list.component.html',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class AccountManagementListComponent implements OnInit, OnDestroy {
  // Table fields
  loadingSubject = new BehaviorSubject<boolean>(false);
  filterGroup: FormGroup;
  searchGroup: FormGroup;
  paginator: PaginatorState;
  sorting: SortState;
  grouping: GroupingState;
  imgFile: string = '';
  isLoading: boolean = false;
  listApp: AppListDTO[] = [];
  isJeeHR: boolean;
  isAdminHeThong: boolean;
  private subscriptions: Subscription[] = [];

  constructor(
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
    //this.accountManagementService.fetch();
    this.grouping = this.accountManagementService.grouping;
    this.paginator = this.accountManagementService.paginator;
    this.sorting = this.accountManagementService.sorting;
    const sb = this.accountManagementService.isLoading$.subscribe((res) => (this.isLoading = res));
    this.subscriptions.push(sb);
    this.loadListApp();
    const userid = +this.auth.getAuthFromLocalStorage()['user']['customData']['jee-account']['userID'];
    this.danhmuc.isAdminHeThong(userid).subscribe((res) => {
      this.isAdminHeThong = res.IsAdmin;
    });
  }

  loadListApp() {
    const sb = this.accountManagementService
      .GetListAppByCustomerID()
      .pipe(
        tap((res: ResultModel<AppListDTO>) => {
          if (res) {
            this.listApp = res.data;
            this.isJeeHR = this.listApp.map((item) => item.AppCode).includes('JeeHR');
          }
        })
      )
      .subscribe();
    this.subscriptions.push(sb);
  }

  changeKeyword(val) {
    this.search(val);
  }

  changeFilter(filter) {
    this.accountManagementService.patchState({ filter });
  }

  search(searchTerm: string) {
    this.accountManagementService.patchState({ searchTerm });
  }

  create() {
    let saveMessageTranslateParam = '';
    saveMessageTranslateParam += 'Thêm thành công';
    const saveMessage = this.translate.instant(saveMessageTranslateParam);
    const messageType = MessageType.Create;
    if (!this.isJeeHR) {
      const dialogRef = this.dialog.open(AccountManagementEditDialogComponent, {
        data: {},
      });
      const sb = dialogRef.afterClosed().subscribe((res) => {
        if (!res) {
          this.accountManagementService.fetch();
        } else {
          this.layoutUtilsService.showActionNotification(saveMessage, messageType, 4000, true, false);
          this.accountManagementService.fetch();
        }
      });
      this.subscriptions.push(sb);
    }
    if (this.isJeeHR) {
      const dialogRef = this.dialog.open(AccountManagementEditJeeHRDialogComponent, {
        data: {},
      });
      const sb = dialogRef.afterClosed().subscribe((res) => {
        if (!res) {
          this.accountManagementService.fetch();
        } else {
          this.layoutUtilsService.showActionNotification(saveMessage, messageType, 4000, true, false);
          this.accountManagementService.fetch();
        }
      });
      this.subscriptions.push(sb);
    }
  }

  getHeight(): any {
    let tmp_height = 0;
    tmp_height = window.innerHeight - 236;
    return tmp_height + 'px';
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
    this.accountManagementService.patchState({ sorting });
  }

  paginate(paginator: PaginatorState) {
    this.accountManagementService.patchState({ paginator });
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
        this.accountManagementService.fetch();
      } else {
        this.layoutUtilsService.showActionNotification(saveMessage, messageType, 4000, true, false);
        this.accountManagementService.fetch();
      }
    });
    this.subscriptions.push(sb);
  }

  openQuanLyTrucTiepEdit(Username: string, DirectManager: string) {
    let saveMessageTranslateParam = '';
    DirectManager === '' ? (saveMessageTranslateParam += 'Thêm thành công') : (saveMessageTranslateParam += 'Cập nhật thành công');
    const saveMessage = this.translate.instant(saveMessageTranslateParam);
    let messageType;
    DirectManager === '' ? (messageType = MessageType.Create) : (messageType = MessageType.Update);
    const dialogRef = this.dialog.open(QuanLytrucTiepEditDialogComponent, {
      data: { Username, DirectManager },
    });
    const sb = dialogRef.afterClosed().subscribe((res) => {
      if (!res) {
        this.accountManagementService.fetch();
      } else {
        this.layoutUtilsService.showActionNotification(saveMessage, messageType, 4000, true, false);
        this.accountManagementService.fetch();
      }
    });
    this.subscriptions.push(sb);
  }

  onFileChange(event, username: string) {
    let saveMessageTranslateParam = '';
    saveMessageTranslateParam += 'Thêm thành công';
    const saveMessage = this.translate.instant(saveMessageTranslateParam);
    const messageType = MessageType.Create;
    if (event.target.files && event.target.files[0]) {
      const reader = new FileReader();
      reader.onload = (e: any) => {
        this.imgFile = e.target.result.split(',')[1];
        const postimg = new PostImgModel();
        postimg.imgFile = this.imgFile;
        postimg.Username = username;
        const sb = this.accountManagementService
          .UpdateAvatarWithChangeUrlAvatar(postimg)
          .pipe(
            tap((res) => {
              this.layoutUtilsService.showActionNotification(saveMessage, messageType, 4000, true, false);
              this.imgFile = '';
              this.accountManagementService.fetch();
            }),
            catchError((err) => {
              console.log(err);
              this.layoutUtilsService.showActionNotification(err.error.message, MessageType.Read, 999999999, true, false, 3000, 'top', 0);
              this.imgFile = '';
              return of();
            })
          )
          .subscribe();
        this.subscriptions.push(sb);
      };
      reader.readAsDataURL(event.target.files[0]);
    }
  }

  update(item) {
    let saveMessageTranslateParam = 'COMMOM.CAPNHATTHANHCONG';
    const saveMessage = this.translate.instant(saveMessageTranslateParam);
    const messageType = MessageType.Create;
    if (!this.isJeeHR) {
      const dialogRef = this.dialog.open(AccountManagementChinhSuaNoJeeHRDialogComponent, {
        data: { item: item },
      });
      const sb = dialogRef.afterClosed().subscribe((res) => {
        if (!res) {
          this.accountManagementService.fetch();
        } else {
          this.layoutUtilsService.showActionNotification(saveMessage, messageType, 4000, true, false);
          this.accountManagementService.fetch();
        }
      });
      this.subscriptions.push(sb);
    }
    if (this.isJeeHR) {
      const dialogRef = this.dialog.open(AccountManagementChinhSuaJeeHRDialogComponent, {
        data: { item: item },
      });
      const sb = dialogRef.afterClosed().subscribe((res) => {
        if (!res) {
          this.accountManagementService.fetch();
        } else {
          this.layoutUtilsService.showActionNotification(saveMessage, messageType, 4000, true, false);
          this.accountManagementService.fetch();
        }
      });
      this.subscriptions.push(sb);
    }
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
        this.accountManagementService.fetch();
      } else {
        this.accountManagementService.Delete(UserID).subscribe(
          (res) => {
            this.layoutUtilsService.showActionNotification(saveMessage, messageType, 4000, true, false);
          },
          (error) => {
            this.layoutUtilsService.showActionNotification(error.error.message, MessageType.Read, 999999999, true, false, 3000, 'top', 0);
          },
          () => {
            this.accountManagementService.fetch();
          }
        );
      }
    });
    this.subscriptions.push(sb);
  }

  ngOnDestroy(): void {
    this.subscriptions.forEach((sb) => sb.unsubscribe());
    this.accountManagementService.ngOnDestroy();
  }
}
