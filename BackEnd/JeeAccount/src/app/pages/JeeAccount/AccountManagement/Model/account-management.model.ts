export class AccountManagementDTO {
  fullname: string;
  name: string;
  avatar: string;
  jobtitle: string;
  departmemt: string;
  directManager: string;
  isActive: boolean;
  note: string;
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
