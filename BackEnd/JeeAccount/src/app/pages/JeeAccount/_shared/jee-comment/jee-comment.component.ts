import { BehaviorSubject, of, Subject, interval } from 'rxjs';
import { Component, Input, OnInit, ViewChild, ChangeDetectionStrategy, ChangeDetectorRef, ElementRef } from '@angular/core';
import { JeeCommentService } from './jee-comment.service';
import { CdkTextareaAutosize } from '@angular/cdk/text-field';
import { catchError, finalize, takeUntil, tap, share } from 'rxjs/operators';
import { CommentDTO, QueryFilterComment, ReturnFilterComment, TopicCommentDTO } from './jee-comment.model';

@Component({
  selector: 'app-jee-comment',
  templateUrl: './jee-comment.component.html',
  styleUrls: ['jee-comment.scss'],
  changeDetection: ChangeDetectionStrategy.OnPush,

})
export class JeeCommentComponent implements OnInit {
  private readonly onDestroy = new Subject<void>();
  private _isLoading$ = new BehaviorSubject<boolean>(false);
  private _errorMessage$ = new BehaviorSubject<string>('');
  get isLoading$() {
    return this._isLoading$.asObservable();
  }
  get errorMessage$() {
    return this._errorMessage$.asObservable();
  }

  item: TopicCommentDTO;
  hiddenLike: boolean = false;
  hiddenShare: boolean = false;
  isFirstTime: boolean = true;
  ShowSpinner$: BehaviorSubject<boolean> = new BehaviorSubject<boolean>(false);
  ShowFilter$: BehaviorSubject<boolean> = new BehaviorSubject<boolean>(false);
  ShowSpinnerViewMore$: BehaviorSubject<boolean> = new BehaviorSubject<boolean>(false);

  //filter
  filterViewLengthComment: number = 10;
  filterDate: Date = new Date();;
  filterLstObjectID: string[] = [];

  @Input() isDeteachChange$?: BehaviorSubject<boolean> = new BehaviorSubject<boolean>(false);
  @Input() objectID: string;
  @Input() showCommentDefault?: boolean;
  @Input() number: number;
  public lstObjectID: string[] = [];

  //demo
  @Input() img: any;
  @ViewChild('autosize') autosize: CdkTextareaAutosize;

  constructor(public service: JeeCommentService, public cd: ChangeDetectorRef, private elementRef: ElementRef) { }

  ngOnInit() {
    if (this.objectID) this.lstObjectID.push(this.objectID);
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
        this.getShowTopic();
        const source = interval(1000);
        source.pipe(takeUntil(this.onDestroy)).subscribe(() => {
          if (this._errorMessage$.value == '' && this.isScrolledViewElement() && this._isLoading$.value === false) {
            this.getShowChangeTopic();
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
        tap((topic: TopicCommentDTO) => {
          this.item = topic;
          topic.Comments.forEach(element => {
            this.filterLstObjectID.push(element.Id);
          });
          if (topic.ViewLengthComment < topic.TotalLengthComment) {
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

  getShowChangeTopic() {
    this._isLoading$.next(true);
    this.service.showChangeTopicCommentByObjectID(this.objectID, this.filter())
      .pipe(
        tap((result: ReturnFilterComment) => {
          if (result.LstCreate.length > 0 || result.LstEdit.length > 0 || result.LstStringObjectIDDelete.length > 0) {
            this.updateLength(this.item.ViewLengthComment, result.LstCreate.length, result.LstStringObjectIDDelete.length);
            this.updateLength(this.item.TotalLengthReaction, result.LstCreate.length, result.LstStringObjectIDDelete.length);
            if (result.LstCreate.length > 0) {
              this.pushItemCommentInTopicComemnt(this.item, result.LstCreate);
            }
            if (result.LstEdit.length > 0) {
              this.editItemCommentInTopicComemnt(this.item, result.LstEdit);
            }
            if (result.LstStringObjectIDDelete.length > 0) {
              this.deleteItemCommentInTopicComemnt(this.item, result.LstStringObjectIDDelete);
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

  pushItemCommentInTopicComemnt(topicComment: TopicCommentDTO, lstCommentDTO: CommentDTO[]) {
    lstCommentDTO.forEach((comment) => {
      topicComment.Comments.push(comment);
      this.filterLstObjectID.push(comment.Id);
    });
  }

  deleteItemCommentInTopicComemnt(topicComment: TopicCommentDTO, lstStringObjectIDComment: string[]) {
    lstStringObjectIDComment.forEach((commentID) => {
      const index = topicComment.Comments.findIndex(item => item.Id === commentID);
      if (index !== -1) {
        topicComment.Comments.splice(index, 1);
        this.filterLstObjectID.splice(index, 1);
      }
    });
  }

  editItemCommentInTopicComemnt(topicComment: TopicCommentDTO, lstCommentDTO: CommentDTO[]) {
    if (lstCommentDTO.length > 0) console.log('lst edit', lstCommentDTO);
    lstCommentDTO.forEach((comment) => {
      const index = topicComment.Comments.findIndex(item => item.Id === comment.Id);
      if (index !== -1) {
        this.copyComment(topicComment.Comments[index], comment);
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

  ngOnDestroy(): void {
    this.onDestroy.next();
  }

  isScrolledViewElement() {
    const rect = this.elementRef.nativeElement.getBoundingClientRect();
    const isVisible = rect.top < window.innerHeight && rect.bottom >= 0
    return isVisible;
  }
}
