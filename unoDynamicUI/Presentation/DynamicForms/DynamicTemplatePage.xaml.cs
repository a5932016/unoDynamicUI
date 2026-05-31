using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Data;
using Windows.Storage.Pickers;
using Windows.UI.Text;

namespace unoDynamicUI.Presentation.DynamicForms;

public sealed partial class DynamicTemplatePage : Page
{
    private DynamicTemplateViewModel? _viewModel;

    public DynamicTemplatePage()
    {
        this.InitializeComponent();
        DataContextChanged += OnDataContextChanged;
        Unloaded += OnUnloaded;
    }

    private void OnDataContextChanged(FrameworkElement sender, DataContextChangedEventArgs args)
    {
        WireViewModel(args.NewValue as DynamicTemplateViewModel);
    }

    private void OnUnloaded(object sender, RoutedEventArgs e)
    {
        WireViewModel(null);
    }

    private void WireViewModel(DynamicTemplateViewModel? viewModel)
    {
        if (_viewModel is not null)
        {
            _viewModel.TemplateChanged -= OnTemplateChanged;
        }

        _viewModel = viewModel;

        if (_viewModel is not null)
        {
            _viewModel.TemplateChanged += OnTemplateChanged;
            BuildTemplateView(_viewModel);
        }
        else
        {
            TemplateHost.Content = null;
        }
    }

    private void OnTemplateChanged(object? sender, EventArgs e)
    {
        if (_viewModel is not null)
        {
            BuildTemplateView(_viewModel);
        }
    }

    private void BuildTemplateView(DynamicTemplateViewModel viewModel)
    {
        var root = new StackPanel
        {
            Spacing = 12,
            Margin = new Thickness(16)
        };

        root.Children.Add(new TextBlock
        {
            Text = viewModel.FormTitle,
            FontSize = 26,
            FontWeight = FontWeights.SemiBold,
            TextWrapping = TextWrapping.WrapWholeWords
        });

        foreach (var field in viewModel.Fields)
        {
            root.Children.Add(CreateFieldControl(viewModel, field));
        }

        var actionPanel = new StackPanel
        {
            Orientation = Orientation.Horizontal,
            Spacing = 12,
            HorizontalAlignment = HorizontalAlignment.Right
        };

        actionPanel.Children.Add(new Button
        {
            Content = "Confirm",
            MinWidth = 120,
            Command = viewModel.ConfirmCommand
        });

        actionPanel.Children.Add(new Button
        {
            Content = "Cancel",
            MinWidth = 120,
            Command = viewModel.CancelCommand
        });

        root.Children.Add(actionPanel);
        TemplateHost.Content = root;
    }

    private static UIElement CreateFieldControl(DynamicTemplateViewModel viewModel, DynamicTemplateViewModel.DynamicFieldState field)
    {
        var wrapper = new StackPanel
        {
            Spacing = 6
        };

        wrapper.Children.Add(new TextBlock
        {
            Text = field.Required ? $"{field.Label} *" : field.Label,
            FontWeight = FontWeights.Medium
        });

        wrapper.Children.Add(CreateEditor(viewModel, field));

        var errorText = new TextBlock
        {
            Foreground = Application.Current.Resources.TryGetValue("SystemFillColorCriticalBrush", out var brush)
                ? brush as Microsoft.UI.Xaml.Media.Brush
                : null,
            TextWrapping = TextWrapping.WrapWholeWords,
            FontSize = 12
        };
        errorText.SetBinding(TextBlock.TextProperty, new Binding
        {
            Source = field,
            Path = new PropertyPath(nameof(DynamicTemplateViewModel.DynamicFieldState.Error)),
            Mode = BindingMode.OneWay
        });

        wrapper.Children.Add(errorText);
        return wrapper;
    }

