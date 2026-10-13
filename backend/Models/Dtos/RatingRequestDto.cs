using System;
using System.ComponentModel.DataAnnotations;

namespace backend.Models.Dtos;

public class RatingRequestDto
{
    [Range(1, 5, ErrorMessage = "La calificación debe estar entre 1 y 5")]
    public int Rating { get; set; }
}
