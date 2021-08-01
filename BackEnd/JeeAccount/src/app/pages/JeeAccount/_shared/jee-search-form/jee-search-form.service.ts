import { HttpClient } from '@angular/common/http';
import { Observable, Subscription } from 'rxjs';
import { Inject, Injectable, OnDestroy } from '@angular/core';
import { HttpUtilsService } from '../../_core/utils/http-utils.service';

@Injectable()
export class JeeSearchFormService implements OnDestroy {
  private _subscriptions: Subscription[] = [];

  constructor(private http: HttpClient, private httpUtils: HttpUtilsService) {}

  ngOnDestroy(): void {
    this._subscriptions.forEach((sb) => sb.unsubscribe());
  }
}
