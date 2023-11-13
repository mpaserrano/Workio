using System.ComponentModel.DataAnnotations;
using System.IO;
using System.Linq;

public class ValidateImageAttribute : ValidationAttribute
{
    private readonly string[] _allowedExtensions = new[] { ".png", ".jpg", ".jpeg", ".gif", ".tif" };

    public override bool IsValid(object value)
    {
        if (value == null)
        {
            return true; // returning true here since the field is not marked as required
        }
        IFormFile file = (IFormFile) value;
        string extension = Path.GetExtension(file.FileName);

        Console.WriteLine("AQUI----");
        if (!_allowedExtensions.Contains(extension))
        {
            return false;
        }

        return true;
    }
}
