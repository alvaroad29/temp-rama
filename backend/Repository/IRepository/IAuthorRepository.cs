using System;
using backend.Models;

namespace backend.Repository.IRepository;

public interface IAuthorRepository
{
    ICollection<Author> GetAuthors();
    Author? GetAuthor(int id);
    bool AuthorExists(int id);
    bool AuthorExists(string name);
    bool CreateAuthor(Author author);
    bool UpdateAuthor(Author author);
    bool DeleteAuthor(Author author);
    bool Save();
}
