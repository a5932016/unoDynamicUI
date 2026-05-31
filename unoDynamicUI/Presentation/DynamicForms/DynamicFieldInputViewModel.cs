using System.Globalization;
using System.Text.RegularExpressions;

namespace unoDynamicUI.Presentation.DynamicForms;

public partial class DynamicFieldInputViewModel : ObservableObject
{
    private readonly DynamicFieldDefinition _definition;

    public DynamicFieldInputViewModel(DynamicFieldDefinition definition)
    {
        _definition = definition;
        Label = definition.Label;
        Placeholder = definition.Placeholder;
        Type = definition.Type;

        if (Type == DynamicFieldType.Boolean)
        {
            if (bool.TryParse(definition.InitialValue, out var boolValue))
            {
                IsChecked = boolValue;
            }
        }
        else
        {
            InputText = definition.InitialValue;
        }
    }

    public string Key => _definition.Key;

    public string Label { get; }

    public string? Placeholder { get; }

    public DynamicFieldType Type { get; }

    [ObservableProperty]
    private string? inputText;

    [ObservableProperty]
    private bool? isChecked;

    [ObservableProperty]
    private string? validationMessage;

    partial void OnInputTextChanged(string? value)
    {
        ValidationMessage = string.Empty;
    }

    partial void OnIsCheckedChanged(bool? value)
    {
        ValidationMessage = string.Empty;
    }

    public bool Validate()
    {
        ValidationMessage = string.Empty;

        if (Type == DynamicFieldType.Boolean)
        {
            if (_definition.IsRequired && IsChecked != true)
            {
                ValidationMessage = $"{Label} is required.";
                return false;
            }

            return true;
        }

        var currentValue = (InputText ?? string.Empty).Trim();

        if (_definition.IsRequired && string.IsNullOrWhiteSpace(currentValue))
        {
            ValidationMessage = $"{Label} is required.";
            return false;
        }

        if (string.IsNullOrWhiteSpace(currentValue))
        {
            return true;
        }

        if (_definition.MinLength is not null && currentValue.Length < _definition.MinLength)
        {
            ValidationMessage = $"{Label} must be at least {_definition.MinLength} characters.";
            return false;
        }

        if (_definition.MaxLength is not null && currentValue.Length > _definition.MaxLength)
        {
            ValidationMessage = $"{Label} must be at most {_definition.MaxLength} characters.";
            return false;
        }

        if (!string.IsNullOrWhiteSpace(_definition.RegexPattern))
        {
            try
            {
                if (!Regex.IsMatch(currentValue, _definition.RegexPattern))
                {
                    ValidationMessage = $"{Label} format is invalid.";
                    return false;
                }
            }
            catch (ArgumentException)
            {
                ValidationMessage = $"{Label} has an invalid regex pattern.";
                return false;
            }
        }

        if (Type == DynamicFieldType.Number)
        {
            if (!TryParseNumber(currentValue, out var numericValue))
            {
                ValidationMessage = $"{Label} must be a valid number.";
                return false;
            }

            if (_definition.Min is not null && numericValue < _definition.Min)
            {
                ValidationMessage = $"{Label} must be greater than or equal to {_definition.Min}.";
                return false;
            }

            if (_definition.Max is not null && numericValue > _definition.Max)
            {
                ValidationMessage = $"{Label} must be less than or equal to {_definition.Max}.";
                return false;
            }
        }

        return true;
    }

    public object? GetValue()
    {
        if (Type == DynamicFieldType.Boolean)
        {
            return IsChecked == true;
        }

        var currentValue = (InputText ?? string.Empty).Trim();

        if (string.IsNullOrWhiteSpace(currentValue))
        {
            return null;
        }

        if (Type == DynamicFieldType.Number && TryParseNumber(currentValue, out var numericValue))
        {
            return numericValue;
        }

        return currentValue;
    }

    private static bool TryParseNumber(string value, out double result)
    {
        return double.TryParse(value, NumberStyles.Float, CultureInfo.CurrentCulture, out result)
            || double.TryParse(value, NumberStyles.Float, CultureInfo.InvariantCulture, out result);
    }
}
