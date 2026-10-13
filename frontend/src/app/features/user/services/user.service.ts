import { inject, Injectable } from '@angular/core';
import { environment } from '../../../../environments/environment';
import { HttpClient, HttpParams } from '@angular/common/http';
import { AssignRoleRequest, PagedResponse, UserActionResponse, UserQueryParams } from '../interfaces/user-query-params';
import { Observable } from 'rxjs';
import { User } from '../../../auth/interfaces/user.interface';

let baseUrl = `${environment.baseUrl}/users`;

@Injectable({
  providedIn: 'root'
})
export class UserService {

  private http = inject(HttpClient);

  getUsers(params: UserQueryParams): Observable<PagedResponse<User>> {
    let httpParams = new HttpParams()
      .set('pageNumber', params.pageNumber.toString())
      .set('pageSize', params.pageSize.toString());

    if (params.searchText) {
      httpParams = httpParams.set('searchText', params.searchText);
    }
    if (params.searchIn) {
      httpParams = httpParams.set('searchIn', params.searchIn);
    }
    if (params.isBanned !== undefined) {
      httpParams = httpParams.set('isBanned', params.isBanned.toString());
    }
    if (params.sortBy) {
      httpParams = httpParams.set('sortBy', params.sortBy);
    }
    if (params.sortDescending !== undefined) {
      httpParams = httpParams.set('sortDescending', params.sortDescending.toString());
    }

    return this.http.get<PagedResponse<User>>(baseUrl, { params: httpParams });
  }

  assignRole(userId: string, roleName: string): Observable<UserActionResponse> {
    const request: AssignRoleRequest = { roleName };
    return this.http.post<UserActionResponse>(`${baseUrl}/${userId}/roles`, request);
  }

  removeRole(userId: string, roleName: string): Observable<UserActionResponse> {
    return this.http.delete<UserActionResponse>(`${baseUrl}/${userId}/roles/${roleName}`);
  }

  banUser(userId: string): Observable<UserActionResponse> {
    return this.http.post<UserActionResponse>(`${baseUrl}/${userId}/ban`, {});
  }

  unbanUser(userId: string): Observable<UserActionResponse> {
    return this.http.post<UserActionResponse>(`${baseUrl}/${userId}/unban`, {});
  }

  deleteUser(userId: string): Observable<UserActionResponse> {
    return this.http.delete<UserActionResponse>(`${baseUrl}/${userId}`);
  }

}
