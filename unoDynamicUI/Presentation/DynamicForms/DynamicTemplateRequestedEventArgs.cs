namespace unoDynamicUI.Presentation.DynamicForms;

public sealed class DynamicTemplateRequestedEventArgs : EventArgs
{
    public DynamicTemplateRequestedEventArgs(DynamicTemplateViewModel viewModel)
    {
        ViewModel = viewModel;
    }

    public DynamicTemplateViewModel ViewModel { get; }
}
