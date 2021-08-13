export class showSearchFormModel {
  username: boolean;
  tennhanvien: boolean;
  phongban: boolean;
  chucvu: boolean;
  phongbanid: boolean;
  chucvuid: boolean;
  isAdmin: boolean;
  dakhoa: boolean;
  titlekeyword: string;
  constructor() {
    this.username = true;
    this.tennhanvien = true;
    this.phongban = true;
    this.chucvu = true;
    this.phongbanid = true;
    this.chucvuid = true;
    this.isAdmin = true;
    this.dakhoa = true;
    this.titlekeyword = 'SEARCH.SEARCH1';
  }
}

export interface SelectModel {
  RowID: number;
  Title: string;
}