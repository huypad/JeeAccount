import { BehaviorSubject, Observable, Observer } from 'rxjs';
import { ChangeDetectionStrategy, Component, Inject, OnInit, ViewChild } from '@angular/core';
import { FormControl } from '@angular/forms';
import { MatDialogRef, MAT_DIALOG_DATA } from '@angular/material/dialog';
import { JeeCommentService } from './jee-comment.service';
import { CdkTextareaAutosize } from '@angular/cdk/text-field';

@Component({
  selector: 'app-jee-comment',
  templateUrl: './jee-comment.component.html',
  styleUrls: ['jee-comment.scss'],
})
export class JeeCommentComponent implements OnInit {
  hiddenLike: boolean = false;
  hiddenShare: boolean = false;
  isDone$: BehaviorSubject<boolean> = new BehaviorSubject<boolean>(false);
  input: string = 'hello tôi chính là nhà tiên tri vũ trụ trần dần hello tôi chính là nhà tiên tri vũ trụ trần dần hello tôi chính là nhà tiên tri vũ trụ trần dần hello tôi chính là nhà tiên tri vũ trụ trần dần hello tôi chính là nhà tiên tri vũ trụ trần dần hello tôi chính là nhà tiên tri vũ trụ trần dần hello tôi chính là nhà tiên tri vũ trụ trần dần';
  @ViewChild('autosize') autosize: CdkTextareaAutosize;

  constructor(public service: JeeCommentService) { }

  ngOnInit() {
  }


  clickButtonComment() {

  }

}
