using Workio.Areas.Identity.Pages.Account;
using System.ComponentModel.DataAnnotations;

namespace Workio.Attributes
{
    public class CheckboxRequired : ValidationAttribute
    {
        protected override ValidationResult IsValid(object value, ValidationContext validationContext)
        {
            //get the entered value
            var student = (RegisterModel.InputModel)validationContext.ObjectInstance;
            //Check whether the IsAccepted is selected or not.
            if (student.CheckTerms == false)
            {
                //if not checked the checkbox, return the error message.
                return new ValidationResult(ErrorMessage == null ? "You must agree to the terms and conditions." : ErrorMessage);
            }
            return ValidationResult.Success;
        }
    }
}
