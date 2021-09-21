import { CommentDTO, UserCommentInfo } from './../jee-comment.model';
import { catchError, takeUntil, tap } from 'rxjs/operators';
import { of, Subject, BehaviorSubject } from 'rxjs';
import {
  ChangeDetectionStrategy,
  Component,
  Input,
  OnInit,
  ViewEncapsulation,
  ChangeDetectorRef,
  ElementRef,
  ViewChild,
  AfterViewInit,
  EventEmitter,
  Output,
} from '@angular/core';
import { JeeCommentService } from '../jee-comment.service';
import { PostCommentModel } from '../jee-comment.model';
import { PopoverContentComponent } from 'ngx-smart-popover';

@Component({
  selector: 'jeecomment-enter-comment-content',
  templateUrl: 'enter-comment-content.component.html',
  styleUrls: ['enter-comment-content.scss', '../jee-comment.scss'],
  changeDetection: ChangeDetectionStrategy.OnPush,
  encapsulation: ViewEncapsulation.None,
})
export class JeeCommentEnterCommentContentComponent implements OnInit, AfterViewInit {
  private readonly onDestroy = new Subject<void>();
  constructor(public service: JeeCommentService, public cd: ChangeDetectorRef) {}

  @Input('objectID') objectID: string = '';
  @Input('commentID') commentID: string = '';
  @Input('replyCommentID') replyCommentID: string = '';

  @Input('isEdit') set editing(isEdit: boolean) {
    this.isEdit$.next(isEdit);
  }
  private isEdit$ = new BehaviorSubject<boolean>(false);
  @Input('isFocus$') isFocus$: BehaviorSubject<boolean> = new BehaviorSubject<boolean>(false);
  @Input('editCommentModel') commentModelDto?: CommentDTO;

  @Output('isEditEvent') isEditEvent = new EventEmitter<boolean>();

  showPopupEmoji: boolean;
  isClickIconEmoji: boolean;
  showSpanCancelFocus: boolean;
  showSpanCancelNoFocus: boolean;
  imagesUrl: string[];
  videosUrl: any[];
  filesUrl: any[];
  filesName: string[] = [];
  inputTextArea: string;
  reg = /@\w*(\.[A-Za-z]\w*)|\@[A-Za-z]\w*/gm;
  @ViewChild('myPopover') myPopover: PopoverContentComponent;
  selected: any[] = [];
  it: any = {};
  CommentTemp: string = '';
  index: number = -1;
  @ViewChild('txtarea') txtarea: ElementRef;
  listUser: UserCommentInfo[] = [];

  private _isLoading$: BehaviorSubject<boolean> = new BehaviorSubject<boolean>(false);
  get isLoading$() {
    return this._isLoading$.asObservable();
  }

  ngOnInit() {
    this.imagesUrl = [];
    this.videosUrl = [];
    this.filesUrl = [];
    this.inputTextArea = '';
    this.showPopupEmoji = false;
    this.isClickIconEmoji = false;
    this.showSpanCancelFocus = false;
    this.showSpanCancelNoFocus = false;
    this.listUser = this.service.lstUser;
  }

  getKeyword() {
    let i = this.CommentTemp.lastIndexOf('@');
    if (i >= 0) {
      let temp = this.CommentTemp.slice(i);
      if (temp.includes('  ')) {
        return '';
      }
      return this.CommentTemp.slice(i).replace('@', '');
    }
    return '';
  }

  getListTagUser(chuoi, array) {
    var arr = [];
    var user = [];
    chuoi.split(' ').forEach((element) => {
      if (element[0] == '@') {
        user.push(element);
      }
    });
    user = Array.from(new Set(user));
    user.forEach((element) => {
      var x = array.find((x) => x.username == element.substr(1));
      if (x) {
        arr.push(x);
      }
    });
    return arr;
  }

  ngAfterViewInit(): void {
    if (this.commentModelDto) {
      this.initData();
    }
    this.isFocus$
      .pipe(
        tap((res) => {
          if (res) {
            this.FocusTextarea();
          }
        }),
        takeUntil(this.onDestroy)
      )
      .subscribe();
  }

  initData() {
    this.inputTextArea = this.commentModelDto.Text;
    this.imagesUrl = this.commentModelDto.Attachs.Images;
    this.videosUrl = this.commentModelDto.Attachs.Videos;
    this.filesUrl = this.commentModelDto.Attachs.Files;
    this.commentModelDto.Attachs.Files.forEach((file) => {
      this.filesName.push(file.split('/')[file.split('/').length - 1]);
    });
    this.isEdit$
      .pipe(
        tap((res) => {
          this.isEditEvent.emit(res);
        }),
        takeUntil(this.onDestroy)
      )
      .subscribe();

    this.cd.detectChanges();
    this.isFocus$.next(true);
  }

  FocusTextarea() {
    this.txtarea.nativeElement.focus();
  }

  getContent($event) {
    console.log($event.target.innerHTML);
  }

