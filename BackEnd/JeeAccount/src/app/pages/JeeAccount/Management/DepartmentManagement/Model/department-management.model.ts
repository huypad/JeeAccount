export class DepartmentModel {
  public RowID: number;
  public DepartmentName: string;
  public DepartmentManager: string;
  public ThanhVien: string[];
  public Note: string;
  public Fullname: string;
  clear() {
    this.RowID = 0;
    this.DepartmentManager = '';
    this.DepartmentName = '';
    this.ThanhVien = [];
    this.Note = '';
    this.Fullname = '';
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
}

export interface TreeJeeHRDepartmentDTO {
  RowID: number;
  Title: string;
  Level: number;
  ParentID: number;
  Position: number;
  Children: TreeJeeHRDepartmentDTO[];
}

export interface FlatJeeHRDepartmentDTO {
  RowID: number;
  Title: string;
}

export interface DepartmentManagement {
  flat: any[];
  isJeeHR: boolean;
  isTree: boolean;
  tree: TreeJeeHRDepartmentDTO;
}
