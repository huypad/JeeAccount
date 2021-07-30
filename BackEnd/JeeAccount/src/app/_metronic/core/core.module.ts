import { NgModule } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FirstLetterPipe } from './pipes/first-letter.pipe';
import { SafePipe } from './pipes/safe.pipe';
import { JeeChatOffcanvasDirective } from './directives/jee-chat-offcanvas.directive';

@NgModule({
  declarations: [FirstLetterPipe, SafePipe, JeeChatOffcanvasDirective],
  imports: [CommonModule],
  exports: [FirstLetterPipe, SafePipe],
})
export class CoreModule {}
