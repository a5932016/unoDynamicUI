using System.Text.Json;

namespace unoDynamicUI.Presentation.DynamicForms;

public partial class DynamicFormPageViewModel : ObservableObject
{
    public DynamicFormPageViewModel(DynamicFormRequest request)
    {
        FormViewModel = new DynamicFormViewModel(request.Definition);
        LastAction = "No action yet.";
        LastSavedValues = "{}";

        FormViewModel.Confirmed += HandleConfirmed;
        FormViewModel.Cancelled += HandleCancelled;
    }

    public DynamicFormViewModel FormViewModel { get; }

    [ObservableProperty]
    private string lastAction;

    [ObservableProperty]
    private string? lastSavedValues;

    private void HandleConfirmed(object? sender, DynamicFormSubmittedEventArgs e)
    {
        LastAction = "Confirm clicked.";
        LastSavedValues = JsonSerializer.Serialize(e.Values, new JsonSerializerOptions
        {
            WriteIndented = true
        });
    }

    private void HandleCancelled(object? sender, EventArgs e)
    {
        LastAction = "Cancel clicked.";
    }
}
