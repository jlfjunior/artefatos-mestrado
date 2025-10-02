export interface Paged<T> {
  totalItems: number;
  totalPages: number;
  page: number;
  pageSize: number;
  items: T[];
}