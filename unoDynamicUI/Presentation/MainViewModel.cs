using System.Text.Json;

namespace unoDynamicUI.Presentation;

public partial class MainViewModel : ObservableObject
{
    private readonly INavigator _navigator;

    public MainViewModel(
        IStringLocalizer localizer,
        IOptions<AppConfig> appInfo,
        INavigator navigator)
    {
        _navigator = navigator;

        Title = "Main";
        Title += $" - {localizer["ApplicationName"]}";
        Title += $" - {appInfo?.Value?.Environment}";

        JsonTemplate = CreateDefaultTemplateJson();
        BuildMessage = "Ready. Use JSON to open Dialog or Popup Page.";
        LastAction = "No action yet.";
        LastSavedValues = "{}";

        GoToSecond = new AsyncRelayCommand(GoToSecondView);
        OpenTemplateDialog = new RelayCommand(OpenTemplateDialogView);
    }

    public string? Title { get; }

    [ObservableProperty]
    private string? name;

    [ObservableProperty]
    private string jsonTemplate = string.Empty;

    [ObservableProperty]
    private string buildMessage = string.Empty;

    [ObservableProperty]
    private string lastAction = string.Empty;

    [ObservableProperty]
    private string lastSavedValues = string.Empty;

    public ICommand GoToSecond { get; }

    public ICommand OpenTemplateDialog { get; }

    public event EventHandler<DynamicTemplateRequestedEventArgs>? TemplateDialogRequested;

    private async Task GoToSecondView()
    {
        await _navigator.NavigateViewModelAsync<SecondViewModel>(this, data: new Entity(Name ?? string.Empty));
    }

    private void OpenTemplateDialogView()
    {
      if (!TryBuildTemplateViewModel(out var viewModel))
        {
            return;
        }

      BuildMessage = "Opening dynamic form dialog from JSON...";
      TemplateDialogRequested?.Invoke(this, new DynamicTemplateRequestedEventArgs(viewModel));
    }

    private bool TryBuildTemplateViewModel(out DynamicTemplateViewModel viewModel)
    {
      if (string.IsNullOrWhiteSpace(JsonTemplate))
      {
        BuildMessage = "JSON template cannot be empty.";
        viewModel = new DynamicTemplateViewModel(new DynamicTemplateRequest("{\"fields\":[]}"));
        return false;
      }

      viewModel = new DynamicTemplateViewModel(new DynamicTemplateRequest(JsonTemplate));
      viewModel.Confirmed += OnTemplateConfirmed;
      viewModel.Cancelled += OnTemplateCancelled;
      return true;
    }

    private void OnTemplateConfirmed(object? sender, IReadOnlyDictionary<string, object?> values)
    {
      LastAction = "Confirm clicked.";
      LastSavedValues = JsonSerializer.Serialize(values, new JsonSerializerOptions
      {
        WriteIndented = true
      });
      BuildMessage = "Confirm event triggered and values saved.";
    }

    private void OnTemplateCancelled(object? sender, EventArgs e)
    {
      LastAction = "Cancel clicked.";
      BuildMessage = "Cancel event triggered.";
    }

    private static string CreateDefaultTemplateJson()
    {
        return """
               {
                 "title": "Employee Profile Template",
                 "fields": [
                   {
                     "key": "employeeName",
                     "label": "Employee Name",
                     "type": "input",
                     "placeholder": "Type employee name",
                     "required": true,
                     "minLength": 2,
                     "maxLength": 40
                   },
                   {
                     "key": "department",
                     "label": "Department",
                     "type": "select",
                     "required": true,
                     "options": ["Engineering", "Design", "HR", "Finance"]
                   },
                   {
                     "key": "salary",
                     "label": "Monthly Salary",
                     "type": "number",
                     "placeholder": "30000 to 200000",
                     "required": true,
                     "min": 30000,
                     "max": 200000
                   },
                   {
                     "key": "startDate",
                     "label": "Start Date",
                     "type": "date",
                     "required": true
                   },
                   {
                     "key": "isRemote",
                     "label": "Remote Work",
                     "type": "checkbox"
                   },
                   {
                     "key": "resume",
                     "label": "Resume File",
                     "type": "filePicker",
                     "required": true,
                     "accept": [".pdf", ".docx"]
                   }
                 ]
               }
               """;
    }
}
