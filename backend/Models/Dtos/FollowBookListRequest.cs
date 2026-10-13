using System;
using System.ComponentModel.DataAnnotations;

namespace backend.Models.Dtos;
public class FollowBookListRequest
{
    [Required]
    public int BookListId { get; set; }
}