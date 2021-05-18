import { SelectionModel } from '@angular/cdk/collections';
import { ChangeDetectionStrategy, ChangeDetectorRef, Component, EventEmitter, OnInit, ViewChild } from '@angular/core';
import { MatDialog, MatDialogRef } from '@angular/material/dialog';
import { MatPaginator } from '@angular/material/paginator';
import { MatSort } from '@angular/material/sort';
import { MatTableDataSource } from '@angular/material/table';
import { TranslateService } from '@ngx-translate/core';
import { BehaviorSubject, merge } from 'rxjs';
import { tap } from 'rxjs/operators';
import { TokenStorage } from 'src/app/modules/auth/_services/token-storage.service';
import { SubheaderService } from 'src/app/_metronic/partials/layout';
import { QueryParamsModelNew } from '../../_core/models/query-models/query-params.model';
import {LayoutUtilsService, MessageType} from '../../_core/utils/layout-utils.service';
import { AccountManagementEditDialogComponent } from '../account-management-edit-dialog/account-management-edit-dialog.component';
import {AccountManagementDTO, PostImgModel} from '../Model/account-management.model';
import { AccountManagementDataSource } from '../Model/data-sources/account-management.datasource';
import { AccountManagementService } from '../Services/account-management.service';
import {DanhMucChungService} from '../../_core/services/danhmuc.service';

@Component({
  selector: 'app-account-management-list',
  templateUrl: './account-management-list.component.html',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class AccountManagementLístComponent implements OnInit {
  // Table fields
  loadingSubject = new BehaviorSubject<boolean>(false);
  dataSource: AccountManagementDataSource;
  displayedColumns = [];
  availableColumns = [
    {
      stt: 1,
      name: 'NhanVien',
      alwaysChecked: false,
    },
    {
      stt: 2,
      name: 'TenDangNhap',
      alwaysChecked: false,
    },
    {
      stt: 3,
      name: 'ChucVu',
      alwaysChecked: false,
    },
    {
      stt: 4,
      name: 'QuanLyTrucTiep',
      alwaysChecked: false,
    },
    {
      stt: 5,
      name: 'TinhTrang',
      alwaysChecked: false,
    },
    {
      stt: 6,
      name: 'GhiChu',
      alwaysChecked: false,
    },
    {
      stt: 99,
      name: 'ThaoTac',
      alwaysChecked: false,
    },
  ];
  selectedColumns = new SelectionModel<any>(true, this.availableColumns);
  @ViewChild(MatPaginator, { static: true }) paginator: MatPaginator;
  @ViewChild(MatSort, { static: true }) sort: MatSort;
  dataResult: AccountManagementDTO[] = [];
  imgFile: string = '';
  constructor(
    private changeDetect: ChangeDetectorRef,
    private accountManagementService: AccountManagementService,
    private translate: TranslateService,
    public subheaderService: SubheaderService,
    private layoutUtilsService: LayoutUtilsService,
    private tokenStorage: TokenStorage,
    public dialog: MatDialog,
    public  danhmuc: DanhMucChungService
  ) {}

  //=================PageSize Table=====================
  pageSize: number = 50;
  dataAccounts: any[] = [];
  //==========Dropdown Search==============
  filter: any = {};

  ngOnInit() {
    this.tokenStorage.getPageSize().subscribe((res) => {
      this.pageSize = +res;
    });

    this.beginMatTable();
  }

  beginMatTable() {
    //set show table data
    this.applySelectedColumns();

    this.sort.sortChange.subscribe(() => (this.paginator.pageIndex = 0));

    //load data khi sort Changed
    merge(this.sort.sortChange, this.paginator.page)
      .pipe(
        tap(() => {
          this.loadDataList(true);
        })
      )
      .subscribe();

    // Init DataSource
    this.dataSource = new AccountManagementDataSource(this.accountManagementService);
    // Read from URL itemId, for restore previous state
    this.dataSource.entitySubject.subscribe((res) => (this.dataResult = res));
    // First load list
    this.loadDataList();
  }

  /*=========== Check columns of data grid ==========*/
  applySelectedColumns() {
    const selectedColumns: string[] = [];
    this.selectedColumns.selected
      .sort((a, b) => {
        return a.stt - b.stt;
      })
      .forEach((col) => {
        selectedColumns.push(col.name);
      });
    this.displayedColumns = selectedColumns;
  }

  /*=================================================*/

  /** FILTRATION */
  filterConfiguration(): any {
    return this.filter;
  }

  loadDataList(page: boolean = false) {
    const queryParams = new QueryParamsModelNew(
      this.filterConfiguration(),
      this.sort.direction,
      this.sort.active,
      page ? this.paginator.pageIndex : (this.paginator.pageIndex = 0),
      this.paginator.pageSize
    );
    this.dataSource.LoadList(queryParams);
  }

  create() {
    let saveMessageTranslateParam = '';
    saveMessageTranslateParam += 'Thêm thành công';
    const saveMessage = this.translate.instant(saveMessageTranslateParam);
    const messageType = MessageType.Create;
    const dialogRef = this.dialog.open(AccountManagementEditDialogComponent, {
      data: { },
    });
    dialogRef.afterClosed().subscribe((res) => {
      if (!res) {
        this.loadDataList();
      } else {
        this.layoutUtilsService.showActionNotification(saveMessage, messageType, 4000, true, false);
        this.loadDataList();
      }
    });
  }

  getHeight(): any {
    let tmp_height = 0;
    tmp_height = window.innerHeight - 236;
    return tmp_height + 'px';
  }

  changeTinhTrang(Username: string) {
    this.accountManagementService.changeTinhTrang(Username).subscribe((res) => {
      this.loadDataList();
    });
  }
  
  onFileChange(event, username: string) {
    console.log({username: username});
    let saveMessageTranslateParam = '';
    saveMessageTranslateParam += 'Thêm thành công';
    const saveMessage = this.translate.instant(saveMessageTranslateParam);
    const messageType = MessageType.Create;
    if (event.target.files && event.target.files[0]) {
      const reader = new FileReader();
      reader.onload = (e: any) => {
        this.imgFile =  e.target.result.split(',')[1];
        const postimg = new PostImgModel();
        postimg.imgFile = this.imgFile;
        postimg.Username = username;
        this.accountManagementService.UpdateAvatar(postimg).subscribe((res => {
          if (res && res.status === 1) {
            this.layoutUtilsService.showActionNotification(saveMessage, messageType, 4000, true, false);
          } else {
            this.layoutUtilsService.showActionNotification(res.error.message, MessageType.Read, 999999999, true, false, 3000, 'top', 0);
          }
        }));
        console.log({imgFile: this.imgFile});
      };
      reader.readAsDataURL(event.target.files[0]);
    }
  }
}
