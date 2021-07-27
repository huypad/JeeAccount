export class AccountManagementDTO {
  fullname: string;
  name: string;
  avatar: string;
  jobtitle: string;
  departmemt: string;
  directManager: string;
  isActive: boolean;
  note: string;
  username: string;
  email: string;
  structureid: string;
  bgcolor: string;
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
  AppCode: string[];
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
