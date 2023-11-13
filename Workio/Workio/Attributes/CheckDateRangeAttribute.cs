
using System.ComponentModel.DataAnnotations;
/// <summary>
/// Verifica se uma data é maior que o tempo atual +1
/// </summary>
public class CheckDateRangeAttribute : ValidationAttribute
{
    protected override ValidationResult IsValid(object value, ValidationContext validationContext)
    {
        DateTime dt = (DateTime)value;
        if (dt.Date >= DateTime.UtcNow.AddDays(1).Date)
        {
            return ValidationResult.Success;
        }

        return new ValidationResult(ErrorMessage ?? "Make sure your date is >= than today");
    }

}