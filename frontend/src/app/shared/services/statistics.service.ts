import { inject, Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { environment } from '../../../environments/environment';
import { Observable } from 'rxjs';

const baseUrl = environment.baseUrl;

export interface AdminStatisticsDto {
  users: UserStats;
  books: BookStats;
  reports: ReportStats;
  submissions: SubmissionStats;
  userRegistrationTrend: MonthlyRegistrationDto[];
  recentActivity: DailyActivityDto[];
}

export interface UserStats {
  totalUsers: number;
  activeUsers: number;
  bannedUsers: number;
  newUsersThisWeek: number;
  newUsersThisMonth: number;
}

export interface BookStats {
  totalBooks: number;
  activeBooks: number;
  inactiveBooks: number;
  completedBooks: number;
  pausedBooks: number;
  totalChapters: number;
}

export interface ReportStats {
  totalReports: number;
  pendingReports: number;
  inReviewReports: number;
  resolvedReports: number;
}

export interface SubmissionStats {
  totalSubmissions: number;
  pendingSubmissions: number;
  approvedSubmissions: number;
  rejectedSubmissions: number;
}

export interface MonthlyRegistrationDto {
  year: number;
  month: number;
  count: number;
  monthName: string;
}

export interface DailyActivityDto {
  date: string;
  newUsers: number;
  newBooks: number;
  newReports: number;
}

export interface AuthorStatisticsDto {
  totalBooks: number;
  activeBooks: number;
  completedBooks: number;
  pausedBooks: number;
  totalChapters: number;
  totalVolumes: number;
  totalViews: number;
  viewsThisWeek: number;
  viewsThisMonth: number;
  averageRating: number;
  totalRatings: number;
  lastChapterPublished: string | null;
  lastBookUpdate: string | null;
  mostPopularBook: PopularBookDto | null;
  bestRatedBook: PopularBookDto | null;
  viewsTrend: MonthlyViewsDto[];
  booksPerformance: BookPerformanceDto[];
}

export interface PopularBookDto {
  id: number;
  title: string;
  imageUrl: string | null;
  viewCount: number;
  score: number | null;
  totalRatings: number;
}

export interface MonthlyViewsDto {
  year: number;
  month: number;
  views: number;
  monthName: string;
}

export interface BookPerformanceDto {
  bookId: number;
  title: string;
  viewCount: number;
  score: number | null;
  chapterCount: number;
  state: string;
}

@Injectable({
  providedIn: 'root'
})
export class StatisticsService {
  private http = inject(HttpClient);

  getAdminStatistics(): Observable<AdminStatisticsDto> {
    return this.http.get<AdminStatisticsDto>(`${baseUrl}/Users/admin/statistics`);
  }

  getAuthorStatistics(): Observable<AuthorStatisticsDto> {
    return this.http.get<AuthorStatisticsDto>(`${baseUrl}/Users/author/statistics`);
  }

  getAuthorStatisticsForAdmin(authorId: string): Observable<AuthorStatisticsDto> {
    return this.http.get<AuthorStatisticsDto>(`${baseUrl}/Users/admin/author/${authorId}/statistics`);
  }
}
