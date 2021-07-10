import { HttpUtilsService } from './../../_core/utils/http-utils.service';
import { Injectable } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Observable, forkJoin, BehaviorSubject, of, Subscription } from 'rxjs';
import { environment } from '../../../../../environments/environment';
import { catchError, finalize, share, take, takeUntil, tap } from 'rxjs/operators';
import { AuthService } from 'src/app/modules/auth';
import { QueryFilterComment, UserCommentInfo, PostCommentModel, ReactionCommentModel } from './jee-comment.model';

const API_JEECOMMENT_URL = environment.HOST_JEECOMMENT_API + '/api';
const API_JEEACCOUNT_URL = environment.HOST_JEEACCOUNT_API + '/api';

@Injectable()
export class JeeCommentService {
  private _isLoading$ = new BehaviorSubject<boolean>(false);
  private _errorMessage$ = new BehaviorSubject<string>('');
  public lstUser: UserCommentInfo[] = [];
  private _mainUser$: BehaviorSubject<UserCommentInfo> = new BehaviorSubject<UserCommentInfo>(undefined);
  public filterLstCommentID = new BehaviorSubject<string[]>([]);
  public filterDate = new BehaviorSubject<Date>(new Date());

  get isLoading$() {
    return this._isLoading$.asObservable();
  }

  get mainUser$() {
    return this._mainUser$.asObservable();
  }

  // tiếng việt icon
  public i18n = {
    search: 'Tìm kiếm',
    emojilist: 'Danh sách Emoji',
    notfound: 'Không tìm thấy Emoji ',
    clear: 'Xoá sạch',
    categories: {
      search: 'Kết quả',
      recent: 'Sử dụng thường xuyên',
      people: 'Biểu tượng mặt cười & Con người',
      nature: 'Động vật & Thiên nhiên',
      foods: 'Đồ ăn & Nước uống',
      activity: 'Hoạt động',
      places: 'Du lịch & Nghỉ dưỡng',
      objects: 'Đồ vật',
      symbols: 'Biểu tượng',
      flags: 'Cờ',
      custom: 'Custom',
    },
    skintones: {
      1: 'Default Skin Tone',
      2: 'Light Skin Tone',
      3: 'Medium-Light Skin Tone',
      4: 'Medium Skin Tone',
      5: 'Medium-Dark Skin Tone',
      6: 'Dark Skin Tone',
    }
  };

  constructor(private http: HttpClient, private httpUtils: HttpUtilsService, private _authService: AuthService) {
    this.getlstUserCommentInfo();
  }

  public showTopicCommentByObjectID(objectID: string, filter: QueryFilterComment): Observable<any> {
    const httpHeaders = this.httpUtils.getHTTPHeaders();
    const httpParams = this.getHttpParamsFilter(filter);
    const url = API_JEECOMMENT_URL + `/comments/show/${objectID}`;
    return this.http.get<any>(url, {
      headers: httpHeaders,
      params: httpParams
    });
  }

  public showChangeTopicCommentByObjectID(objectID: string, filter: QueryFilterComment): Observable<any> {
    const httpHeaders = this.httpUtils.getHTTPHeaders();
    const httpParams = this.getHttpParamsFilter(filter);
    const url = API_JEECOMMENT_URL + `/comments/showChange/${objectID}`;
    return this.http.get<any>(url, {
      headers: httpHeaders,
      params: httpParams
    });
  }

  public showChangeComment(objectID: string, commentID: string, filter: QueryFilterComment): Observable<any> {
    const httpHeaders = this.httpUtils.getHTTPHeaders();
    const httpParams = this.getHttpParamsFilter(filter);
    const url = API_JEECOMMENT_URL + `/comments/showChange/${objectID}/${commentID}`;
    return this.http.get<any>(url, {
      headers: httpHeaders,
      params: httpParams
    });
  }

  public showFullComment(objectID: string, commentID: string, filter: QueryFilterComment): Observable<any> {
    const httpHeaders = this.httpUtils.getHTTPHeaders();
    const httpParams = this.getHttpParamsFilter(filter);
    const url = API_JEECOMMENT_URL + `/comments/show/${objectID}/${commentID}`;
    return this.http.get<any>(url, {
      headers: httpHeaders,
      params: httpParams
    });
  }

  private getHttpParamsFilter(filter: QueryFilterComment): HttpParams {
    let query = new HttpParams()
      .set('ViewLengthComment', filter.ViewLengthComment.toString())
      .set('Date', filter.Date.toISOString())
    return query;
  }

  public getlstUserCommentInfo() {
    this.getDSUserCommentInfo()
      .pipe(
        tap((res) => {
          if (res && res.status == 1) {
            const usernamelogin = this._authService.getAuthFromLocalStorage()['user']['username'];
            res.data.forEach(element => {
              // init ListUserCommentInfo
              const item = new UserCommentInfo();
              item.Username = element.Username;
              item.AvartarImgURL = element.AvartarImgURL;
              item.PhoneNumber = element.PhoneNumber;
              item.Email = element.Email;
              item.Jobtitle = element.Jobtitle;
              item.FullName = element.FullName;
              item.Display = element.FullName ? element.FullName : element.Username;
              this.lstUser.push(item);

              // init main User Login
              if (usernamelogin === item.Username) this._mainUser$.next(item);
            });
          } else {
            this._errorMessage$.next(res.error.message);
            return of();
          }
        }),
        finalize(() => {
          this._isLoading$.next(false);
        }),
        share()
      ).
      subscribe();
  }

  private getDSUserCommentInfo(): Observable<any> {
    const httpHeaders = this.httpUtils.getHTTPHeaders();
    const url = API_JEEACCOUNT_URL + `/accountmanagement/usernamesByCustermerID`;
    return this.http.get<any>(url, {
      headers: httpHeaders,
    });
  }


  public getDisplay(username?: string): string {
    const object = this.lstUser.find(element => element.Username === username);
    if (object) return object.Display;
    return username;
  }
  public getUriAvatar(username?: string): string {
    const avatar = this.lstUser.find(element => element.Username === username);
    if (avatar) return avatar.AvartarImgURL;
    return 'https://cdn.jee.vn/jee-account/images/avatars/default2.png';
  }

  public postCommentModel(model: PostCommentModel): Observable<any> {
    const httpHeaders = this.httpUtils.getHTTPHeaders();
    const url = API_JEEACCOUNT_URL + `/comments/postcomment`;
    return this.http.post<any>(url, model, {
      headers: httpHeaders
    });
  }

  public postReactionCommentModel(model: ReactionCommentModel): Observable<any> {
    const httpHeaders = this.httpUtils.getHTTPHeaders();
    const url = API_JEEACCOUNT_URL + `/comments/postReactionComment`;
    return this.http.post<any>(url, model, {
      headers: httpHeaders
    });
  }
}
