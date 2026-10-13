using System;

namespace backend.Services.Interfaces;

public interface IImageService
{
    Task<string> UploadImageAsync(IFormFile file, string folder, int? maxWidth = null, int? maxHeight = null);
    Task<bool> DeleteImageAsync(string publicId);
    Task<string> ExtractPublicIdFromUrl(string imageUrl);
}
