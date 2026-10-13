using System.ComponentModel.DataAnnotations;

namespace backend.Models;

public class BookListBook
{
    public int BookListId { get; set; }
    public BookList BookList { get; set; } = null!;

    public int BookId { get; set; }
    public Book Book { get; set; } = null!;

    public DateTime AddedDate { get; set; } = DateTime.Now;
    public int Order { get; set; }
}