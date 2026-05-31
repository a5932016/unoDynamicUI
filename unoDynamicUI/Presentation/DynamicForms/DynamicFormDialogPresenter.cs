using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;

namespace unoDynamicUI.Presentation.DynamicForms;

public static class DynamicFormDialogPresenter
{
    public static async Task ShowAsync(FrameworkElement host, DynamicFormViewModel formViewModel)
    {
        var dialog = new ContentDialog
        {
            Content = DynamicFormRenderer.Build(formViewModel)
        };

        if (host.XamlRoot is not null)
        {
            dialog.XamlRoot = host.XamlRoot;
        }

        void CloseDialog(object? sender, EventArgs args)
        {
            dialog.Hide();
        }

        formViewModel.Confirmed += CloseDialog;
        formViewModel.Cancelled += CloseDialog;

        try
        {
            await dialog.ShowAsync();
        }
        finally
        {
            formViewModel.Confirmed -= CloseDialog;
            formViewModel.Cancelled -= CloseDialog;
        }
    }
}
