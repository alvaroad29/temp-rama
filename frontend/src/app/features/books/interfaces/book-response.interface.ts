export interface BooksResponse {
  pageNumber:   number;
  pageSize:     number;
  totalPages:   number;
  totalRecords: number;
  data:         Book[];
}

export interface Book {
  id:           number;
  title:        string;
  creationDate: Date;
  updateDate:   Date;
  score:        number;
  description:  string;
  chapterCount: number;
  viewCount:    number;
  state:        State;
  imageUrl:     string;
  genres:       Genre[];
  author:       Author;
  isActive: boolean;
}

export enum State {
  Activo = 1,
  Completado = 2,
  Pausado = 3,
  All = 4
}

export interface Author {
  id:          string;
  displayName: string;
}



export interface Genre {
  id: number;
  name: string;
  isActive?: boolean;
}
