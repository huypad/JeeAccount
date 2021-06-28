import { BehaviorSubject, of, Subject } from 'rxjs';
import { Component, Input, OnInit, OnDestroy, ViewChild, ElementRef } from '@angular/core';
import { CommentDTO } from '../jee-comment.model';
import { JeeCommentService } from '../jee-comment.service';
import { catchError, finalize, takeUntil, tap, share } from 'rxjs/operators';

@Component({
  selector: 'jeecomment-post-comment-content',
  templateUrl: 'post-comment-content.component.html',
  styleUrls: ['post-comment-content.component.scss'],
})

export class JeeCommentPostContentComponent implements OnInit, OnDestroy {
  private readonly onDestroy = new Subject<void>();
  constructor(public service: JeeCommentService) { }

  @Input() objectID: string;
  @Input() comment: CommentDTO;
  showSpinner$: BehaviorSubject<boolean> = new BehaviorSubject<boolean>(false);
  showEnterComment$: BehaviorSubject<boolean> = new BehaviorSubject<boolean>(false);
  ClickShowReply$: BehaviorSubject<boolean> = new BehaviorSubject<boolean>(false);

  ngOnInit() {
  }

  showEnterComment() {
    this.ClickShowReply$.next(true);
    this.showEnterComment$.next(true);
  }

  showReply() {
    console.log('show replyyyyyyyyyy');
    this.showSpinner$.next(true);
    this.ClickShowReply$.next(true);
    if (this.comment.Replies.length > 0) {
      this.showEnterComment$.next(true);
    }
    this.showSpinner$.next(false);
  }

  ngOnDestroy(): void {
  }

  @ViewChild('videoPlayer') videoplayer: ElementRef;
  toggleVideo() {
    this.videoplayer.nativeElement.play();
  }
}