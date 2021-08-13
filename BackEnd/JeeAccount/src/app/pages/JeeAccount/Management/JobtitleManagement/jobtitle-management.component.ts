import { ChangeDetectionStrategy, Component, OnInit } from '@angular/core';

@Component({
  selector: 'app-jobtitle-management',
  templateUrl: './jobtitle-management.component.html',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class JobtitleManagementComponent implements OnInit {
  constructor() {}

  ngOnInit(): void {}
}
