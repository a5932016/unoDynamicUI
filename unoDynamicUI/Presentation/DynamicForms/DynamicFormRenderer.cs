using Microsoft.UI;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Media;
using Windows.UI.Text;

namespace unoDynamicUI.Presentation.DynamicForms;

public static class DynamicFormRenderer
{
    public static FrameworkElement Build(DynamicFormViewModel viewModel)
    {
        var root = new StackPanel
        {
            Spacing = 12,
            Margin = new Thickness(16)
        };

        root.Children.Add(new TextBlock
        {
            Text = viewModel.Title,
            FontSize = 28,
            FontWeight = FontWeights.SemiBold,
            TextWrapping = TextWrapping.WrapWholeWords
        });

        foreach (var field in viewModel.Fields)
        {
            root.Children.Add(CreateField(field));
        }

        var summaryText = new TextBlock
        {
            Foreground = new SolidColorBrush(Colors.IndianRed),
            TextWrapping = TextWrapping.WrapWholeWords
        };
        summaryText.SetBinding(TextBlock.TextProperty, new Binding
        {
            Source = viewModel,
            Path = new PropertyPath(nameof(DynamicFormViewModel.ValidationSummary)),
            Mode = BindingMode.OneWay
        });
        root.Children.Add(summaryText);

        var actionPanel = new StackPanel
        {
            Orientation = Orientation.Horizontal,
            Spacing = 12,
            HorizontalAlignment = HorizontalAlignment.Right,
            Margin = new Thickness(0, 8, 0, 0)
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

        return new ScrollViewer
        {
            Content = root,
            VerticalScrollBarVisibility = ScrollBarVisibility.Auto
        };
    }

    private static UIElement CreateField(DynamicFieldInputViewModel field)
    {
        var fieldPanel = new StackPanel
        {
            Spacing = 6
        };

        fieldPanel.Children.Add(new TextBlock
        {
            Text = field.Label,
            FontWeight = FontWeights.Medium
        });

        fieldPanel.Children.Add(CreateEditor(field));

        var errorText = new TextBlock
        {
            Foreground = new SolidColorBrush(Colors.IndianRed),
            FontSize = 12,
            TextWrapping = TextWrapping.WrapWholeWords
        };
        errorText.SetBinding(TextBlock.TextProperty, new Binding
        {
            Source = field,
            Path = new PropertyPath(nameof(DynamicFieldInputViewModel.ValidationMessage)),
            Mode = BindingMode.OneWay
        });

        fieldPanel.Children.Add(errorText);
        return fieldPanel;
    }

    private static FrameworkElement CreateEditor(DynamicFieldInputViewModel field)
    {
        if (field.Type == DynamicFieldType.Boolean)
        {
            var checkBox = new CheckBox
            {
                Content = field.Placeholder ?? "Enabled"
            };

            checkBox.SetBinding(ToggleButton.IsCheckedProperty, new Binding
            {
                Source = field,
                Path = new PropertyPath(nameof(DynamicFieldInputViewModel.IsChecked)),
                Mode = BindingMode.TwoWay
            });

            return checkBox;
        }

        var textBox = new TextBox
        {
            PlaceholderText = field.Placeholder ?? string.Empty
        };

        if (field.Type == DynamicFieldType.MultilineText)
        {
            textBox.AcceptsReturn = true;
            textBox.TextWrapping = TextWrapping.Wrap;
            textBox.MinHeight = 96;
        }

        if (field.Type == DynamicFieldType.Number)
        {
            var inputScope = new InputScope();
            inputScope.Names.Add(new InputScopeName(InputScopeNameValue.Number));
            textBox.InputScope = inputScope;
        }

        textBox.SetBinding(TextBox.TextProperty, new Binding
        {
            Source = field,
            Path = new PropertyPath(nameof(DynamicFieldInputViewModel.InputText)),
            Mode = BindingMode.TwoWay,
            UpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged
        });

        return textBox;
    }
}
