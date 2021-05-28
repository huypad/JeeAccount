import { ChangeDetectionStrategy, Component, OnInit } from '@angular/core';

@Component({
    selector: 'app-department-management',
    templateUrl: './department-management.component.html',
    changeDetection: ChangeDetectionStrategy.OnPush,
})
export class DepartmentManagementComponent implements OnInit {
    constructor() {}

    ngOnInit(): void {}
}
