import { CommentDTO, QueryFilterComment, ReturnFilterComment } from './../jee-comment.model';
import { BehaviorSubject, interval, of, Subject } from 'rxjs';
import {
  Component,
  Input,
  OnInit,
  OnDestroy,
  ViewChild,
  ElementRef,
  EventEmitter,
  Output,
  ChangeDetectionStrategy,
  ChangeDetectorRef,
} from '@angular/core';
import { JeeCommentService } from '../jee-comment.service';
import { catchError, finalize, takeUntil, tap, share, switchMap } from 'rxjs/operators';

@Component({
  selector: 'jeecomment-post-comment-content',
  templateUrl: 'post-comment-content.component.html',
  styleUrls: ['post-comment-content.component.scss'],
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class JeeCommentPostContentComponent implements OnInit, OnDestroy {
  private readonly onDestroy = new Subject<void>();
  private _errorMessage$ = new BehaviorSubject<string>('');
  private _isLoading$ = new BehaviorSubject<boolean>(false);
  get isLoading$() {
    return this._isLoading$.asObservable();
  }
  get errorMessage$() {
    return this._errorMessage$.asObservable();
  }

  isFirstTime: boolean = true;
  showSpinner$: BehaviorSubject<boolean> = new BehaviorSubject<boolean>(false);
  showEnterComment$: BehaviorSubject<boolean> = new BehaviorSubject<boolean>(false);
  ClickShowReply$: BehaviorSubject<boolean> = new BehaviorSubject<boolean>(false);
  isFocus$$: BehaviorSubject<boolean> = new BehaviorSubject<boolean>(false);
  ShowFilter$: BehaviorSubject<boolean> = new BehaviorSubject<boolean>(false);
  ShowSpinnerViewMore$: BehaviorSubject<boolean> = new BehaviorSubject<boolean>(false);

  //filter
  filterViewLengthComment: number = 10;
  filterDate: Date = new Date();

  @Input() inputLstObjectID: string[];
  public lstObjectID: string[] = [];
  objectID: string = '';
  commentID: string = '';
  replyCommentID: string = '';
  @Input() comment?: CommentDTO;
  @Input() showCommentDefault?: boolean;
  @Input() isDeteachChange$?: BehaviorSubject<boolean> = new BehaviorSubject<boolean>(false);
  @Output() isFocus = new EventEmitter<any>();

  isDeteachChangeComment$?: BehaviorSubject<boolean> = new BehaviorSubject<boolean>(false);
  constructor(public service: JeeCommentService, public cd: ChangeDetectorRef, private elementRef: ElementRef) {}

  ngOnInit() {
    if (this.isDeteachChange$) {
      this.isDeteachChange$
        .pipe(
          switchMap(async (res) => {
            if (res) {
              this.cd.detectChanges();
              this.isDeteachChange$.next(false);
              this.isDeteachChangeComment$.next(true);
            }
          })
        )
        .subscribe();
    }
    if (this.inputLstObjectID.length == 1) {
      this.initObjectID();
      this.lstObjectID.push(this.comment.Id);
      this.commentID = this.comment.Id;
    }
    if (this.inputLstObjectID.length == 2) {
      this.initObjectID();
      this.initCommentID();
      this.replyCommentID = this.comment.Id;
    }

    if (this.showCommentDefault) {
      setTimeout(() => {
        this.clickButtonShowReply();
      }, 1000);
    }
  }

  initObjectID() {
    const objectid = this.inputLstObjectID[0];
    this.lstObjectID.push(objectid);
    this.objectID = objectid;
  }

  initCommentID() {
    const commentid = this.inputLstObjectID[1];
    this.lstObjectID.push(commentid);
    this.commentID = commentid;
  }

  showEnterComment() {
    this.ClickShowReply$.next(true);
    if (this.replyCommentID === '') {
      this.showEnterComment$.next(true);
    }
    this.isFocus.emit(true);
    this.clickButtonShowReply();
  }

  clickButtonShowReply() {
    if (this.isFirstTime) {
      this.ClickShowReply$.next(true);
      this.showEnterComment$.next(true);
      this.showSpinner$.next(true);
      if (this.objectID && this.comment.Id) {
        this.showFullComment();
      } else {
        this.showSpinner$.next(false);
        this.isFirstTime = false;
      }
    }
  }

  showFullComment() {
    this.service
      .showFullComment(this.objectID, this.commentID, this.filter())
      .pipe(
        tap((CommentDTO: CommentDTO) => {
          if (this.isFirstTime) this.comment.Replies = CommentDTO.Replies;
          if (this.comment.ViewLengthComment < CommentDTO.TotalLengthComment) {
            this.ShowFilter$.next(true);
          } else {
            this.ShowFilter$.next(false);
          }
        }),
        catchError((err) => {
          console.log(err);
          this._errorMessage$.next(err);
          return of();
        }),
        finalize(() => {
          if (this.isFirstTime) {
            this.showSpinner$.next(false);
            this.isFirstTime = false;
          }
          this._isLoading$.next(false);
          this.cd.detectChanges();
        }),
        takeUntil(this.onDestroy),
        share()
      )
      .subscribe();
  }

  pushItemReplyCommentInComment(commentDTO: CommentDTO, lstCommentDTO: CommentDTO[]) {
    lstCommentDTO.forEach((comment) => {
      commentDTO.Replies.push(comment);
    });
  }

  deleteItemReplyCommentInComment(commentDTO: CommentDTO, lstStringObjectIDComment: string[]) {
    lstStringObjectIDComment.forEach((commentID) => {
      const index = commentDTO.Replies.findIndex((item) => item.Id === commentID);
      if (index !== -1) {
        commentDTO.Replies.splice(index, 1);
      }
    });
  }

  editItemReplyCommentInComment(commentDTO: CommentDTO, lstCommentDTO: CommentDTO[]) {
    lstCommentDTO.forEach((comment) => {
      const index = commentDTO.Replies.findIndex((item) => item.Id === comment.Id);
      if (index !== -1) {
        this.copyComment(commentDTO.Replies[index], comment);
      }
    });
  }

  copyComment(mainCommentDTO: CommentDTO, newCommentDTO: CommentDTO) {
    if (mainCommentDTO.Text !== newCommentDTO.Text) mainCommentDTO.Text = newCommentDTO.Text;
    if (mainCommentDTO.Attachs !== newCommentDTO.Attachs) mainCommentDTO.Attachs = newCommentDTO.Attachs;
    if (mainCommentDTO.IsEdit !== newCommentDTO.IsEdit) mainCommentDTO.IsEdit = newCommentDTO.IsEdit;
    if (mainCommentDTO.DateCreated !== newCommentDTO.DateCreated) mainCommentDTO.DateCreated = newCommentDTO.DateCreated;
    if (mainCommentDTO.IsUserReply !== newCommentDTO.IsUserReply) mainCommentDTO.IsUserReply = newCommentDTO.IsUserReply;
    if (mainCommentDTO.LengthReply !== newCommentDTO.LengthReply) mainCommentDTO.LengthReply = newCommentDTO.LengthReply;
    if (mainCommentDTO.MostLengthReaction !== newCommentDTO.MostLengthReaction)
      mainCommentDTO.MostLengthReaction = newCommentDTO.MostLengthReaction;
    if (mainCommentDTO.MostTypeReaction !== newCommentDTO.MostTypeReaction)
      mainCommentDTO.MostTypeReaction = newCommentDTO.MostTypeReaction;
    if (mainCommentDTO.TotalLengthComment !== newCommentDTO.TotalLengthComment)
      mainCommentDTO.TotalLengthComment = newCommentDTO.TotalLengthComment;
    if (mainCommentDTO.TotalLengthReaction !== newCommentDTO.TotalLengthReaction)
      mainCommentDTO.TotalLengthReaction = newCommentDTO.TotalLengthReaction;
    if (mainCommentDTO.UserReaction !== newCommentDTO.UserReaction) mainCommentDTO.UserReaction = newCommentDTO.UserReaction;
    if (mainCommentDTO.UserReactionColor !== newCommentDTO.UserReactionColor)
      mainCommentDTO.UserReactionColor = newCommentDTO.UserReactionColor;
    this.cd.detectChanges();
  }

  ngOnDestroy(): void {
    this.onDestroy.next();
  }

  getFocus(event) {
    this.isFocus$$.next(true);
  }

  filter(): QueryFilterComment {
    let filter = new QueryFilterComment();
    filter.ViewLengthComment = this.filterViewLengthComment;
    filter.Date = this.filterDate;
    return filter;
  }

  viewMoreComment() {
    this.filterViewLengthComment += 10;
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
    const isVisible = rect.top < window.innerHeight && rect.bottom >= 0;
    return isVisible;
  }
}
