import {SelectionModel} from '@angular/cdk/collections';
import {ChangeDetectionStrategy, ChangeDetectorRef, Component, EventEmitter, OnInit, ViewChild} from '@angular/core';
import {MatDialog, MatDialogRef} from '@angular/material/dialog';
import {MatPaginator} from '@angular/material/paginator';
import {MatSort} from '@angular/material/sort';
import {MatTableDataSource} from '@angular/material/table';
import {TranslateService} from '@ngx-translate/core';
import {BehaviorSubject, merge} from 'rxjs';
import {tap} from 'rxjs/operators';
import {TokenStorage} from 'src/app/modules/auth/_services/token-storage.service';
import {SubheaderService} from 'src/app/_metronic/partials/layout';
import {QueryParamsModelNew} from '../../_core/models/query-models/query-params.model';
import {LayoutUtilsService, MessageType} from '../../_core/utils/layout-utils.service';
import {DepartmentManagementService} from '../Sevices/department-management.service';
import {DepartmentManagementDatasource} from '../Model/data-sources/department-management.datasource';
import {DepartmentManagementEditDialogComponent} from '../department-management-edit-dialog/department-management-edit-dialog.component';
import {DepartmentModel} from '../Model/department-management.model';

@Component({
    selector: 'app-department-management-list',
    templateUrl: './department-management-list.component.html',
    changeDetection: ChangeDetectionStrategy.OnPush,
})
export class DepartmentManagementLístComponent implements OnInit {
    // Table fields
    loadingSubject = new BehaviorSubject<boolean>(false);
    dataSource: DepartmentManagementDatasource;
    displayedColumns = [];
    availableColumns = [
        {
            stt: 1,
            name: 'PhongBan',
            alwaysChecked: false,
        },
        {
            stt: 2,
            name: 'QuanLy',
            alwaysChecked: false,
        },
        {
            stt: 3,
            name: 'TinhTrang',
            alwaysChecked: false,
        },
        {
            stt: 4,
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
    @ViewChild(MatPaginator, {static: true}) paginator: MatPaginator;
    @ViewChild(MatSort, {static: true}) sort: MatSort;
    dataResult: any[] = [];

    constructor(
        private changeDetect: ChangeDetectorRef,
        private departmentManagementService: DepartmentManagementService,
        private translate: TranslateService,
        public subheaderService: SubheaderService,
        private layoutUtilsService: LayoutUtilsService,
        private tokenStorage: TokenStorage,
        public dialog: MatDialog
    ) {
    }

    //=================PageSize Table=====================
    pageSize = 50;
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
        // set show table data
        this.applySelectedColumns();

        this.sort.sortChange.subscribe(() => (this.paginator.pageIndex = 0));

        // load data khi sort Changed
        merge(this.sort.sortChange, this.paginator.page)
            .pipe(
                tap(() => {
                    this.loadDataList(true);
                })
            )
            .subscribe();

        // Init DataSource
        this.dataSource = new DepartmentManagementDatasource(this.departmentManagementService);
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
        // this..changeTinhTrang(Username).subscribe((res) => {
        //     this.loadDataList();
        // });
    }
}
