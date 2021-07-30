import { DemoCommentService } from './demo-comment.service';
import { Component, OnInit, OnDestroy } from '@angular/core';
// Material
// RXJS
import { Subject, of, BehaviorSubject } from 'rxjs';
import { finalize, tap, share, takeUntil, catchError } from 'rxjs/operators';

@Component({
  selector: 'app-demo-comment',
  templateUrl: './demo-comment.component.html',
})
export class DemoCommentComponent implements OnInit, OnDestroy {

  topicObjectID$: BehaviorSubject<string> = new BehaviorSubject<string>('');
  private readonly componentName = 'm-danh-sach-truy-cap-nhanh-widget';
  private readonly onDestroy = new Subject<void>();
  numbers = Array(1).fill(1).map((x, i) => x + i);
  img = 'https://picsum.photos/200/300?random=2';
  imgsurl: any[] = [];
  constructor(public commentService: DemoCommentService) { }

  ngOnInit() {
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
