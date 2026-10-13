
export interface CardUpdate {
  id:             number;
  title:          string;
  // score:          number;
  // totalRatings:   number;
  // state:          number;
  imageUrl:       string;
  recentChapters: RecentChapter[];
}

export interface RecentChapter {
  id:          number;
  title:       string;
  // viewCount:   number;
  publishDate: Date;
  order:       number;
}



