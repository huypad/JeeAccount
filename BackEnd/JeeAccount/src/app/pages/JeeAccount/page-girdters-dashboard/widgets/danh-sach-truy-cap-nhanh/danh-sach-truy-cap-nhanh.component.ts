import { Component, OnInit, ChangeDetectorRef, Input, EventEmitter, OnDestroy } from '@angular/core';
// Material
// RXJS
import { Subscription, Subject, of, Observable, BehaviorSubject } from 'rxjs';

@Component({
  selector: 'm-danh-sach-truy-cap-nhanh-widget',
  templateUrl: './danh-sach-truy-cap-nhanh.component.html',
})
export class DanhSachTruyCapNhanhWidgetComponent implements OnInit {

  listTruyCap: any;
  topicObjectID$: BehaviorSubject<string> = new BehaviorSubject<string>('');
  private readonly componentName = 'm-danh-sach-truy-cap-nhanh-widget';
  constructor() { }

  ngOnInit() {

  }


}
