
import { Component, Input, OnInit, ViewEncapsulation } from '@angular/core';
import { of, Subject } from 'rxjs';
import { catchError, takeUntil, tap } from 'rxjs/operators';
import { ReactionCommentModel } from '../jee-comment.model';
import { JeeCommentService } from '../jee-comment.service';

@Component({
  selector: 'jeecomment-reaction-content',
  templateUrl: 'reaction-comment-content.component.html',
  styleUrls: ['reaction-comment-content.scss'],
  encapsulation: ViewEncapsulation.None
})

export class JeeCommentReactionContentComponent implements OnInit {
  private readonly onDestroy = new Subject<void>();
  @Input() objectID: string = '';
  @Input() commentID: string = '';
  @Input() replyCommentID: string = '';
  @Input() userOldReaction?: string;

  userReaction: string = '';
  constructor(public service: JeeCommentService) { }

  ngOnInit() {
    if (!this.objectID) this.objectID = '';
    if (!this.commentID) this.commentID = '';
    if (!this.replyCommentID) this.replyCommentID = '';
    if (!this.userOldReaction) this.userOldReaction = '';
  }

  postReaction(react: string) {
    this.userReaction = react;
    const model = this.prepareModel();
    this.postReactionComment(model);
  }

  postReactionComment(model: ReactionCommentModel) {
    this.service.postReactionCommentModel(model).pipe(
      tap(
        (res) => { },
        catchError((err) => { console.log(err); return of() }),
      ),
      takeUntil(this.onDestroy),
    ).subscribe();
  }

  prepareModel(): ReactionCommentModel {
    const model = new ReactionCommentModel();
    model.TopicCommentID = this.objectID;
    model.CommentID = this.commentID;
    model.ReplyCommentID = this.replyCommentID;
    model.UserReaction = this.userReaction;
    model.UserOldReaction = this.userOldReaction;
    return model;
  }

  ngOnDestroy(): void {
    this.onDestroy.next();
  }
}