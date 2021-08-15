import { Injectable } from '@angular/core';
import { Observable, of } from 'rxjs';
import { AuthService } from 'src/app/modules/auth';
import * as io from 'socket.io-client';
import { HttpClient, HttpHeaders, HttpParams } from '@angular/common/http';
import { environment } from 'src/environments/environment';
import { catchError } from 'rxjs/operators';

@Injectable()
export class SocketioService {
  socket: any;
  constructor(private auth: AuthService, private http: HttpClient) {}

  connect() {
    const auth = this.auth.getAuthFromLocalStorage();
    this.socket = io(environment.HOST_SOCKET + '/notification', {
      transportOptions: {
        polling: {
          extraHeaders: {
            'x-auth-token': `${auth != null ? auth.access_token : ''}`,
          },
        },
      },
    });
    this.socket.on('connect', (data) => {
      console.log('connected: ', data);
    });
    this.socket.on('disconnect', (data) => {
      console.log('disconnected: ', data);
    });
    const username = auth.user.username;
    const host = {
      portal: 'https://portal.jee.vn',
    };
    // Thiết lập iframe đến trang đăng ký
    const iframeSource = `${host.portal}/?getstatus=true`;
    const iframe = document.createElement('iframe');
    iframe.setAttribute('src', iframeSource);
    iframe.style.display = 'none';
    document.body.appendChild(iframe);

    // Thiết lập Event Listener để xác nhận người dùng đăng ký chưa
    window.addEventListener(
      'message',
      (event) => {
        if (event.origin !== host.portal) return; // Quan trọng, bảo mật, nếu không phải message từ portal thì ko làm gì cả, tránh XSS attack
        // event.data = false là user chưa đăng ký nhận thông báo, nếu đăng ký rồi thì là true
        if (event.data === false) {
          // Đoạn setTimeout này chỉ là 1 ví dụ -> Nếu người dùng vào trang mà chưa đăng ký thì 2s sau sẽ hiện popup cho người dùng đăng ký
          // Có thể tùy chỉnh đoạn này, thêm vào cookie, popup, button,... để tự chủ động trong việc đăng ký
          setTimeout(() => {
            // Lệnh window.open này chính là lệnh gọi mở popup đến trang đăng ký
            // Trang này vừa có thể đăng ký, vừa có thể hủy đăng ký
            // Có thể sử dụng lệnh này gán vào 1 nút nào đó trên trang cho người dùng chủ động trong việc đăng ký hoặc hủy đăng ký
            window.open(
              `${host.portal}/notificationsubscribe?username=${username}`, // username điền vào đây
              'childWin',
              'width=400,height=400'
            );
          }, 2000);
        }
      },
      false
    );
  }
  listen() {
    return new Observable((subscriber) => {
      this.socket.on('notification', (data) => {
        subscriber.next(data);
        console.log('Thông báo', data);
      });
    });
  }

  getNotificationList(isRead: any): Observable<any> {
    const auth = this.auth.getAuthFromLocalStorage();
    const httpHeader = new HttpHeaders({
      Authorization: `${auth != null ? auth.access_token : ''}`,
    });
    const httpParam = new HttpParams().set('status', isRead);
    return this.http.get<any>(environment.HOST_NOTIFICATION + '/notification/pull', {
      headers: httpHeader,
      params: httpParam,
    });
  }
readNotification(id: string): Observable<any> {
    const auth = this.auth.getAuthFromLocalStorage();
    const httpHeader = new HttpHeaders({
      Authorization: `${auth!=null ? auth.access_token : ''}`,
    });
    let item = {
      "id":  id
    }
		return this.http.post<any>(environment.HOST_NOTIFICATION+'/notification/read', item, { headers: httpHeader });
	}

  getListApp(): Observable<any> {
		const auth = this.auth.getAuthFromLocalStorage();
		const httpHeader = new HttpHeaders({
		  Authorization: `Bearer ${auth!=null ? auth.access_token : ''}`,
		});
		const httpParam = new HttpParams().set('userID', this.auth.getUserId())
		return this.http.get<any>(environment.HOST_JEEACCOUNT_API+'/api/accountmanagement/GetListAppByUserID', {
				headers: httpHeader,
				params: httpParam
			});
	}

  ReadAll(): Observable<any> {
    const auth = this.auth.getAuthFromLocalStorage();
    const httpHeader = new HttpHeaders({
      Authorization: `${auth != null ? auth.access_token : ''}`,
    });
    let item = { };
    return this.http.post<any>(environment.HOST_NOTIFICATION + `/notification/readall`, item, { headers: httpHeader });
  }
}
