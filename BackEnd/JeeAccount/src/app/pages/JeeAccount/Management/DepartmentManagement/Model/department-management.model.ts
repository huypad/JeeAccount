export class DepartmentModel {
  public RowID: number;
  public DepartmentName: string;
  public DepartmentManager: string;
  public ThanhVien: string[];
  public ThanhVienDelete: string[];
  public Note: string;
  public Description: string;
  public Fullname: string;
  clear() {
    this.RowID = 0;
    this.DepartmentManager = '';
    this.DepartmentName = '';
    this.ThanhVien = [];
    this.ThanhVienDelete = [];
    this.Note = '';
    this.Fullname = '';
    this.Description = '';
  }
}

export class DepChangeTinhTrangModel {
  RowID: number;
  Note: string;

  clear() {
    this.RowID = 0;
    this.Note = '';
  }
}

export interface DepartmentManagementDTO {
  DepartmentManager: string;
  DepartmentManagerUserID: string;
  DepartmentManagerUsername: string;
  DepartmentName: string;
  IsActive: boolean;
  Note: string;
  RowID: number;
  Description: string;
}

export interface TreeJeeHRDepartmentDTO {
  RowID: number;
  Title: string;
  Level: string;
  ParentID: string;
  Position: string;
  Children?: TreeJeeHRDepartmentDTO[];
}

export interface FlatJeeHRDepartmentDTO {
  RowID: number;
  Title: string;
}

export interface DepartmentManagement {
  flat: any[];
  isJeeHR: boolean;
  isTree: boolean;
  tree: TreeJeeHRDepartmentDTO[];
}

export class DepDirectManagerModel {
  RowID: number;
  DepartmentManager: string;

  clear() {
    this.RowID = 0;
    this.DepartmentManager = '';
  }
}
