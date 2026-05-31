using Microsoft.UI;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Media;

namespace unoDynamicUI.Presentation;

public sealed partial class MainPage : Page
{
    private MainViewModel? _viewModel;

    public MainPage()
    {
        this.InitializeComponent();
        DataContextChanged += OnDataContextChanged;
        Unloaded += OnUnloaded;
    }

    private void OnDataContextChanged(FrameworkElement sender, DataContextChangedEventArgs args)
    {
        WireViewModel(args.NewValue as MainViewModel);
    }

    private void OnUnloaded(object sender, RoutedEventArgs e)
    {
        WireViewModel(null);
    }

    private void WireViewModel(MainViewModel? viewModel)
    {
        if (_viewModel is not null)
        {
            _viewModel.TemplateDialogRequested -= OnTemplateDialogRequested;
        }

        _viewModel = viewModel;

        if (_viewModel is not null)
        {
            _viewModel.TemplateDialogRequested += OnTemplateDialogRequested;
        }
    }

    private async void OnTemplateDialogRequested(object? sender, DynamicTemplateRequestedEventArgs e)
    {
        await ShowTemplateDialogAsync(e.ViewModel);
    }

    private async Task ShowTemplateDialogAsync(DynamicTemplateViewModel viewModel)
    {
        var dialog = new ContentDialog
        {
            Content = DynamicTemplateFormRenderer.Build(viewModel)
        };

        if (XamlRoot is not null)
        {
            dialog.XamlRoot = XamlRoot;
        }

        EventHandler cancelledHandler = (_, _) => dialog.Hide();
        EventHandler<IReadOnlyDictionary<string, object?>> confirmedHandler = (_, _) => dialog.Hide();

        viewModel.Cancelled += cancelledHandler;
        viewModel.Confirmed += confirmedHandler;

        try
        {
            await dialog.ShowAsync();
        }
        finally
        {
            viewModel.Cancelled -= cancelledHandler;
            viewModel.Confirmed -= confirmedHandler;
        }
    }
}
