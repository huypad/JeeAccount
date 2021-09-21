import { ChangeDetectionStrategy, Component, EventEmitter, Input, OnInit, Output } from '@angular/core';

@Component({
  selector: 'tag-comment-show',
  templateUrl: 'tag-comment-show.component.html',
  styleUrls: ['tag-comment-show.component.scss'],
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class TagCommentShowComponent implements OnInit {
  @Input() data: any[];

  @Output() ItemSelected = new EventEmitter<any>();

  listUser: any[] = [];

  constructor() {}

  ngOnInit() {
    this.listUser = this.data;
  }

  select(user) {
    this.ItemSelected.emit(user);
  }
}
