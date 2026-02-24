using AppForLogin.ViewModel.Generic;

namespace AppForLogin.Views.Generic;

public partial class CategoryTreeView : ContentView
{
	public CategoryTreeView()
	{
		InitializeComponent();
	}

    protected override void OnBindingContextChanged()
    {
        base.OnBindingContextChanged();

        if (BindingContext is CategoryViewModel vm)
        {
            vm.PropertyChanged += async (s, e) =>
            {
                if (e.PropertyName == nameof(CategoryViewModel.IsExpanded))
                {
                    if (vm.IsExpanded)
                    {
                        this.FadeTo(1, 200, Easing.CubicIn);
                    }
                    else
                    {
                        this.FadeTo(0.7, 150, Easing.CubicOut);
                    }
                }
            };
        }
    }

}