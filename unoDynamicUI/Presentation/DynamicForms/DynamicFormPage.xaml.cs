using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;

namespace unoDynamicUI.Presentation.DynamicForms;

public sealed partial class DynamicFormPage : Page
{
    public DynamicFormPage()
    {
        this.InitializeComponent();
        DataContextChanged += OnDataContextChanged;
    }

    private void OnDataContextChanged(FrameworkElement sender, DataContextChangedEventArgs args)
    {
        if (args.NewValue is DynamicFormPageViewModel viewModel)
        {
            FormHost.Content = DynamicFormRenderer.Build(viewModel.FormViewModel);
        }
    }
}
