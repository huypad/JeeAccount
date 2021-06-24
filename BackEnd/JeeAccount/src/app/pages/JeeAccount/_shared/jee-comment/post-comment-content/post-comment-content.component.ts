import { BehaviorSubject, of, Subject } from 'rxjs';
import { Component, Input, OnInit, OnDestroy, ViewChild, ElementRef } from '@angular/core';
import { CommentDTO } from '../jee-comment.model';
import { JeeCommentService } from '../jee-comment.service';
import { catchError, finalize, takeUntil, tap } from 'rxjs/operators';

@Component({
  selector: 'jeecomment-post-comment-content',
  templateUrl: 'post-comment-content.component.html',
  styleUrls: ['post-comment-content.component.scss'],
})

export class JeeCommentPostContentComponent implements OnInit, OnDestroy {
  private readonly onDestroy = new Subject<void>();
  constructor(public service: JeeCommentService) { }

  @Input() ObjectID: string;
  @Input() Comment: CommentDTO;
  @Input() ShowAllReply?: boolean;
  comment$ = new BehaviorSubject<CommentDTO>(undefined);
  ShowSpinner$: BehaviorSubject<boolean> = new BehaviorSubject<boolean>(false);

  postInput = '';
  ngOnInit() {
    this.comment$.next(this.Comment);
    if (this.ShowAllReply) {
      this.showReply();
    }
  }
  showReply() {
    this.ShowSpinner$.next(true);
    this.service.showFullComment(this.ObjectID, this.Comment.Id)
      .pipe(
        tap((res) => {
          this.ShowSpinner$.next(false);
          this.comment$.next(res);
        }),
        catchError(err => {
          console.log(err);
          return of();
        }),
        finalize(() => {
        }),
        takeUntil(this.onDestroy)).
      subscribe();
  }
  ngOnDestroy(): void {
    this.onDestroy.next();
  }

  @ViewChild('videoPlayer') videoplayer: ElementRef;
  toggleVideo() {
    this.videoplayer.nativeElement.play();
  }
}