import { Directive, ElementRef, OnDestroy, AfterViewInit } from '@angular/core';

@Directive({
	selector: '[mJeeChatOffcanvas]'
})
export class JeeChatOffcanvasDirective
	implements AfterViewInit, OnDestroy {
	constructor(private el: ElementRef) {}

	ngAfterViewInit(): void {
		const offcanvas = new mOffcanvas(this.el.nativeElement, {
			overlay: true,
			baseClass: 'm-quick-sidebar',
			closeBy: 'kt_messenger_close',
			toggleBy: 'kt_messenger_toggle'
		});
	}
	ngOnDestroy(): void {}
}
