using System.Text.Json;

namespace unoDynamicUI.Presentation.DynamicForms;

public partial class DynamicFormViewModel : ObservableObject
{
    public DynamicFormViewModel(DynamicFormDefinition definition)
    {
        Definition = definition;
        Title = definition.Title;
        Fields = definition.Fields
            .Select(field => new DynamicFieldInputViewModel(field))
            .ToList();

        ConfirmCommand = new RelayCommand(HandleConfirm);
        CancelCommand = new RelayCommand(HandleCancel);
        LastSubmittedValues = new Dictionary<string, object?>();
    }

    public DynamicFormDefinition Definition { get; }

    public string Title { get; }

    public IReadOnlyList<DynamicFieldInputViewModel> Fields { get; }

    [ObservableProperty]
    private string? validationSummary;

    [ObservableProperty]
    private string lastSubmittedJson = "{}";

    public IReadOnlyDictionary<string, object?> LastSubmittedValues { get; private set; }

    public ICommand ConfirmCommand { get; }

    public ICommand CancelCommand { get; }

    public event EventHandler<DynamicFormSubmittedEventArgs>? Confirmed;

    public event EventHandler? Cancelled;

    private void HandleConfirm()
    {
        ValidationSummary = string.Empty;

        var isAllValid = true;
        foreach (var field in Fields)
        {
            if (!field.Validate())
            {
                isAllValid = false;
            }
        }

        if (!isAllValid)
        {
            ValidationSummary = "Please fix the invalid fields before confirm.";
            return;
        }

        var savedValues = Fields.ToDictionary(field => field.Key, field => field.GetValue());
        LastSubmittedValues = savedValues;
        LastSubmittedJson = JsonSerializer.Serialize(savedValues, new JsonSerializerOptions
        {
            WriteIndented = true
        });

        Confirmed?.Invoke(this, new DynamicFormSubmittedEventArgs(savedValues));
    }

    private void HandleCancel()
    {
        ValidationSummary = string.Empty;
        Cancelled?.Invoke(this, EventArgs.Empty);
    }
}
