using FluentValidation;
using Microsoft.AspNetCore.Http;

namespace Application.Common.Validators
{
    public class FileValidator : AbstractValidator<IFormFile>
    {
        // 10 MB
        public const long MaxBytes = 10 * 1024 * 1024;

        private static readonly HashSet<string> AllowedContentTypes =
            new(new[] { "image/png", "image/jpeg", "image/webp" }, StringComparer.OrdinalIgnoreCase);

        private static readonly HashSet<string> AllowedExtensions =
            new(new[] { ".png", ".jpg", ".jpeg", ".webp" }, StringComparer.OrdinalIgnoreCase);

        // Mapa simple de extensiones -> tipos MIME válidos
        private static readonly Dictionary<string, string[]> ExtToContentTypes =
            new(StringComparer.OrdinalIgnoreCase)
            {
                [".png"] = new[] { "image/png" },
                [".jpg"] = new[] { "image/jpeg" },
                [".jpeg"] = new[] { "image/jpeg" },
                [".webp"] = new[] { "image/webp" },
            };

        public FileValidator()
        {
            // Debe venir un archivo
            RuleFor(file => file)
                .NotNull()
                .WithMessage("A file is required.");

            // Reglas que solo aplican si hay archivo
            When(file => file is not null, () =>
            {
                // No vacío y no mayor a 10 MB
                RuleFor(file => file!.Length)
                    .GreaterThan(0).WithMessage("The file is empty.")
                    .LessThanOrEqualTo(MaxBytes).WithMessage($"The file must not exceed {MaxBytes / (1024 * 1024)}MB.");

                // Content-Type permitido (normalizando a minúsculas)
                RuleFor(file => (file!.ContentType ?? string.Empty).ToLowerInvariant())
                    .Must(ct => AllowedContentTypes.Contains(ct))
                    .WithMessage("Only PNG, WEBP or JPEG files are allowed.");

                // Extensión permitida
                RuleFor(file => Path.GetExtension(file!.FileName).ToLowerInvariant())
                    .Must(ext => AllowedExtensions.Contains(ext))
                    .WithMessage("File extension must be .png, .jpg, .jpeg or .webp.");

                // Coherencia extensión <-> content-type (opcional pero recomendable)
                RuleFor(file => file)
                    .Must(file =>
                    {
                        var ext = Path.GetExtension(file!.FileName)?.ToLowerInvariant() ?? string.Empty;
                        var ct = (file.ContentType ?? string.Empty).ToLowerInvariant();

                        return ExtToContentTypes.TryGetValue(ext, out var validCts) && validCts.Contains(ct);
                    })
                    .WithMessage("The file extension does not match the content type.");
            });
        }
    }
}
