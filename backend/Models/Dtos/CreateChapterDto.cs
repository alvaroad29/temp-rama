using Microsoft.AspNetCore.Http;
using System.ComponentModel.DataAnnotations;

namespace backend.Models.Dtos
{
    public class CreateChapterDto
    {
        [Required]
        public string Title { get; set; } = string.Empty;
        
        [Required]
        public int VolumeId { get; set; }
        
        [Required]
        public int Order { get; set; }
        
        [Required]
        [StringLength(20000, ErrorMessage = "El contenido del capítulo no puede exceder los 20,000 caracteres")]
        public string Content { get; set; } = string.Empty;
        public bool IsActive { get; set; } = true; 
    }
}
