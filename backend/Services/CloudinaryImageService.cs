using System;
using backend.Services.Interfaces;
using CloudinaryDotNet;
using CloudinaryDotNet.Actions;

namespace backend.Services;

public class CloudinaryImageService: IImageService
{
    private readonly Cloudinary _cloudinary;

    public CloudinaryImageService(Cloudinary cloudinary, ILogger<CloudinaryImageService> logger)
    {
        _cloudinary = cloudinary;
    }

    public async Task<string> UploadImageAsync(IFormFile file, string folder, int? maxWidth = null, int? maxHeight = null)
    {
        if (file == null || file.Length == 0)
            throw new ArgumentException("Archivo inválido");

        if (file.Length > 5 * 1024 * 1024)
            throw new ArgumentException("La imagen no puede superar 5MB");

        // Validar extensiones
        var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".webp" };
        var extension = Path.GetExtension(file.FileName).ToLower();
        if (!allowedExtensions.Contains(extension))
            throw new ArgumentException("Formato de imagen no permitido");

        // Configurar transformaciones
        var transformation = new Transformation().Quality("auto");

        if (maxWidth.HasValue && maxHeight.HasValue)
            transformation = transformation.Width(maxWidth.Value).Height(maxHeight.Value).Crop("limit");
        else if (maxWidth.HasValue)
            transformation = transformation.Width(maxWidth.Value).Crop("scale");
        else if (maxHeight.HasValue)
            transformation = transformation.Height(maxHeight.Value).Crop("scale");

        // Subir a Cloudinary
        await using var stream = file.OpenReadStream();
        
        var uploadParams = new ImageUploadParams
        {
            File = new FileDescription(file.FileName, stream),
            Folder = folder,
            Transformation = transformation
        };

        var uploadResult = await _cloudinary.UploadAsync(uploadParams);
        
        if (uploadResult.Error != null)
        {
            throw new Exception($"Error al subir imagen: {uploadResult.Error.Message}");
        }

        return uploadResult.SecureUrl.ToString();
    }

    public async Task<string> ExtractPublicIdFromUrl(string imageUrl)
    {
        try
        {
            if (string.IsNullOrEmpty(imageUrl))
                throw new ArgumentException("La URL no puede estar vacía");

            // Ejemplo de URL: 
            // https://res.cloudinary.com/demo/image/upload/v123456789/user-profiles/mi-imagen.jpg
            
            var uri = new Uri(imageUrl);
            
            // Dividir la URL por "/"
            var parts = uri.AbsolutePath.Split('/');
            
            // Encontrar el índice de "upload"
            var uploadIndex = Array.IndexOf(parts, "upload");
            if (uploadIndex == -1)
                throw new ArgumentException("URL de Cloudinary no válida - no contiene 'upload'");

            // El public_id está después de la versión (upload/[version]/[public_id])
            // Tomamos todo lo que viene después de la versión
            var publicIdParts = parts.Skip(uploadIndex + 2).ToArray();
            
            if (publicIdParts.Length == 0)
                throw new ArgumentException("URL de Cloudinary no válida - sin public_id");

            // Unir todas las partes (esto incluye la carpeta)
            var publicIdWithExtension = string.Join("/", publicIdParts);
            
            // Remover SOLO la extensión del último segmento
            var lastDotIndex = publicIdWithExtension.LastIndexOf('.');
            if (lastDotIndex > 0)
            {
                publicIdWithExtension = publicIdWithExtension.Substring(0, lastDotIndex);
            }

            return publicIdWithExtension; // Retorna: "user-profiles/mqv54tjlxm0otchgmru8"
        }
        catch (Exception ex)
        {
            throw new ArgumentException($"No se pudo extraer el public_id: {ex.Message}");
        }
    }

    public async Task<bool> DeleteImageAsync(string publicId)
    {
        var deleteParams = new DeletionParams(publicId);
        var result = await _cloudinary.DestroyAsync(deleteParams);
        return result.Result == "ok";
    }
}
