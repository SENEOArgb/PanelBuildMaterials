using System.ComponentModel.DataAnnotations;
using System.Globalization;

namespace PanelBuildMaterials.Utilities
{
    public class ValidatePrices : ValidationAttribute
    {
        public ValidatePrices()
        {
            ErrorMessage = "Значение должно быть положительным числом.";
        }

        public override bool IsValid(object value)
        {
            if (value == null || string.IsNullOrWhiteSpace(value.ToString()))
            {
                return false;
            }

            if (decimal.TryParse(value.ToString(), NumberStyles.Any, CultureInfo.InvariantCulture, out decimal result))
            {
                return result > 0;
            }

            return false;
        }
    }
}