    private static FrameworkElement CreateEditor(DynamicTemplateViewModel viewModel, DynamicTemplateViewModel.DynamicFieldState field)
    {
        switch (field.Type)
        {
            case "select":
                {
                    var comboBox = new ComboBox
                    {
                        ItemsSource = field.Options,
                        PlaceholderText = field.Placeholder ?? "Please choose"
                    };

                    comboBox.SetBinding(Selector.SelectedItemProperty, new Binding
                    {
                        Source = field,
                        Path = new PropertyPath(nameof(DynamicTemplateViewModel.DynamicFieldState.SelectedOption)),
                        Mode = BindingMode.TwoWay
                    });

                    return comboBox;
                }

            case "checkbox":
                {
                    var checkBox = new CheckBox
                    {
                        Content = field.Placeholder ?? "Checked"
                    };

                    checkBox.SetBinding(ToggleButton.IsCheckedProperty, new Binding
                    {
                        Source = field,
                        Path = new PropertyPath(nameof(DynamicTemplateViewModel.DynamicFieldState.BoolValue)),
                        Mode = BindingMode.TwoWay
                    });

                    return checkBox;
                }

            case "date":
                {
                    var datePicker = new DatePicker();

                    datePicker.SetBinding(DatePicker.DateProperty, new Binding
                    {
                        Source = field,
                        Path = new PropertyPath(nameof(DynamicTemplateViewModel.DynamicFieldState.DateValue)),
                        Mode = BindingMode.TwoWay
                    });

                    return datePicker;
                }

            case "filepicker":
                {
                    var host = new StackPanel
                    {
                        Orientation = Orientation.Horizontal,
                        Spacing = 8
                    };

                    var pathText = new TextBox
                    {
                        IsReadOnly = true,
                        Width = 360,
                        PlaceholderText = field.Placeholder ?? "No file selected"
                    };
                    pathText.SetBinding(TextBox.TextProperty, new Binding
                    {
                        Source = field,
                        Path = new PropertyPath(nameof(DynamicTemplateViewModel.DynamicFieldState.FilePath)),
                        Mode = BindingMode.OneWay
                    });

                    var browseButton = new Button
                    {
                        Content = "Browse",
                        MinWidth = 100
                    };

                    browseButton.Click += async (_, _) =>
                    {
                        try
                        {
                            var path = await PickFilePathAsync(field);
                            if (!string.IsNullOrWhiteSpace(path))
                            {
                                viewModel.SetPickedFile(field, path);
                            }
                        }
                        catch (Exception ex)
                        {
                            viewModel.SetStatus($"File picker failed: {ex.Message}");
                        }
                    };

                    host.Children.Add(pathText);
                    host.Children.Add(browseButton);
                    return host;
                }

            default:
                {
                    var textBox = new TextBox
                    {
                        PlaceholderText = field.Placeholder ?? string.Empty
                    };

                    if (field.Type == "textarea")
                    {
                        textBox.AcceptsReturn = true;
                        textBox.TextWrapping = TextWrapping.Wrap;
                        textBox.MinHeight = 100;
                    }

                    if (field.Type == "number")
                    {
                        var inputScope = new InputScope();
                        inputScope.Names.Add(new InputScopeName(InputScopeNameValue.Number));
                        textBox.InputScope = inputScope;
                    }

                    textBox.SetBinding(TextBox.TextProperty, new Binding
                    {
                        Source = field,
                        Path = new PropertyPath(nameof(DynamicTemplateViewModel.DynamicFieldState.TextValue)),
                        Mode = BindingMode.TwoWay,
                        UpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged
                    });

                    return textBox;
                }
        }
    }

    private static async Task<string?> PickFilePathAsync(DynamicTemplateViewModel.DynamicFieldState field)
    {
        var picker = new FileOpenPicker();

        if (field.Accept.Count > 0)
        {
            foreach (var ext in field.Accept)
            {
                picker.FileTypeFilter.Add(ext);
            }
        }
        else
        {
            picker.FileTypeFilter.Add("*");
        }

        var file = await picker.PickSingleFileAsync();
        return file?.Path;
    }
}
