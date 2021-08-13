export class JobtitleModel {
  public RowID: number;
  public JobtitleName: string;
  public Note: string;
  public Fullname: string;
  public Description: string;
  public ThanhVien: string[];
  public ThanhVienDelete: string[];
  clear() {
    this.RowID = 0;
    this.JobtitleName = '';
    this.Note = '';
    this.Fullname = '';
    this.Description = '';
    this.ThanhVien = [];
    this.ThanhVienDelete = [];
  }
}

export class JobChangeTinhTrangModel {
  RowID: number;
  Note: string;

  clear() {
    this.RowID = 0;
    this.Note = '';
  }
}

export interface JobtitleManagementDTO {
  Title: string;
  IsActive: boolean;
  Note: string;
  RowID: number;
}
