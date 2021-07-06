import { Observable, of } from 'rxjs';
import { Component, Input, OnInit, ViewEncapsulation } from '@angular/core';


@Component({
  selector: 'jeecomment-reaction-content',
  templateUrl: 'reaction-comment-content.component.html',
  styleUrls: ['reaction-comment-content.scss'],
  encapsulation: ViewEncapsulation.None
})

export class JeeCommentReactionContentComponent implements OnInit {

  @Input() objectID?: string;
  @Input() commentID?: string;
  @Input() replyCommentID?: string;
  @Input() isEdit?: boolean = false;
  
  constructor() { }

  ngOnInit() { }

  postReaction(value) {

  }
}