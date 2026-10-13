import { Book } from "./book-response.interface";

export interface BookPopularResponse {
  daily:   Book[];
  weekly:  Book[];
  monthly: Book[];
}
