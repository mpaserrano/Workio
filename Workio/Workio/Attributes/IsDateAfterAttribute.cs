
using System.ComponentModel.DataAnnotations;
/// <summary>
/// Verifica se uma data é maior que outra
/// </summary>
public sealed class IsDateAfterAttribute : ValidationAttribute
{
    private readonly string testedPropertyName;
    private readonly bool allowEqualDates;

    public IsDateAfterAttribute(string testedPropertyName, bool allowEqualDates = false)
    {
        this.testedPropertyName = testedPropertyName;
        this.allowEqualDates = allowEqualDates;
    }

    protected override ValidationResult IsValid(object value, ValidationContext validationContext)
    {
        var propertyTestedInfo = validationContext.ObjectType.GetProperty(this.testedPropertyName);
        if (propertyTestedInfo == null)
        {
            return new ValidationResult(string.Format("unknown property {0}", this.testedPropertyName));
        }

        var propertyTestedValue = propertyTestedInfo.GetValue(validationContext.ObjectInstance, null);

        if (value == null || !(value is DateTime))
        {
            return ValidationResult.Success;
        }

        if (propertyTestedValue == null || !(propertyTestedValue is DateTime))
        {
            return ValidationResult.Success;
        }

        // Compare values
        if ((DateTime)value >= (DateTime)propertyTestedValue)
        {
            if (this.allowEqualDates)
            {
                return ValidationResult.Success;
            }
            else if ((DateTime)value > (DateTime)propertyTestedValue)
            {
                return ValidationResult.Success;
            }
        }

        return new ValidationResult(FormatErrorMessage(validationContext.DisplayName));
    }
}