import { ChangeDetectionStrategy, ChangeDetectorRef, Component, EventEmitter, OnInit, ViewChild } from '@angular/core';
import { MatDialog } from '@angular/material/dialog';
import { TranslateService } from '@ngx-translate/core';
import { BehaviorSubject, of, Subscription } from 'rxjs';
import { SubheaderService } from 'src/app/_metronic/partials/layout';
import { AccountManagementEditDialogComponent } from '../account-management-edit-dialog/account-management-edit-dialog.component';
import { AccountManagementDTO, PostImgModel } from '../Model/account-management.model';
import { AccountManagementService } from '../Services/account-management.service';
import { QuanLytrucTiepEditDialogComponent } from '../quan-ly-truc-tiep-edit-dialog/quan-ly-truc-tiep-edit-dialog.component';
import { ChangeTinhTrangEditDialogComponent } from '../change-tinh-trang-edit-dialog/change-tinh-trang-edit-dialog.component';
import { AccountManagementEditNoJeeHRDialogComponent } from '../account-management-edit-no-jeehr-dialog/account-management-edit-no-jeehr-dialog.component';
import { LayoutUtilsService, MessageType } from '../../../_core/utils/layout-utils.service';
import { DanhMucChungService } from '../../../_core/services/danhmuc.service';
import { DeleteEntityDialogComponent } from '../../../_shared/delete-entity-dialog/delete-entity-dialog.component';
import { FormGroup, FormBuilder } from '@angular/forms';
import { PaginatorState } from 'src/app/_metronic/shared/crud-table';
import { GroupingState } from 'src/app/_metronic/shared/crud-table/grouping.model';
import { SortState } from './../../../../../_metronic/shared/crud-table/models/sort.model';
import { catchError, debounceTime, distinctUntilChanged, tap } from 'rxjs/operators';
import { error } from 'protractor';

@Component({
  selector: 'app-account-management-list',
  templateUrl: './account-management-list.component.html',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class AccountManagementListComponent implements OnInit {
  // Table fields
  loadingSubject = new BehaviorSubject<boolean>(false);
  displayedColumns = ['NhanVien', 'TenDangNhap', 'ChucVu', 'QuanLyTrucTiep', 'TinhTrang', 'GhiChu', 'ThaoTac'];
  filterGroup: FormGroup;
  searchGroup: FormGroup;
  paginator: PaginatorState;
  sorting: SortState;
  grouping: GroupingState;
  dataResult: AccountManagementDTO[] = [];
  imgFile: string = '';
  isLoading: boolean = false;
  private subscriptions: Subscription[] = [];

  constructor(
    public accountManagementService: AccountManagementService,
    private translate: TranslateService,
    public subheaderService: SubheaderService,
    private layoutUtilsService: LayoutUtilsService,
    public dialog: MatDialog,
    public danhmuc: DanhMucChungService,
    private fb: FormBuilder
  ) {}

  //=================PageSize Table=====================
  pageSize: number = 50;

  ngOnInit() {
    this.searchForm();
    this.accountManagementService.fetch();
    this.grouping = this.accountManagementService.grouping;
    this.paginator = this.accountManagementService.paginator;
    this.sorting = this.accountManagementService.sorting;
    const sb = this.accountManagementService.isLoading$.subscribe((res) => (this.isLoading = res));
    this.subscriptions.push(sb);
  }

  // search
  searchForm() {
    this.searchGroup = this.fb.group({
      searchTerm: [''],
    });
    const searchEvent = this.searchGroup.controls.searchTerm.valueChanges
      .pipe(
        /*
      The user can type quite quickly in the input box, and that could trigger a lot of server requests. With this operator,
      we are limiting the amount of server requests emitted to a maximum of one every 150ms
      */
        debounceTime(150),
        distinctUntilChanged()
      )
      .subscribe((val) => this.search(val));
    this.subscriptions.push(searchEvent);
  }

  search(searchTerm: string) {
    this.accountManagementService.patchState({ searchTerm });
  }

  create() {
    let saveMessageTranslateParam = '';
    saveMessageTranslateParam += 'Thêm thành công';
    const saveMessage = this.translate.instant(saveMessageTranslateParam);
    const messageType = MessageType.Create;
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
    this.accountManagementService.fetchStateSort({ sorting });
  }

  paginate(paginator: PaginatorState) {
    this.accountManagementService.fetchStateSort({ paginator });
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
              console.log(res);
              this.layoutUtilsService.showActionNotification(saveMessage, messageType, 4000, true, false);
              this.imgFile = '';
              this.accountManagementService.fetch();
            }),
            catchError((err) => {
              console.log(err);
              this.layoutUtilsService.showActionNotification(err.message, MessageType.Read, 999999999, true, false, 3000, 'top', 0);
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
    let saveMessageTranslateParam = '';
    saveMessageTranslateParam += 'Cập nhật thành công';
    const saveMessage = this.translate.instant(saveMessageTranslateParam);
    const messageType = MessageType.Create;
    const dialogRef = this.dialog.open(AccountManagementEditNoJeeHRDialogComponent, {
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

  delete(item) {
    let saveMessageTranslateParam = '';
    saveMessageTranslateParam += 'Xoá thành công';
    const saveMessage = this.translate.instant(saveMessageTranslateParam);
    const messageType = MessageType.Create;
    const dialogRef = this.dialog.open(DeleteEntityDialogComponent, {
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

  ngOnDestroy(): void {
    this.subscriptions.forEach((sb) => sb.unsubscribe());
  }
}
