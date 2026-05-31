using System.Collections.ObjectModel;
using System.Globalization;
using System.Text.Json;
using System.Text.RegularExpressions;

namespace unoDynamicUI.Presentation.DynamicForms;

public sealed record DynamicTemplateRequest(string JsonTemplate);

public partial class DynamicTemplateViewModel : ObservableObject
{
    private readonly JsonSerializerOptions _jsonOptions = new()
    {
        PropertyNameCaseInsensitive = true
    };

    public DynamicTemplateViewModel(DynamicTemplateRequest request)
    {
        JsonTemplate = string.IsNullOrWhiteSpace(request.JsonTemplate)
            ? CreateDefaultTemplateJson()
            : request.JsonTemplate;

        FormTitle = "Dynamic Template";
        StatusMessage = "Load JSON to generate controls.";
        LastAction = "No action yet.";
        SavedValuesJson = "{}";

        LoadFromJsonCommand = new RelayCommand(LoadFromJson);
        ConfirmCommand = new RelayCommand(Confirm);
        CancelCommand = new RelayCommand(Cancel);

        LoadFromJson();
    }

    public ObservableCollection<DynamicFieldState> Fields { get; } = [];

    public IReadOnlyDictionary<string, object?> SavedValues { get; private set; } = new Dictionary<string, object?>();

    [ObservableProperty]
    private string jsonTemplate = string.Empty;

    [ObservableProperty]
    private string formTitle = string.Empty;

    [ObservableProperty]
    private string statusMessage = string.Empty;

    [ObservableProperty]
    private string lastAction = string.Empty;

    [ObservableProperty]
    private string savedValuesJson = string.Empty;

    public ICommand LoadFromJsonCommand { get; }

    public ICommand ConfirmCommand { get; }

    public ICommand CancelCommand { get; }

    public event EventHandler? TemplateChanged;

    public event EventHandler<IReadOnlyDictionary<string, object?>>? Confirmed;

    public event EventHandler? Cancelled;

    public void LoadFromJson()
    {
        try
        {
            var parsed = JsonSerializer.Deserialize<TemplateSchema>(JsonTemplate, _jsonOptions)
                ?? throw new InvalidOperationException("JSON is empty.");

            Fields.Clear();
            FormTitle = string.IsNullOrWhiteSpace(parsed.Title)
                ? "Dynamic Template"
                : parsed.Title.Trim();

            foreach (var rawField in parsed.Fields ?? [])
            {
                if (string.IsNullOrWhiteSpace(rawField.Key))
                {
                    continue;
                }

                Fields.Add(new DynamicFieldState(rawField));
            }

            StatusMessage = $"Loaded {Fields.Count} fields from JSON.";
            TemplateChanged?.Invoke(this, EventArgs.Empty);
        }
        catch (Exception ex)
        {
            StatusMessage = $"JSON parse failed: {ex.Message}";
        }
    }

    public void SetPickedFile(DynamicFieldState field, string? filePath)
    {
        if (!Fields.Contains(field))
        {
            return;
        }

        field.FilePath = filePath ?? string.Empty;
        field.Error = string.Empty;
    }

    public void SetStatus(string message)
    {
        StatusMessage = message;
    }

    private void Confirm()
    {
        var isAllValid = true;

        foreach (var field in Fields)
        {
            if (!ValidateField(field))
            {
                isAllValid = false;
            }
        }

        if (!isAllValid)
        {
            StatusMessage = "Please fix field errors before confirm.";
            return;
        }

        var values = Fields.ToDictionary(field => field.Key, GetTypedValue);
        SavedValues = values;
        SavedValuesJson = JsonSerializer.Serialize(values, new JsonSerializerOptions
        {
            WriteIndented = true
        });

        LastAction = "Confirm clicked.";
        StatusMessage = "Values saved to ViewModel.";
        Confirmed?.Invoke(this, SavedValues);
    }

    private void Cancel()
    {
        LastAction = "Cancel clicked.";
        StatusMessage = "Cancelled.";
        Cancelled?.Invoke(this, EventArgs.Empty);
    }

    private static object? GetTypedValue(DynamicFieldState field)
    {
        return field.Type switch
        {
            "checkbox" => field.BoolValue,
            "date" => field.DateValue,
            "select" => field.SelectedOption,
            "filepicker" => field.FilePath,
            "number" => double.TryParse(field.TextValue, NumberStyles.Float, CultureInfo.InvariantCulture, out var value)
                ? value
                : null,
            _ => field.TextValue
        };
    }

