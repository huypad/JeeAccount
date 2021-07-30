import { Injectable } from '@angular/core';
import { Observable, of } from 'rxjs';
import { AuthService } from 'src/app/modules/auth';
import * as io from 'socket.io-client';
import { HttpClient, HttpHeaders, HttpParams } from '@angular/common/http';
import { environment } from 'src/environments/environment';
import { catchError, map, tap } from 'rxjs/operators';

@Injectable()
export class SocketioService {
  socket: any;
  constructor(private auth: AuthService, private http: HttpClient) {}

  connect() {
    const auth = this.auth.getAuthFromLocalStorage();
    this.socket = io(environment.HOST_WEBSOCKET + '/notification', {
      transportOptions: {
        polling: {
          extraHeaders: {
            'x-auth-token': `${auth != null ? auth.access_token : ''}`,
          },
        },
      },
    });
  }

  listen() {
    return new Observable((subscriber) => {
      this.socket.on('notification', (data) => {
        subscriber.next(data);
      });
    });
  }

  getNotificationList(isRead: any): Observable<any> {
    const auth = this.auth.getAuthFromLocalStorage();
    const httpHeader = new HttpHeaders({
      Authorization: `${auth != null ? auth.access_token : ''}`,
    });
    const httpParam = new HttpParams().set('status', isRead);
    return this.http.get<any>(environment.HOST_NOTIFICATION + '/pull', {
      headers: httpHeader,
      params: httpParam,
    });
  }

  readNotification(id: string): Observable<any> {
    const auth = this.auth.getAuthFromLocalStorage();
    const httpHeader = new HttpHeaders({
      Authorization: `${auth != null ? auth.access_token : ''}`,
    });
    let item = { id: id };
    return this.http.post<any>(environment.HOST_NOTIFICATION + '/read', item, { headers: httpHeader });
  }

  getListApp(): Observable<any> {
    const auth = this.auth.getAuthFromLocalStorage();
    const httpHeader = new HttpHeaders({
      Authorization: `Bearer ${auth != null ? auth.access_token : ''}`,
    });
    //const httpParam = new HttpParams().set('userID', this.auth.getUserId())
    return this.http
      .get<any>(environment.HOST_JEEACCOUNT_API + '/api/accountmanagement/GetListAppByUserID', {
        headers: httpHeader,
      })
      .pipe(
        catchError((err) => {
          console.error(err);
          return of({ items: [], total: 0 });
        })
      );
  }
}
