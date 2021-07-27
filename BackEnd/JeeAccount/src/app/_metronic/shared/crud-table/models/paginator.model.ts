export const PageSizes = [10, 50, 100];
export interface IPaginatorState {
  page: number;
  pageSize: number;
  total: number;
  recalculatePaginator(total: number): IPaginatorState;
}

export class PaginatorState implements IPaginatorState {
  page = 1;
  pageSize = PageSizes[0];
  total = 0;
  pageSizes: number[] = [];
  totalpage = 0;
  recalculatePaginator(total: number): PaginatorState {
    this.total = total;
    this.totalpage = total;
    return this;
  }
}

export interface IPaginatorView {
  paginator: PaginatorState;
  ngOnInit(): void;
  paginate(paginator: PaginatorState): void;
}
