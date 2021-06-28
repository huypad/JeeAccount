import { Component, OnInit, ChangeDetectorRef, Input, EventEmitter, OnDestroy } from '@angular/core';
// Material
// RXJS
import { Subscription, Subject, of, Observable, BehaviorSubject } from 'rxjs';
// Services
import { LayoutUtilsService, MessageType } from '../../../_core/utils/layout-utils.service';
// Models
import { QueryParamsModel } from '../../../_core/models/query-models/query-params.model';
import { DanhMucChungService } from '../../../_core/services/danhmuc.service';
import { PageGirdtersDashboardService } from '../../Services/page-girdters-dashboard.service';
import { JeeCommentComponent } from '../../../_shared/jee-comment/jee-comment.component';
import { DemoCommentService } from '../../Services/demo-comment.service';
import { finalize, tap, share, takeUntil, catchError } from 'rxjs/operators';

@Component({
  selector: 'm-danh-sach-truy-cap-nhanh-widget',
  templateUrl: './danh-sach-truy-cap-nhanh.component.html',
})
export class DanhSachTruyCapNhanhWidgetComponent implements OnInit, OnDestroy {

  topicObjectID$: BehaviorSubject<string> = new BehaviorSubject<string>('');
  private readonly componentName = 'm-danh-sach-truy-cap-nhanh-widget';
  private readonly onDestroy = new Subject<void>();
  numbers = Array(100).fill(1).map((x, i) => x + i);
  constructor(public commentService: DemoCommentService) { }

  ngOnInit() {
    console.log(this.numbers);
    this.commentService.getTopicObjectIDByComponentName(this.componentName).pipe(
      tap((res) => {
        this.topicObjectID$.next(res);
      }),
      catchError(err => {
        console.log(err);
        return of();
      }),
      finalize(() => { }),
      share(),
      takeUntil(this.onDestroy),
    ).subscribe();
  }

  ngOnDestroy(): void {
    this.onDestroy.next();
  }

}
