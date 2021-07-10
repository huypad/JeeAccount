import { CommentDTO, QueryFilterComment, ReturnFilterComment } from './../jee-comment.model';
import { BehaviorSubject, interval, of, Subject } from 'rxjs';
import { Component, Input, OnInit, OnDestroy, ViewChild, ElementRef, EventEmitter, Output, ChangeDetectionStrategy, ChangeDetectorRef } from '@angular/core';
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
  filterDate: Date = new Date();;
  filterLstObjectID: string[] = [];

  @Input() inputLstObjectID: string[];
  public lstObjectID: string[] = [];
  objectID: string = '';
  commentID: string = '';
  replyCommentID: string = '';
  @Input() comment?: CommentDTO;
  @Input() showCommentDefault?: boolean;
  @Input() isDeteachChange$?: BehaviorSubject<boolean> = new BehaviorSubject<boolean>(false);
  @Output() isFocus = new EventEmitter<any>();

  constructor(public service: JeeCommentService, public cd: ChangeDetectorRef, private elementRef: ElementRef) { }

  ngOnInit() {
    this.isDeteachChange$.subscribe((res) => {
      if (res) {
        this.cd.detectChanges();
        this.isDeteachChange$.next(false);
      }
    });

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
    this.showEnterComment$.next(true);
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
        // const source = interval(1000);
        // source.pipe(takeUntil(this.onDestroy)).subscribe(() => {
        //   if (this._errorMessage$.value == '' && this.isScrolledViewElement() && this._isLoading$.value === false) {
        //     this.getShowChangeComment();
        //   }
        // });
      } else {
        this.showSpinner$.next(false);
        this.isFirstTime = false;
      };
    }
  }

  showFullComment() {
    this.service.showFullComment(this.objectID, this.commentID, this.filter()).pipe(
      tap((CommentDTO: CommentDTO) => {
        if (this.isFirstTime)
          this.comment = CommentDTO;
        CommentDTO.Replies.forEach(element => {
          this.filterLstObjectID.push(element.Id);
        });
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
        this._isLoading$.next(false);
        this.cd.detectChanges();
      }),
      takeUntil(this.onDestroy),
      share())
      .subscribe();
  }

  getShowChangeComment() {
    this._isLoading$.next(true);
    this.service.showChangeComment(this.objectID, this.comment.Id, this.filter())
      .pipe(
        tap((result: ReturnFilterComment) => {
          if (result.LstCreate.length > 0 || result.LstEdit.length > 0 || result.LstStringObjectIDDelete.length > 0) {
            this.updateLength(this.comment.ViewLengthComment, result.LstCreate.length, result.LstStringObjectIDDelete.length);
            this.updateLength(this.comment.TotalLengthReaction, result.LstCreate.length, result.LstStringObjectIDDelete.length);
            if (result.LstCreate.length > 0) {
              this.pushItemReplyCommentInComment(this.comment, result.LstCreate);
            }
            if (result.LstEdit.length > 0) {
              console.log('trong nay ne');
              this.editItemReplyCommentInComment(this.comment, result.LstEdit);
            }
            if (result.LstStringObjectIDDelete.length > 0) {
              this.deleteItemReplyCommentInComment(this.comment, result.LstStringObjectIDDelete);
            }
            this.isDeteachChange$.next(true);
          }

        }),
        catchError(err => {
          console.log(err);
          this._isLoading$.next(false);
          this._errorMessage$.next(err);
          return of();
        }),
        finalize(() => {
          this.filterDate = new Date();
          this._isLoading$.next(false);
          this.cd.detectChanges();
        }),
        takeUntil(this.onDestroy),
        share())
      .subscribe();
  }

  updateLength(currentLength: number, lengthLstCreate: number, lengthLstDelete: number) {
    currentLength = currentLength + lengthLstCreate - lengthLstDelete;
  }

  pushItemReplyCommentInComment(commentDTO: CommentDTO, lstCommentDTO: CommentDTO[]) {
    lstCommentDTO.forEach((comment) => {
      commentDTO.Replies.push(comment);
    });
  }

  deleteItemReplyCommentInComment(commentDTO: CommentDTO, lstStringObjectIDComment: string[]) {
    lstStringObjectIDComment.forEach((commentID) => {
      const index = commentDTO.Replies.findIndex(item => item.Id === commentID);
      if (index !== -1) {
        commentDTO.Replies.splice(index, 1);
      }
    });
  }

  editItemReplyCommentInComment(commentDTO: CommentDTO, lstCommentDTO: CommentDTO[]) {
    lstCommentDTO.forEach((comment) => {
      const index = commentDTO.Replies.findIndex(item => item.Id === comment.Id);
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
    if (mainCommentDTO.MostLengthReaction !== newCommentDTO.MostLengthReaction) mainCommentDTO.MostLengthReaction = newCommentDTO.MostLengthReaction;
    if (mainCommentDTO.MostTypeReaction !== newCommentDTO.MostTypeReaction) mainCommentDTO.MostTypeReaction = newCommentDTO.MostTypeReaction;
    if (mainCommentDTO.TotalLengthComment !== newCommentDTO.TotalLengthComment) mainCommentDTO.TotalLengthComment = newCommentDTO.TotalLengthComment;
    if (mainCommentDTO.TotalLengthReaction !== newCommentDTO.TotalLengthReaction) mainCommentDTO.TotalLengthReaction = newCommentDTO.TotalLengthReaction;
    if (mainCommentDTO.UserReaction !== newCommentDTO.UserReaction) mainCommentDTO.UserReaction = newCommentDTO.UserReaction;
    if (mainCommentDTO.UserReactionColor !== newCommentDTO.UserReactionColor) mainCommentDTO.UserReactionColor = newCommentDTO.UserReactionColor;
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
    filter.LstObjectID = this.filterLstObjectID;
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
    const isVisible = rect.top < window.innerHeight && rect.bottom >= 0
    return isVisible;
  }

}