import { Genre, State } from "../../books/interfaces/book-response.interface";

export interface BooksListDetailedResponse {
  books:          BooksListResponse;
  id:             number;
  title:          string;
  description:    string;
  imageUrl:       null;
  creationDate:   Date;
  updateDate:     Date;
  bookCount:      number;
  followersCount: number;
  isPublic:       boolean;
  creatorId:      string;
  creatorName:    string;
  genres:         Genre[];
  isFollowing:    boolean;
}

export interface BooksListResponse {
  pageNumber:   number;
  pageSize:     number;
  totalPages:   number;
  totalRecords: number;
  data:         BookList[];
}

export interface BookList { //libro
  id:        number;
  title:     string;
  imageUrl:  string;
  description:  string;
  score:     number;
  addedDate: Date;
  order:     number;
  state:     State;
}
