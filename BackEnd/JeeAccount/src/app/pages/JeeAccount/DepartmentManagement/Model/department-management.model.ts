export class DepartmentModel {
  public RowID: number;
  public DepartmentName: string;
  public DepartmentManager: string;
  public ThanhVien: string[];
  public Note: string;
  public  Fullname: string;
  clear() {
    this.RowID = 0;
    this.DepartmentManager = '';
    this.DepartmentName = '';
    this.ThanhVien = [];
    this.Note = '';
    this.Fullname = '';
  }
}
