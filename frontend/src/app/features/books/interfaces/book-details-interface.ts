// Cambiar BookDetails para que coincida con la respuesta de la API
export interface BookDetails {
  id: number;
  title: string;
  creationDate: string;
  updateDate: string;
  score: number;
  totalRatings: number;
  description: string;
  chapterCount: number;
  viewCount: number;
  state: number;
  imageUrl: string;
  genres: Genre[];
  author: Author;
  volumes: Volume[];
}

export interface Author {
  id: string;
  displayName: string;
}

export interface Genre {
  id: number;
  name: string;
}

export interface Volume {
  id: number;
  title: string;
  order: number;
  chapters: Chapter[];
}

export interface Chapter {
  id: number;
  title: string;
  viewCount: number;
  publishDate: string; // Cambié Date por string ya que viene como string desde la API
  order: number;
}
