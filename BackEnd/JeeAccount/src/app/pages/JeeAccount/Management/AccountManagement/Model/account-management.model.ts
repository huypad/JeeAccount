export class AccountManagementDTO {
  UserId: number;
  Username: string;
  FullName: string;
  AvartarImgURL: string;
  CustomerID: number;
  PhoneNumber: string;
  Email: string;
  re;
  Jobtitle: string;
  JobtitleID: number;
  NgaySinh: string;
  BgColor: string;
  FirstName: string;
  LastName: string;
  Department: string;
  DepartmentID: number;
  ChucVuID: string;
  StructureID: string;
  DirectManager: string;
  DirectManagerUserID: number;
  DirectManagerUsername: string;
  IsAdmin: boolean;
  IsActive: boolean;
  Note: string;
}

export interface AppListDTO {
  AppID: number;
  AppCode: string;
  AppName: string;
  Description: string;
  BackendURL: string;
  APIUrl: string;
  ReleaseDate: string;
  Note: string;
  CurrentVersion: string;
  LastUpdate: string;
  IsDefaultApp: boolean;
  IsAdmin: boolean;
  StartDate: string;
  EndDate: string;
}

export interface CheckEditAppListByDTO {
  AppID: number;
  AppCode: string;
  AppName: string;
  IsUsed: boolean;
  Disable: boolean;
}
export class AccountManagementModel {
  Fullname: string;
  ImageAvatar: string;
  Email: string;
  Username: string;
  Jobtitle: string;
  Departmemt: string;
  Phonemumber: string;
  Password: string;
  Birthday: string;
  AppCode: string[];
  AppID: number[];
  DepartmemtID: number;
  JobtitleID: number;
  constructor() {
    this.Fullname = '';
    this.ImageAvatar = '';
    this.Email = '';
    this.Username = '';
    this.Jobtitle = '';
    this.JobtitleID = 0;
    this.DepartmemtID = 0;
    this.Departmemt = '';
    this.Phonemumber = '';
    this.Password = '';
    this.AppCode = [];
    this.AppID = [];
    this.Birthday = '';
  }
}

export class PostImgModel {
  imgFile: string;
  Username: string;
}

export class AccDirectManagerModel {
  Username: string;
  DirectManager: string;

  clear() {
    this.Username = '';
    this.DirectManager = '';
  }
}

export class AccChangeTinhTrangModel {
  Username: string;
  Note: string;

  clear() {
    this.Username = '';
    this.Note = '';
  }
}

export class InfoUserDTO {
  Fullname: string;
  Name: string;
  Avatar: string;
  Jobtitle: string;
  Departmemt: string;
  Email: string;
  PhoneNumber: string;
  LastName: string;
  Username: string;

  clear() {
    this.Fullname = '';
    this.Name = '';
    this.Avatar = '';
    this.Jobtitle = '';
    this.Departmemt = '';
    this.Email = '';
    this.PhoneNumber = '';
    this.LastName = '';
    this.Username = '';
  }
}

export interface JeeHRNhanVien {
  HoTen: string;
  NgaySinh: string;
  Structure: string;
  Phai: string;
  TenChucVu: string;
  MaNV: string;
  ID_NV: number;
  Email: string;
  TuNgay: string;
  NgayBatDauLamViec: Date;
  IDNV: number;
  Title: string;
  cmnd: string;
  avatar: string;
  username: string;
  structureid: number;
  jobtitleid: number;
  managerid: number;
  manager: string;
}
