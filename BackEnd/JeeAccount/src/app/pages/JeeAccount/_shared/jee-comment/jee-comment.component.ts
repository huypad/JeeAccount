import { BehaviorSubject, of, Subject, Subscription, Observable, interval } from 'rxjs';
import { Component, Input, OnInit, ViewChild, Pipe, ChangeDetectionStrategy, ChangeDetectorRef } from '@angular/core';
import { JeeCommentService } from './jee-comment.service';
import { CdkTextareaAutosize } from '@angular/cdk/text-field';
import { catchError, finalize, takeUntil, tap, share } from 'rxjs/operators';
import { CommentDTO, PostCommentModel, QueryFilterComment, TopicCommentDTO } from './jee-comment.model';

@Component({
  selector: 'app-jee-comment',
  templateUrl: './jee-comment.component.html',
  styleUrls: ['jee-comment.scss'],
  changeDetection: ChangeDetectionStrategy.OnPush,

})
export class JeeCommentComponent implements OnInit {
  private readonly onDestroy = new Subject<void>();
  private _items$ = new BehaviorSubject<TopicCommentDTO>(undefined);
  private _lstComment$ = new BehaviorSubject<CommentDTO[]>([]);
  private _isLoading$ = new BehaviorSubject<boolean>(false);
  private _errorMessage$ = new BehaviorSubject<string>('');
  private _filter$ = new BehaviorSubject<any>({});
  public item: TopicCommentDTO;
  get lstComment$() {
    this._lstComment$.next(this._items$.value.Comments);
    return this._lstComment$.asObservable();
  }
  get lstComment() {
    this._lstComment$.next(this._items$.value.Comments);
    return this._lstComment$.value;
  }
  get items$() {
    return this._items$.asObservable();
  }
  get isLoading$() {
    return this._isLoading$.asObservable();
  }
  get errorMessage$() {
    return this._errorMessage$.asObservable();
  }
  get filter$() {
    return this._filter$.asObservable();
  }

  hiddenLike: boolean = false;
  hiddenShare: boolean = false;
  isFirstTime: boolean = true;
  ShowSpinner$: BehaviorSubject<boolean> = new BehaviorSubject<boolean>(false);
  ShowFilter$: BehaviorSubject<boolean> = new BehaviorSubject<boolean>(false);
  ViewLengthComment: number = 10;
  @Input() objectID: string;
  @Input() showCommentDefault?: boolean;
  @Input() number: number;
  @Input() img: any;
  @ViewChild('autosize') autosize: CdkTextareaAutosize;

  constructor(public service: JeeCommentService, public cd: ChangeDetectorRef) { }

  ngOnInit() {
    if (this.showCommentDefault) {
      setTimeout(() => {
        this.clickButtonComment();
      }, 2000);
    };
  }

  clickButtonComment() {
    if (this.isFirstTime) {
      this.ShowSpinner$.next(true);
      if (this.objectID) {
        const source = interval(1000);
        source.pipe(takeUntil(this.onDestroy)).subscribe(() => {
          if (this._errorMessage$.value == '') {
            this.getShowTopic();
          }
        });
      } else {
        this.ShowSpinner$.next(false);
        this.isFirstTime = false;
      };
    }
  }

  getShowTopic() {
    this._isLoading$.next(true);
    this.service.showTopicCommentByObjectID(this.objectID, this.filter())
      .pipe(
        tap((topicCommentNew: TopicCommentDTO) => {
          if (!this.item) {
            this.item = topicCommentNew;
          }
          else {
            this.pushItemComment(this.item, topicCommentNew);
          }
          if (topicCommentNew.ViewLengthComment < topicCommentNew.TotalLengthComment) {
            this.ShowFilter$.next(true);
          } else {
            this.ShowFilter$.next(false);
          }
        }),
        catchError(err => {
          console.log(err);
          this._errorMessage$.next(err);
          return of();
        }),
        finalize(() => {
          if (this.isFirstTime) {
            this.ShowSpinner$.next(false);
            this.isFirstTime = false;
          }
          this._isLoading$.next(false);
          this.cd.detectChanges();
        }),
        takeUntil(this.onDestroy),
        share())
      .subscribe();
  }

  pushItemComment(topicComment: TopicCommentDTO, topicCommentNew: TopicCommentDTO) {
    topicCommentNew.Comments.forEach((comment) => {
      const index = topicComment.Comments.findIndex(item => item.Id === comment.Id);
      if (index === -1) {
        topicComment.Comments.push(comment);
      } else {
        this.pushItemReplyComment(this.item.Comments[index], comment);
      }
    });
  }

  pushItemReplyComment(Comment: CommentDTO, CommentNew: CommentDTO) {
    CommentNew.Replies.forEach((reply) => {
      const index = Comment.Replies.findIndex(item => item.Id === reply.Id);
      if (index === -1) {
        Comment.Replies.push(reply);
      }
    });
  }

  filter(): QueryFilterComment {
    let filter = new QueryFilterComment();
    filter.ViewLengthComment = this.ViewLengthComment = 10;
    return filter;
  }

  ngOnDestroy(): void {
    this.onDestroy.next();
  }

}
