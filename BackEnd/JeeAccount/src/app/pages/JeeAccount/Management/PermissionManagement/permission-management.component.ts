import { ChangeDetectionStrategy, Component, OnInit } from '@angular/core';

@Component({
  selector: 'app-permission-management',
  templateUrl: './permission-management.component.html',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class PermissionManagementComponent implements OnInit {
  constructor() {}

  ngOnInit(): void {}
}
