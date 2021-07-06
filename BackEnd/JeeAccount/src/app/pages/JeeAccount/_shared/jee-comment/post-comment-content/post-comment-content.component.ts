import { style } from '@angular/animations';
import { CommentDTO, QueryFilterComment } from './../jee-comment.model';
import { BehaviorSubject, interval, of, Subject } from 'rxjs';
import { Component, Input, OnInit, OnDestroy, ViewChild, ElementRef, EventEmitter, Output, ChangeDetectionStrategy, ChangeDetectorRef, HostListener } from '@angular/core';
import { JeeCommentService } from '../jee-comment.service';
import { catchError, finalize, takeUntil, tap, share } from 'rxjs/operators';

@Component({
  selector: 'jeecomment-post-comment-content',
  templateUrl: 'post-comment-content.component.html',
  styleUrls: ['post-comment-content.component.scss'],
  changeDetection: ChangeDetectionStrategy.OnPush,

})

export class JeeCommentPostContentComponent implements OnInit, OnDestroy {
  private readonly onDestroy = new Subject<void>();
  constructor(public service: JeeCommentService, public cd: ChangeDetectorRef, private elementRef: ElementRef) { }

  @Input() objectID: string;
  @Input() comment: CommentDTO;
  @Input() indexNested: number;
  @Input() showCommentDefault?: boolean;
  @Output() isFocus = new EventEmitter<any>();

  isFirstTime: boolean = true;
  showSpinner$: BehaviorSubject<boolean> = new BehaviorSubject<boolean>(false);
  showEnterComment$: BehaviorSubject<boolean> = new BehaviorSubject<boolean>(false);
  ClickShowReply$: BehaviorSubject<boolean> = new BehaviorSubject<boolean>(false);
  isFocus$$: BehaviorSubject<boolean> = new BehaviorSubject<boolean>(false);
  ShowFilter$: BehaviorSubject<boolean> = new BehaviorSubject<boolean>(false);
  ShowSpinnerViewMore$: BehaviorSubject<boolean> = new BehaviorSubject<boolean>(false);
  ViewLengthComment: number = 10;

  private _errorMessage$ = new BehaviorSubject<string>('');
  private _filter$ = new BehaviorSubject<any>({});

  get errorMessage$() {
    return this._errorMessage$.asObservable();
  }

  get filter$() {
    return this._filter$.asObservable();
  }


  ngOnInit() {
    if (this.showCommentDefault) {
      setTimeout(() => {
        this.showReply();
      }, 1000);
    }
  }

  showEnterComment() {
    this.ClickShowReply$.next(true);
    this.showEnterComment$.next(true);
    this.isFocus.emit(true);
    this.showReply();
  }

  showReply() {
    if (this.isFirstTime) {
      this.ClickShowReply$.next(true);
      this.showEnterComment$.next(true);
      this.showSpinner$.next(true);
      if (this.objectID && this.comment.Id) {
        this.showFullComment();
        const source = interval(1000);
        source.pipe(takeUntil(this.onDestroy)).subscribe(() => {
          if (this._errorMessage$.value == '' && this.isScrolledViewElement()) {
            this.showFullComment();
          }
        });
      } else {
        this.showSpinner$.next(false);
        this.isFirstTime = false;
      };
    }
  }

  showFullComment() {
    this.service.showFullComment(this.objectID, this.comment.Id, this.filter()).pipe(
      tap((CommentDTO: CommentDTO) => {
        if (this.isFirstTime)
          this.comment = CommentDTO;
        else {
          this.pushItemReplyComment(this.comment, CommentDTO);
        }
        if (this.comment.ViewLengthComment < CommentDTO.TotalLengthComment) {
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
          this.showSpinner$.next(false);
          this.isFirstTime = false;
        }
        this.cd.detectChanges();
      }),
      takeUntil(this.onDestroy),
      share())
      .subscribe();
  }

  pushItemReplyComment(comment: CommentDTO, CommentNew: CommentDTO) {
    comment.TotalLengthComment = CommentNew.TotalLengthComment;
    CommentNew.Replies.forEach((reply, pos) => {
      const index = comment.Replies.findIndex(item => item.Id === reply.Id);
      if (index === -1) {
        comment.Replies.splice(pos, 0, reply);
      }
    });
  }

  ngOnDestroy(): void {
    this.onDestroy.next();
  }

  getFocus(event) {
    this.isFocus$$.next(true);
  }

  filter(): QueryFilterComment {
    let filter = new QueryFilterComment();
    filter.ViewLengthComment = this.ViewLengthComment;
    return filter;
  }

  viewMoreComment() {
    this.ViewLengthComment += 10;
    this.ShowSpinnerViewMore$.next(true);
    setTimeout(() => {
      this.ShowSpinnerViewMore$.next(false);
    }, 750);
  }

  @ViewChild('videoPlayer') videoplayer: ElementRef;
  toggleVideo() {
    this.videoplayer.nativeElement.play();
  }

  isScrolledViewElement() {
    const rect = this.elementRef.nativeElement.getBoundingClientRect();
    const isVisible = rect.top < window.innerHeight && rect.bottom >= 0
    return isVisible;
  }

}