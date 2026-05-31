namespace unoDynamicUI.Models.DynamicForms;

public sealed class DynamicFormDefinition
{
    public string Title { get; set; } = "Dynamic Form";

    public List<DynamicFieldDefinition> Fields { get; set; } = [];
}
