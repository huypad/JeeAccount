import { AuthService } from 'src/app/modules/auth';
import { AccountManagementService } from './../Management/AccountManagement/Services/account-management.service';
import { ChangeDetectionStrategy, ChangeDetectorRef, Component, Inject, OnInit, OnDestroy, HostListener } from '@angular/core';
import { FormBuilder, FormControl, FormGroup } from '@angular/forms';
import { BehaviorSubject, of, Subscription } from 'rxjs';
import { DatePipe } from '@angular/common';
import { TranslateService } from '@ngx-translate/core';
import { LayoutUtilsService, MessageType } from '../_core/utils/layout-utils.service';
import { DanhMucChungService } from '../_core/services/danhmuc.service';
import { catchError, tap } from 'rxjs/operators';
import { PostImgModel } from '../Management/AccountManagement/Model/account-management.model';

@Component({
  selector: 'app-info-account-list',
  templateUrl: './info-account-list.component.html',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class InfoAccountListComponent implements OnInit, OnDestroy {
  private _isLoading$ = new BehaviorSubject<boolean>(false);
  private _isFirstLoading$ = new BehaviorSubject<boolean>(true);
  private _errorMessage$ = new BehaviorSubject<string>('');
  private subscriptions: Subscription[] = [];
  public isLoadingSubmit$: BehaviorSubject<boolean> = new BehaviorSubject<boolean>(false);
  personalInfo: any;
  imgFile: string = '';
  userid: number;
  username: string;
  get isLoading$() {
    return this._isLoading$.asObservable();
  }
  get isFirstLoading$() {
    return this._isFirstLoading$.asObservable();
  }
  get errorMessage$() {
    return this.errorMessage$.asObservable();
  }
  // End
  constructor(
    private fb: FormBuilder,
    public accountManagementService: AccountManagementService,
    private changeDetect: ChangeDetectorRef,
    private layoutUtilsService: LayoutUtilsService,
    public danhmuc: DanhMucChungService,
    public datepipe: DatePipe,
    private translate: TranslateService,
    private auth: AuthService
  ) {}

  ngOnDestroy(): void {
    this.accountManagementService.ngOnDestroy();
    this.subscriptions.forEach((sb) => sb.unsubscribe());
  }

  ngOnInit(): void {
    const user = this.auth.getAuthFromLocalStorage();
    this.personalInfo = user['user']['customData'].personalInfo;
    this.userid = user['user']['customData']['jee-account'].userID;
    this.username = user['user'].username;

    console.log(user);
  }

  onFileChange(event, username: string) {
    let saveMessageTranslateParam = '';
    saveMessageTranslateParam += 'Thêm thành công';
    const saveMessage = this.translate.instant(saveMessageTranslateParam);
    const messageType = MessageType.Create;
    if (event.target.files && event.target.files[0]) {
      const reader = new FileReader();
      reader.onload = (e: any) => {
        this.imgFile = e.target.result.split(',')[1];
        const postimg = new PostImgModel();
        postimg.imgFile = this.imgFile;
        postimg.Username = username;
        const sb = this.accountManagementService
          .UpdateAvatarWithChangeUrlAvatar(postimg)
          .pipe(
            tap((res) => {
              this.layoutUtilsService.showActionNotification(saveMessage, messageType, 4000, true, false);
              this.imgFile = '';
              this.accountManagementService.fetch();
            }),
            catchError((err) => {
              console.log(err);
              this.layoutUtilsService.showActionNotification(err.error.message, MessageType.Read, 999999999, true, false, 3000, 'top', 0);
              this.imgFile = '';
              return of();
            })
          )
          .subscribe();
        this.subscriptions.push(sb);
      };
      reader.readAsDataURL(event.target.files[0]);
    }
  }
  onSubmit(withBack: boolean) {}
}
