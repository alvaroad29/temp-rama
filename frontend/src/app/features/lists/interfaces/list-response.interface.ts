import { Genre } from "../../books/interfaces/book-response.interface";

export interface ListsResponse {
  pageNumber:   number;
  pageSize:     number;
  totalPages:   number;
  totalRecords: number;
  data:         List[];
}

export interface List {
  id:             number;
  title:          string;
  imageUrl?:       string;
  description?:    string;
  creationDate:   Date;
  updateDate?:     Date;
  genres?:         Genre[];
  bookCount:      number;
  isPublic:       boolean;
  followersCount: number;
  creatorId:      string;
  creatorName:    string;
}
