import { HttpUtilsService } from './../../_core/utils/http-utils.service';
import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable, forkJoin, BehaviorSubject, of } from 'rxjs';
import { environment } from '../../../../../environments/environment';

const API_PRODUCTS_URL = environment.HOST_JEEACCOUNT_API + '/api/';
@Injectable()
export class JeeCommentService {

  constructor(private http: HttpClient, private httpUtils: HttpUtilsService) { }



}
