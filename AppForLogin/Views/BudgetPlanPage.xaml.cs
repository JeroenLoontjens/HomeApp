using AppForLogin.ViewModel;

namespace AppForLogin.Views;

public partial class BudgetPlanPage : ContentPage
{
    private readonly BudgetPlanPageViewModel _vm;

    public BudgetPlanPage(BudgetPlanPageViewModel vm)
    {
        InitializeComponent();
        BindingContext = vm;
        _vm = vm;

        SizeChanged += (_, __) => ApplyLayoutMode();
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();
        ApplyLayoutMode();
        _ = _vm.LoadAsync();
    }

    private void ApplyLayoutMode()
    {
        // simpele orientation check
        var horizontal = Width > Height && Width >= 720;

        var mode = horizontal ? BudgetPlanLayoutMode.Horizontal : BudgetPlanLayoutMode.Vertical;
        if (_vm.LayoutMode != mode)
            _vm.LayoutMode = mode;
    }
}