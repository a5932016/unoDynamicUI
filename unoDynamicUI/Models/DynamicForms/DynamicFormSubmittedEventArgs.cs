namespace unoDynamicUI.Models.DynamicForms;

public sealed class DynamicFormSubmittedEventArgs : EventArgs
{
    public DynamicFormSubmittedEventArgs(IReadOnlyDictionary<string, object?> values)
    {
        Values = values;
    }

    public IReadOnlyDictionary<string, object?> Values { get; }
}
