namespace unoDynamicUI.Presentation.DynamicForms;

public sealed class DynamicFormDialogRequestEventArgs : EventArgs
{
    public DynamicFormDialogRequestEventArgs(DynamicFormViewModel formViewModel)
    {
        FormViewModel = formViewModel;
    }

    public DynamicFormViewModel FormViewModel { get; }
}
