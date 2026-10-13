using backend.Models;
using backend.Models.Dtos;
using backend.Models.Dtos.BookQueries;

namespace backend.Repository.IRepository;

public interface IBookListRepository
{
    Task<PagedResponse<BookList>> GetAllAsync(BookListQueryParameters parameters, bool includePrivate = false);
    Task<BookList?> GetByIdAsync(int id, bool includePrivate = false);
    Task<PagedResponse<BookListItemDto>> GetBooksByListIdAsync(int bookListId, BookListBooksQueryParameters parameters);
    Task<IEnumerable<BookList>> GetByUserIdAsync(string userId, bool includePrivate = false);
    Task<BookList> CreateAsync(BookList bookList);
    Task UpdateAsync(BookList bookList);
    Task<bool> DeleteAsync(int id);
    Task<bool> ExistsAsync(int id);
    Task<bool> IsOwnerAsync(int bookListId, string userId);
    Task<bool> AddBookToListAsync(int bookListId, int bookId);
    Task<bool> RemoveBookFromListAsync(int bookListId, int bookId);
    Task<bool> BookExistsInListAsync(int bookListId, int bookId);
    Task<int> GetUserBookListCountAsync(string userId);
    Task<bool> CanUserCreateMoreListsAsync(string userId, int maxListsPerUser);
     
    Task<IEnumerable<BookListWithMembershipDto>> GetBookListsWithMembershipAsync(string userId, int bookId, bool includePrivate = false);

     // Seguir/Dejar de seguir lista
    Task<bool> FollowBookListAsync(int bookListId, string userId);
    Task<bool> UnfollowBookListAsync(int bookListId, string userId);
    Task<bool> IsFollowingBookListAsync(int bookListId, string userId);
    
    // Obtener listas seguidas por usuario
    Task<PagedResponse<BookList>> GetFollowedBookListsAsync(string userId, BookListQueryParameters parameters);
}