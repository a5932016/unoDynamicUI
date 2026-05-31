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
        BuildMessage = "Ready. Click Open Template Page to test your JSON.";

        GoToSecond = new AsyncRelayCommand(GoToSecondView);
        OpenTemplatePage = new AsyncRelayCommand(OpenTemplatePageAsync);
    }

    public string? Title { get; }

    [ObservableProperty]
    private string? name;

    [ObservableProperty]
    private string jsonTemplate = string.Empty;

    [ObservableProperty]
    private string buildMessage = string.Empty;

    public ICommand GoToSecond { get; }

    public ICommand OpenTemplatePage { get; }

    private async Task GoToSecondView()
    {
        await _navigator.NavigateViewModelAsync<SecondViewModel>(this, data: new Entity(Name ?? string.Empty));
    }

    private async Task OpenTemplatePageAsync()
    {
        if (string.IsNullOrWhiteSpace(JsonTemplate))
        {
            BuildMessage = "JSON template cannot be empty.";
            return;
        }

        BuildMessage = "Opening dynamic template page from JSON...";
        await _navigator.NavigateViewModelAsync<DynamicTemplateViewModel>(
            this,
            data: new DynamicTemplateRequest(JsonTemplate));
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
