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

        JsonDefinition = CreateDefaultJsonDefinition();
        LastDialogAction = "No action yet.";
        LastDialogValues = "{}";
        FormBuildMessage = "Ready.";

        GoToSecond = new AsyncRelayCommand(GoToSecondView);
        OpenJsonDialog = new RelayCommand(OpenJsonDialogView);
        OpenModelDialog = new RelayCommand(OpenModelDialogView);
        OpenJsonPage = new AsyncRelayCommand(OpenJsonPageViewAsync);
        OpenModelPage = new AsyncRelayCommand(OpenModelPageViewAsync);
    }

    public string? Title { get; }

    [ObservableProperty]
    private string? name;

    [ObservableProperty]
    private string jsonDefinition = string.Empty;

    [ObservableProperty]
    private string lastDialogAction = string.Empty;

    [ObservableProperty]
    private string lastDialogValues = string.Empty;

    [ObservableProperty]
    private string formBuildMessage = string.Empty;

    public ICommand GoToSecond { get; }

    public ICommand OpenJsonDialog { get; }

    public ICommand OpenModelDialog { get; }

    public ICommand OpenJsonPage { get; }

    public ICommand OpenModelPage { get; }

    public event EventHandler<DynamicFormDialogRequestEventArgs>? DynamicDialogRequested;

    private async Task GoToSecondView()
    {
        await _navigator.NavigateViewModelAsync<SecondViewModel>(this, data: new Entity(Name ?? string.Empty));
    }

    private void OpenJsonDialogView()
    {
        if (!TryBuildDefinitionFromJson(out var definition))
        {
            return;
        }

        ShowDialog(definition);
    }

    private void OpenModelDialogView()
    {
        var definition = BuildModelDefinition();
        ShowDialog(definition);
    }

    private async Task OpenJsonPageViewAsync()
    {
        if (!TryBuildDefinitionFromJson(out var definition))
        {
            return;
        }

        FormBuildMessage = "JSON page generated.";
        await _navigator.NavigateViewModelAsync<DynamicFormPageViewModel>(this, data: new DynamicFormRequest(definition));
    }

    private async Task OpenModelPageViewAsync()
    {
        var definition = BuildModelDefinition();
        FormBuildMessage = "Model page generated.";
        await _navigator.NavigateViewModelAsync<DynamicFormPageViewModel>(this, data: new DynamicFormRequest(definition));
    }

    private bool TryBuildDefinitionFromJson(out DynamicFormDefinition definition)
    {
        try
        {
            definition = DynamicFormDefinitionFactory.FromJson(JsonDefinition);
            FormBuildMessage = "JSON form generated.";
            return true;
        }
        catch (Exception ex)
        {
            definition = new DynamicFormDefinition();
            FormBuildMessage = $"JSON parse failed: {ex.Message}";
            return false;
        }
    }

    private static DynamicFormDefinition BuildModelDefinition()
    {
        var sampleModel = new SampleProfileInputModel
        {
            FullName = "Alex Lee",
            Age = 28,
            Email = "alex@example.com",
            AcceptTerms = false
        };

        return DynamicFormDefinitionFactory.FromModel(sampleModel, "Model Generated Form");
    }

    private void ShowDialog(DynamicFormDefinition definition)
    {
        var formViewModel = new DynamicFormViewModel(definition);

        formViewModel.Confirmed += (_, args) =>
        {
            LastDialogAction = "Confirm clicked.";
            LastDialogValues = JsonSerializer.Serialize(args.Values, new JsonSerializerOptions
            {
                WriteIndented = true
            });
            FormBuildMessage = "Dialog confirm event fired.";
        };

        formViewModel.Cancelled += (_, _) =>
        {
            LastDialogAction = "Cancel clicked.";
            FormBuildMessage = "Dialog cancel event fired.";
        };

        DynamicDialogRequested?.Invoke(this, new DynamicFormDialogRequestEventArgs(formViewModel));
    }

    private static string CreateDefaultJsonDefinition()
    {
        return """
               {
                 "title": "JSON Generated Form",
                 "fields": [
                   {
                     "key": "fullName",
                     "label": "Full Name",
                     "placeholder": "Type your full name",
                     "type": "text",
                     "isRequired": true,
                     "minLength": 2,
                     "maxLength": 40
                   },
                   {
                     "key": "age",
                     "label": "Age",
                     "placeholder": "18 to 60",
                     "type": "number",
                     "isRequired": true,
                     "min": 18,
                     "max": 60
                   },
                   {
                     "key": "email",
                     "label": "Email",
                     "placeholder": "name@example.com",
                     "type": "text",
                     "regexPattern": "^[^\\s@]+@[^\\s@]+\\.[^\\s@]+$"
                   },
                   {
                     "key": "note",
                     "label": "Note",
                     "placeholder": "Optional note",
                     "type": "multilineText",
                     "maxLength": 200
                   }
                 ]
               }
               """;
    }
}