    private static bool ValidateField(DynamicFieldState field)
    {
        field.Error = string.Empty;

        if (field.Type == "checkbox")
        {
            if (field.Required && !field.BoolValue)
            {
                field.Error = $"{field.Label} must be checked.";
                return false;
            }

            return true;
        }

        if (field.Type == "select")
        {
            if (field.Required && string.IsNullOrWhiteSpace(field.SelectedOption))
            {
                field.Error = $"{field.Label} is required.";
                return false;
            }

            return true;
        }

        if (field.Type == "filepicker")
        {
            var currentPath = (field.FilePath ?? string.Empty).Trim();

            if (field.Required && string.IsNullOrWhiteSpace(currentPath))
            {
                field.Error = $"{field.Label} is required.";
                return false;
            }

            if (!string.IsNullOrWhiteSpace(currentPath) && field.Accept.Count > 0)
            {
                var extension = System.IO.Path.GetExtension(currentPath);
                if (!field.Accept.Contains(extension, StringComparer.OrdinalIgnoreCase))
                {
                    field.Error = $"{field.Label} must be one of: {string.Join(", ", field.Accept)}";
                    return false;
                }
            }

            return true;
        }

        var text = (field.TextValue ?? string.Empty).Trim();

        if (field.Type == "date")
        {
            if (field.Required && field.DateValue == default)
            {
                field.Error = $"{field.Label} is required.";
                return false;
            }

            return true;
        }

        if (field.Required && string.IsNullOrWhiteSpace(text))
        {
            field.Error = $"{field.Label} is required.";
            return false;
        }

        if (string.IsNullOrWhiteSpace(text))
        {
            return true;
        }

        if (field.MinLength is not null && text.Length < field.MinLength)
        {
            field.Error = $"{field.Label} min length is {field.MinLength}.";
            return false;
        }

        if (field.MaxLength is not null && text.Length > field.MaxLength)
        {
            field.Error = $"{field.Label} max length is {field.MaxLength}.";
            return false;
        }

        if (!string.IsNullOrWhiteSpace(field.Regex))
        {
            try
            {
                if (!Regex.IsMatch(text, field.Regex))
                {
                    field.Error = $"{field.Label} format is invalid.";
                    return false;
                }
            }
            catch (ArgumentException)
            {
                field.Error = $"{field.Label} regex is invalid.";
                return false;
            }
        }

        if (field.Type == "number")
        {
            if (!double.TryParse(text, NumberStyles.Float, CultureInfo.InvariantCulture, out var numericValue) &&
                !double.TryParse(text, NumberStyles.Float, CultureInfo.CurrentCulture, out numericValue))
            {
                field.Error = $"{field.Label} must be a number.";
                return false;
            }

            if (field.Min is not null && numericValue < field.Min)
            {
                field.Error = $"{field.Label} must be >= {field.Min}.";
                return false;
            }

            if (field.Max is not null && numericValue > field.Max)
            {
                field.Error = $"{field.Label} must be <= {field.Max}.";
                return false;
            }
        }

        return true;
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
                     "key": "memo",
                     "label": "Memo",
                     "type": "textarea",
                     "placeholder": "Anything you want to mention",
                     "maxLength": 200
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

    public sealed partial class DynamicFieldState : ObservableObject
    {
        public DynamicFieldState(FieldSchema schema)
        {
            Key = schema.Key?.Trim() ?? Guid.NewGuid().ToString("N");
            Label = string.IsNullOrWhiteSpace(schema.Label) ? Key : schema.Label.Trim();
            Type = NormalizeType(schema.Type);
            Placeholder = schema.Placeholder;
            Required = schema.Required;
            MinLength = schema.MinLength;
            MaxLength = schema.MaxLength;
            Min = schema.Min;
            Max = schema.Max;
            Regex = schema.Regex;

            Options = new ObservableCollection<string>((schema.Options ?? []).Where(option => !string.IsNullOrWhiteSpace(option)));
            Accept = (schema.Accept ?? []).Where(ext => !string.IsNullOrWhiteSpace(ext)).ToList();

            TextValue = schema.DefaultValue;
            SelectedOption = string.IsNullOrWhiteSpace(schema.DefaultValue) ? null : schema.DefaultValue;
            BoolValue = bool.TryParse(schema.DefaultValue, out var boolValue) && boolValue;
            DateValue = DateTimeOffset.Now;
            FilePath = schema.DefaultValue ?? string.Empty;
            Error = string.Empty;
        }

        public string Key { get; }

        public string Label { get; }

        public string Type { get; }

        public string? Placeholder { get; }

        public bool Required { get; }

        public int? MinLength { get; }

        public int? MaxLength { get; }

        public double? Min { get; }

        public double? Max { get; }

        public string? Regex { get; }

        public ObservableCollection<string> Options { get; }

        public IReadOnlyList<string> Accept { get; }

        [ObservableProperty]
        private string? textValue;

        [ObservableProperty]
        private string? selectedOption;

        [ObservableProperty]
        private bool boolValue;

        [ObservableProperty]
        private DateTimeOffset dateValue;

        [ObservableProperty]
        private string filePath = string.Empty;

        [ObservableProperty]
        private string error = string.Empty;

        partial void OnTextValueChanged(string? value)
        {
            Error = string.Empty;
        }

        partial void OnSelectedOptionChanged(string? value)
        {
            Error = string.Empty;
        }

        partial void OnBoolValueChanged(bool value)
        {
            Error = string.Empty;
        }

        partial void OnDateValueChanged(DateTimeOffset value)
        {
            Error = string.Empty;
        }

        partial void OnFilePathChanged(string value)
        {
            Error = string.Empty;
        }

        private static string NormalizeType(string? type)
        {
            if (string.IsNullOrWhiteSpace(type))
            {
                return "input";
            }

            return type.Trim().ToLowerInvariant() switch
            {
                "text" => "input",
                "multiline" => "textarea",
                _ => type.Trim().ToLowerInvariant()
            };
        }
    }

    private sealed class TemplateSchema
    {
        public string? Title { get; set; }

        public List<FieldSchema>? Fields { get; set; }
    }

    public sealed class FieldSchema
    {
        public string? Key { get; set; }

        public string? Label { get; set; }

        public string? Type { get; set; }

        public string? Placeholder { get; set; }

        public bool Required { get; set; }

        public int? MinLength { get; set; }

        public int? MaxLength { get; set; }

        public double? Min { get; set; }

        public double? Max { get; set; }

        public string? Regex { get; set; }

        public string? DefaultValue { get; set; }

        public List<string>? Options { get; set; }

        public List<string>? Accept { get; set; }
    }
}
