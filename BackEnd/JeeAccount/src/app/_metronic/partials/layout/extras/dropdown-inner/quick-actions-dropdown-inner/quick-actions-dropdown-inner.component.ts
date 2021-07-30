import { Component, OnInit } from '@angular/core';
import { SocketioService } from 'src/app/pages/JeeAccount/_core/services/socketio.service';
import { LayoutService } from '../../../../../core';

@Component({
  selector: 'app-quick-actions-dropdown-inner',
  templateUrl: './quick-actions-dropdown-inner.component.html',
  styleUrls: ['./quick-actions-dropdown-inner.component.scss'],
})
export class QuickActionsDropdownInnerComponent implements OnInit {
  extrasQuickActionsDropdownStyle: 'light' | 'dark' = 'light';
  constructor(private layout: LayoutService, private socketService: SocketioService) {}
  listApp: any = [];

  ngOnInit(): void {
    this.extrasQuickActionsDropdownStyle = this.layout.getProp('extras.quickActions.dropdown.style');

    this.socketService.getListApp().subscribe((res) => {
      if (res.status == 1) {
        this.listApp = res.data;
      }
    });
  }
}
