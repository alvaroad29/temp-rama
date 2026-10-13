export interface BookListQueryParameters {
  // Paginación
  PageNumber?: number;
  PageSize?: number;

  // Filtros
  SearchText?: string;
  GenreIds?: number[];

  // Ordenamiento
  SortBy?: SortByBookList;
  SortDescending?: boolean;
}



export enum SortByBookList {
  Title = 'title',
  FollowersCount = 'followercount',
  CreationDate = 'creationdate',
  UpdateDate = 'updatedate',
  BookCount = 'bookcount'
}
