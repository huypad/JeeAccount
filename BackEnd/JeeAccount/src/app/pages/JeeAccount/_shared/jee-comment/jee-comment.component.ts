import { BehaviorSubject, of, Subject, Subscription } from 'rxjs';
import { Component, Input, OnInit, ViewChild } from '@angular/core';
import { JeeCommentService } from './jee-comment.service';
import { CdkTextareaAutosize } from '@angular/cdk/text-field';
import { catchError, finalize, takeUntil, tap } from 'rxjs/operators';
import { CommentDTO, CommentModel, QueryFilterComment, TopicCommentDTO } from './jee-comment.model';

@Component({
  selector: 'app-jee-comment',
  templateUrl: './jee-comment.component.html',
  styleUrls: ['jee-comment.scss'],
})
export class JeeCommentComponent implements OnInit {
  private readonly onDestroy = new Subject<void>();
  private _items$ = new BehaviorSubject<TopicCommentDTO>(undefined);
  private _lstComment$ = new BehaviorSubject<CommentDTO[]>([]);
  private _isLoading$ = new BehaviorSubject<boolean>(false);
  private _errorMessage$ = new BehaviorSubject<string>('');
  private _filter$ = new BehaviorSubject<any>({});

  get lstComment$() {
    this._lstComment$.next(this._items$.value.Comments);
    return this._lstComment$.asObservable();
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
  postComment: CommentModel = new CommentModel();
  @Input() objectID?: string;
  subscription: Subscription[] = [];

  @ViewChild('autosize') autosize: CdkTextareaAutosize;

  constructor(public service: JeeCommentService) { }

  ngOnInit() {
    if (!this.objectID) {
      this.objectID = '60d1cc345c3ba95b5171466c';
    }
    this.innitData();
  }

  innitData() {
    this.postComment.TopicCommentID = this.objectID ? this.objectID : '';
  }

  clickButtonComment() {
    if (this.isFirstTime) {
      console.log(this.isFirstTime);
      this.ShowSpinner$.next(true);
      this.getShowTopic();
    }
  }

  getShowTopic() {
    this._errorMessage$.next('');
    this._isLoading$.next(true);
    this.service.showTopicCommentByObjectID(this.objectID, this.filter())
      .pipe(
        tap((res: TopicCommentDTO) => {
          if (this.isFirstTime) {
            this.ShowSpinner$.next(false);
            this.isFirstTime = false;
          }
          this._items$.next(res);
          if (res.ViewLengthComment < res.TotalLengthComment) {
            this.ShowFilter$.next(true);
          } else {
            this.ShowFilter$.next(false);
          }
        }),
        catchError(err => {
          this._errorMessage$.next(err);
          return of();
        }),
        finalize(() => {
          this._isLoading$.next(false);
        }),
        takeUntil(this.onDestroy)).
      subscribe();
  }

  filter(): QueryFilterComment {
    let filter = new QueryFilterComment();
    filter.ViewLengthComment = 10;
    return filter;
  }

  ngOnDestroy(): void {
    this.onDestroy.next();
  }

  // khu vực thêm xoá sửa comment
  ValidateCommentAndPost() {
    console.log(this.postComment);

  }

  onKeydown($event) {
    if (($event.ctrlKey && $event.keyCode == 13) || ($event.altKey && $event.keyCode == 13)) {
      console.log('???');
      this.postComment.Text = this.postComment.Text + '\n';
    } else if ($event.keyCode == 13) {
      console.log('enter');
      $event.preventDefault();
    }
  }
}