  validateCommentAndPost() {
    console.log(this.txtarea.nativeElement);
    this.pasteHtmlAtCaret('<span class="text-normal">random</span>&#8203;', this.txtarea.nativeElement);
    // if (!this._isLoading$.value) {
    //   const model = this.prepareComment();
    //   if (this.checkCommentIsEqualEmpty(model)) {
    //     return;
    //   }
    //   if (this.isEdit$.value) {
    //     this.updateComment(model);
    //     this.isEdit$.next(false);
    //   } else {
    //     this.postComment(model);
    //   }
    // }
  }

  prepareComment(): PostCommentModel {
    const model = new PostCommentModel();
    model.TopicCommentID = this.objectID ? this.objectID : '';
    model.CommentID = this.commentID ? this.commentID : '';
    model.ReplyCommentID = this.replyCommentID ? this.replyCommentID : '';
    model.Text = this.inputTextArea;
    model.Attachs.FileNames = this.filesName;
    this.imagesUrl.forEach((imageUrl) => {
      const base64 = imageUrl.split(',')[1];
      model.Attachs.Images.push(base64);
    });
    this.videosUrl.forEach((videoURL) => {
      const base64 = videoURL.split(',')[1];
      model.Attachs.Videos.push(base64);
    });
    this.filesUrl.forEach((fileUrl) => {
      const base64 = fileUrl.split(',')[1];
      model.Attachs.Files.push(base64);
    });
    return model;
  }

  checkCommentIsEqualEmpty(model: PostCommentModel): boolean {
    const empty = new PostCommentModel();
    return this.isEqual(model, empty);
  }

  isEqual(object: PostCommentModel, otherObject: PostCommentModel): boolean {
    let checkValue = object.Text === otherObject.Text;
    let checkList = false;
    if (
      object.Attachs.Files.length === otherObject.Attachs.Files.length &&
      object.Attachs.Images.length === otherObject.Attachs.Images.length &&
      object.Attachs.Videos.length === otherObject.Attachs.Videos.length
    )
      checkList = true;
    if (checkValue && checkList) return true;
    return false;
  }

  updateComment(model: PostCommentModel) {
    this._isLoading$.next(true);
    this.service
      .updateCommentModel(model)
      .pipe(
        tap(
          (res) => {},
          catchError((err) => {
            console.log(err);
            return of();
          }),
          () => {
            this.ngOnInit();
            this.cd.detectChanges();
            setTimeout(() => {
              this._isLoading$.next(false);
            }, 750);
          }
        ),
        takeUntil(this.onDestroy)
      )
      .subscribe();
  }

  postComment(model: PostCommentModel) {
    this._isLoading$.next(true);
    this.service
      .postCommentModel(model)
      .pipe(
        tap(
          (res) => {},
          catchError((err) => {
            console.log(err);
            return of();
          }),
          () => {
            this.cancleComment();
            this.cd.detectChanges();
            setTimeout(() => {
              this._isLoading$.next(false);
            }, 750);
          }
        ),
        takeUntil(this.onDestroy)
      )
      .subscribe();
  }

  onKeydown($event) {
    let data = this.txtarea.nativeElement.innerHTML;

    var match = data.match(this.reg);

    if (match) {
    }
    console.log('match', match);

    //  check cancel keyword
    if (data)
      if (($event.ctrlKey && $event.keyCode == 13) || ($event.altKey && $event.keyCode == 13)) {
        this.inputTextArea = this.inputTextArea + '\n';
      } else if ($event.keyCode == 13) {
        $event.preventDefault();
      }
    this.focusFunction();
  }

  checkUser(text, value) {
    return text.search('@' + value.hoten);
  }

  toggleEmojiPicker() {
    this.showPopupEmoji = true;
    this.isClickIconEmoji = true;
  }

  addEmoji(event) {
    const data = this.txtarea.nativeElement.innerHTML + `${event.emoji.native}`;
    this.txtarea.nativeElement.innerHTML = data;
    this.showPopupEmoji = false;
  }

  previewFileInput(files: FileList) {
    const filesAmount = files.length;
    if (filesAmount > 0) this.showSpanCancelNoFocus = true;
    for (let i = 0; i < filesAmount; i++) {
      const name = files[i].name;
      if (this.isImage(name)) {
        this.addImage(files.item(i));
      } else if (this.isVideo(name)) {
        this.addVideo(files.item(i));
      } else {
        this.filesName.push(name);
        this.addFile(files.item(i));
      }
    }
  }

  addFile(item) {
    let reader = new FileReader();
    reader.readAsDataURL(item);
    reader.onload = () => {
      this.filesUrl.push(reader.result as string);
      this.cd.detectChanges();
    };
  }
  addImage(item) {
    let reader = new FileReader();
    reader.readAsDataURL(item);
    reader.onload = () => {
      this.imagesUrl.push(reader.result as string);
      this.cd.detectChanges();
    };
  }
  addVideo(item) {
    let reader = new FileReader();
    reader.readAsDataURL(item);
    reader.onload = () => {
      this.videosUrl.push(reader.result);
      this.cd.detectChanges();
    };
  }
  getExtension(filename: string) {
    var parts = filename.split('.');
    return parts[parts.length - 1];
  }

