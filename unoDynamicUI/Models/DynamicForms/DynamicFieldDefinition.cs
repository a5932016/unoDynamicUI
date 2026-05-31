namespace unoDynamicUI.Models.DynamicForms;

public sealed class DynamicFieldDefinition
{
    public string Key { get; set; } = string.Empty;

    public string Label { get; set; } = string.Empty;

    public string? Placeholder { get; set; }

    public DynamicFieldType Type { get; set; } = DynamicFieldType.Text;

    public bool IsRequired { get; set; }

    public int? MinLength { get; set; }

    public int? MaxLength { get; set; }

    public double? Min { get; set; }

    public double? Max { get; set; }

    public string? RegexPattern { get; set; }

    public string? InitialValue { get; set; }
}
