// using System;
// using backend.Models;
// using backend.Repository.IRepository;

// namespace backend.Repository;

// public class AuthorRepository: IAuthorRepository
// {
//     private readonly ApplicationDbContext _context;
//     public AuthorRepository(ApplicationDbContext context)
//     {
//         _context = context;
//     }
// public ICollection<Author> GetAuthors()
//     {
//         return _context.Authors.ToList();
//     }

//     public Author? GetAuthor(int id)
//     {
//         return _context.Authors.FirstOrDefault(a => a.Id == id);
//     }

//     public bool AuthorExists(int id)
//     {
//         return _context.Authors.Any(a => a.Id == id);
//     }

//     public bool AuthorExists(string name)
//     {
//         return _context.Authors.Any(a => a.Name.ToLower() == name.ToLower());
//     }

//     public bool CreateAuthor(Author author)
//     {
//         _context.Authors.Add(author);
//         return Save();
//     }

//     public bool UpdateAuthor(Author author)
//     {
//         _context.Authors.Update(author);
//         return Save();
//     }

//     public bool DeleteAuthor(Author author)
//     {
//         _context.Authors.Remove(author);
//         return Save();
//     }

//     public bool Save()
//     {
//         return _context.SaveChanges() > 0;
//     }
// }
