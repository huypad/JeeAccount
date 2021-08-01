import { MatExpansionModule } from '@angular/material/expansion';
import { TranslateModule } from '@ngx-translate/core';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatIconModule } from '@angular/material/icon';
import { NgModule } from '@angular/core';
import { CommonModule } from '@angular/common';
import { MatChipsModule } from '@angular/material/chips';
import { NgxMatSelectSearchModule } from 'ngx-mat-select-search';
import { InlineSVGModule } from 'ng-inline-svg';
import { MatTooltipModule } from '@angular/material/tooltip';
import { FormsModule, ReactiveFormsModule } from '@angular/forms';
import { PickerModule } from '@ctrl/ngx-emoji-mart';
import { JeeSearchFormService } from './jee-search-form.service';
import { JeeSearchFormComponent } from './jee-search-form.component';

@NgModule({
  declarations: [JeeSearchFormComponent],
  imports: [
    CommonModule,
    MatChipsModule,
    NgxMatSelectSearchModule,
    InlineSVGModule,
    MatIconModule,
    MatInputModule,
    MatFormFieldModule,
    MatTooltipModule,
    FormsModule,
    PickerModule,
    TranslateModule,
    ReactiveFormsModule,
    MatExpansionModule,
  ],
  providers: [JeeSearchFormService],
  entryComponents: [JeeSearchFormComponent],
  exports: [JeeSearchFormComponent],
})
export class JeeSearchFormModule {}
