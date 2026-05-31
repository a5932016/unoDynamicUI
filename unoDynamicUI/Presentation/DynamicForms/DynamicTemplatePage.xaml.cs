using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;

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
        TemplateHost.Content = DynamicTemplateFormRenderer.Build(viewModel, new Thickness(16));
    }
}