  isImage(filename: string) {
    const ext = this.getExtension(filename);
    switch (ext.toLowerCase()) {
      case 'jpg':
      case 'gif':
      case 'bmp':
      case 'png':
      case 'heic':
      case 'heif':
        return true;
    }
    return false;
  }

  isVideo(filename: string) {
    var ext = this.getExtension(filename);
    switch (ext.toLowerCase()) {
      case 'm4v':
      case 'avi':
      case 'mpg':
      case 'mp4':
      case 'ts':
      case 'mkv':
      case 'webm':
      case 'wmv':
      case '3gpp':
      case 'mpeg':
      case 'ogv':
        return true;
    }
    return false;
  }

  deletePreviewImage(index) {
    this.imagesUrl.splice(index, 1);
    this.cd.detectChanges();
  }

  deletePreviewVideo(index) {
    this.videosUrl.splice(index, 1);
    this.cd.detectChanges();
  }
  deletePreviewFile(index) {
    this.filesUrl.splice(index, 1);
    this.filesName.splice(index, 1);
    this.cd.detectChanges();
  }

  cancleComment() {
    this.inputTextArea = '';
    this.imagesUrl = [];
    this.videosUrl = [];
    this.filesUrl = [];
    this.filesName = [];
    this.showSpanCancelFocus = false;
    this.showSpanCancelNoFocus = false;
    this.isEdit$.next(false);
    this.cd.detectChanges();
  }

  focusFunction() {
    if (this.checkValueExistCommentModel()) {
      this.showSpanCancelFocus = true;
      this.showSpanCancelNoFocus = false;
    } else {
      this.showSpanCancelFocus = false;
    }
  }

  focusOutFunction() {
    if (this.checkValueExistCommentModel()) {
      this.showSpanCancelFocus = false;
      this.showSpanCancelNoFocus = true;
    } else {
      this.showSpanCancelNoFocus = false;
    }
  }

  checkValueExistCommentModel(): boolean {
    if (this.inputTextArea.length > 0 || this.imagesUrl.length > 1) {
      return true;
    }
    return false;
  }

  clickOutSideEmoji() {
    if (this.showPopupEmoji && this.isClickIconEmoji) {
      this.showPopupEmoji = true;
      this.isClickIconEmoji = false;
    } else {
      this.showPopupEmoji = false;
      this.isClickIconEmoji = false;
    }
  }

  ItemSelected(data) {
    this.selected.push(data);
    let i = this.CommentTemp.lastIndexOf('@');
    this.CommentTemp = this.CommentTemp.substr(0, i) + '@' + data.hoten + '  ';
    //this.myPopover.hide();
    let ele = <HTMLInputElement>this.txtarea.nativeElement;
    // if (this.index >= 0) {
    //   ele = <HTMLInputElement>document.getElementById('CommentRep' + this.index);
    // }
    ele.value = this.CommentTemp;
    this.inputTextArea = this.CommentTemp;
    ele.focus();
    this.cd.detectChanges();
  }

  setEndOfContenteditable(contentEditableElement) {
    var range, selection;
    range = document.createRange();
    range.selectNodeContents(contentEditableElement);
    range.collapse(false);
    selection = window.getSelection();
    selection.removeAllRanges();
    selection.addRange(range);
  }

  elementContainsSelection(el) {
    var sel;
    if (window.getSelection) {
      sel = window.getSelection();
      if (sel.rangeCount > 0) {
        for (var i = 0; i < sel.rangeCount; ++i) {
          if (!this.isOrContains(sel.getRangeAt(i).commonAncestorContainer, el)) {
            return false;
          }
        }
        return true;
      }
    } else if ((sel = document.getSelection()) && sel.type != 'Control') {
      return this.isOrContains(sel.createRange().parentElement(), el);
    }
    return false;
  }

  isOrContains(node, container) {
    while (node) {
      if (node === container) {
        return true;
      }
      node = node.parentNode;
    }
    return false;
  }

  pasteHtmlAtCaret(html, el) {
    var sel, range;
    if (window.getSelection) {
      sel = window.getSelection();
      if (this.elementContainsSelection(el)) {
        if (sel.getRangeAt && sel.rangeCount) {
          range = sel.getRangeAt(0);
          range.deleteContents();

          let el = document.createElement('div');
          el.innerHTML = html;
          var frag = document.createDocumentFragment(),
            node,
            lastNode;
          while ((node = el.firstChild)) {
            lastNode = frag.appendChild(node);
          }
          range.insertNode(frag);

          // Preserve the selection
          if (lastNode) {
            range = range.cloneRange();
            range.setStartAfter(lastNode);
            range.collapse(false);
            sel.removeAllRanges();
            sel.addRange(range);
          }
        }
      } else {
        this.setEndOfContenteditable(el);
        this.pasteHtmlAtCaret(html, el);
      }
    }
  }
}
