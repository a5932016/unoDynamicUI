using System.ComponentModel.DataAnnotations;
using System.Globalization;
using System.Reflection;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace unoDynamicUI.Services.DynamicForms;

public static class DynamicFormDefinitionFactory
{
    private const string DefaultEmailPattern = @"^[^\s@]+@[^\s@]+\.[^\s@]+$";

    public static DynamicFormDefinition FromModel<TModel>(string? title = null)
        where TModel : class, new()
    {
        return FromModel(new TModel(), title);
    }

    public static DynamicFormDefinition FromJson(string json)
    {
        if (string.IsNullOrWhiteSpace(json))
        {
            throw new InvalidOperationException("The JSON form definition cannot be empty.");
        }

        var options = new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        };

        options.Converters.Add(new JsonStringEnumConverter(JsonNamingPolicy.CamelCase));

        var definition = JsonSerializer.Deserialize<DynamicFormDefinition>(json, options)
            ?? throw new InvalidOperationException("The JSON form definition is empty.");

        NormalizeDefinition(definition);
        return definition;
    }

    public static DynamicFormDefinition FromModel<TModel>(TModel model, string? title = null)
        where TModel : class
    {
        var modelType = typeof(TModel);
        var definition = new DynamicFormDefinition
        {
            Title = string.IsNullOrWhiteSpace(title) ? modelType.Name : title
        };

        foreach (var property in modelType.GetProperties(BindingFlags.Public | BindingFlags.Instance))
        {
            if (!property.CanRead)
            {
                continue;
            }

            var display = property.GetCustomAttribute<DisplayAttribute>();
            var required = property.GetCustomAttribute<RequiredAttribute>();
            var stringLength = property.GetCustomAttribute<StringLengthAttribute>();
            var minLength = property.GetCustomAttribute<MinLengthAttribute>();
            var maxLength = property.GetCustomAttribute<MaxLengthAttribute>();
            var range = property.GetCustomAttribute<RangeAttribute>();
            var regex = property.GetCustomAttribute<RegularExpressionAttribute>();
            var email = property.GetCustomAttribute<EmailAddressAttribute>();

            var field = new DynamicFieldDefinition
            {
                Key = property.Name,
                Label = display?.Name ?? property.Name,
                Placeholder = display?.Prompt,
                Type = ResolveFieldType(property.PropertyType),
                IsRequired = required is not null,
                MinLength = minLength?.Length ?? stringLength?.MinimumLength,
                MaxLength = maxLength?.Length ?? stringLength?.MaximumLength,
                RegexPattern = regex?.Pattern ?? (email is not null ? DefaultEmailPattern : null),
                InitialValue = ConvertToString(property.GetValue(model))
            };

            if (range is not null)
            {
                field.Min = ConvertToDouble(range.Minimum);
                field.Max = ConvertToDouble(range.Maximum);
            }

            definition.Fields.Add(field);
        }

        NormalizeDefinition(definition);
        return definition;
    }

    private static void NormalizeDefinition(DynamicFormDefinition definition)
    {
        definition.Title = string.IsNullOrWhiteSpace(definition.Title)
            ? "Dynamic Form"
            : definition.Title.Trim();

        foreach (var field in definition.Fields)
        {
            field.Key = string.IsNullOrWhiteSpace(field.Key)
                ? Guid.NewGuid().ToString("N")
                : field.Key.Trim();

            field.Label = string.IsNullOrWhiteSpace(field.Label)
                ? field.Key
                : field.Label.Trim();

            if (field.MinLength is < 0)
            {
                field.MinLength = 0;
            }

            if (field.MaxLength is < 0)
            {
                field.MaxLength = null;
            }

            if (field.MaxLength is not null && field.MinLength is not null && field.MaxLength < field.MinLength)
            {
                (field.MinLength, field.MaxLength) = (field.MaxLength, field.MinLength);
            }

            if (field.Max is not null && field.Min is not null && field.Max < field.Min)
            {
                (field.Min, field.Max) = (field.Max, field.Min);
            }
        }
    }

    private static DynamicFieldType ResolveFieldType(Type type)
    {
        var actualType = Nullable.GetUnderlyingType(type) ?? type;

        if (actualType == typeof(bool))
        {
            return DynamicFieldType.Boolean;
        }

        if (actualType == typeof(byte) ||
            actualType == typeof(short) ||
            actualType == typeof(int) ||
            actualType == typeof(long) ||
            actualType == typeof(float) ||
            actualType == typeof(double) ||
            actualType == typeof(decimal))
        {
            return DynamicFieldType.Number;
        }

        return DynamicFieldType.Text;
    }

    private static string? ConvertToString(object? value)
    {
        return value switch
        {
            null => null,
            bool boolValue => boolValue.ToString(),
            IFormattable formattable => formattable.ToString(null, CultureInfo.InvariantCulture),
            _ => value.ToString()
        };
    }

    private static double? ConvertToDouble(object? value)
    {
        if (value is null)
        {
            return null;
        }

        return value switch
        {
            byte byteValue => byteValue,
            short shortValue => shortValue,
            int intValue => intValue,
            long longValue => longValue,
            float floatValue => floatValue,
            double doubleValue => doubleValue,
            decimal decimalValue => (double)decimalValue,
            _ => double.TryParse(value.ToString(), NumberStyles.Float, CultureInfo.InvariantCulture, out var result)
                ? result
                : null
        };
    }
}
