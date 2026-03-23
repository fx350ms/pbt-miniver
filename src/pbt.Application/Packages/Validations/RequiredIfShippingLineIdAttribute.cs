using System.ComponentModel.DataAnnotations;
using System.Linq;

public class RequiredIfShippingLineIdAttribute : ValidationAttribute
{
    private readonly string _shippingLineIdPropertyName;
    private readonly int[] _shippingLineIdValues;

    /// <summary>
    /// Tạo một instance của RequiredIfShippingLineIdAttribute
    /// </summary>
    /// <param name="shippingLineIdPropertyName"></param>
    /// <param name="shippingLineIdValues"></param>
    public RequiredIfShippingLineIdAttribute(string shippingLineIdPropertyName, int[] shippingLineIdValues)
    {
        _shippingLineIdPropertyName = shippingLineIdPropertyName;
        _shippingLineIdValues = shippingLineIdValues;
    }

    /// <summary>
    /// Kiểm tra xem shippingLineId có trong danh sách giá trị được chỉ định hay không
    /// </summary>
    /// <param name="value"></param>
    /// <param name="validationContext"></param>
    /// <returns></returns>
    protected override ValidationResult IsValid(object value, ValidationContext validationContext)
    {
        var shippingLineIdProperty = validationContext.ObjectType.GetProperty(_shippingLineIdPropertyName);
        if (shippingLineIdProperty == null)
        {
            return new ValidationResult($"Unknown property {_shippingLineIdPropertyName}");
        }

        var shippingLineIdValue = (int?)shippingLineIdProperty.GetValue(validationContext.ObjectInstance);

        if (_shippingLineIdValues.Any(x => x == shippingLineIdValue) && string.IsNullOrWhiteSpace(value?.ToString()))
        {
            return new ValidationResult(ErrorMessage);
        }

        return ValidationResult.Success;
    }
}
