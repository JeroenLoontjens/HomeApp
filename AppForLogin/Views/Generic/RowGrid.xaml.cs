using AppForLogin.ViewModel.Generic;
using System.Windows.Input;
using System.Diagnostics;

namespace AppForLogin.Views.Generic;

public partial class RowGrid : ContentView
{
	public RowGrid()
	{
		InitializeComponent();
	}

    // LEFT ICON
    public static readonly BindableProperty LeftIconProperty =
        BindableProperty.Create(nameof(LeftIcon), typeof(string), typeof(RowGrid), default(string), propertyChanged: OnLeftIconChanged);


    public string LeftIcon
    {
        get => (string)GetValue(LeftIconProperty);
        set => SetValue(LeftIconProperty, value);
    }

    public bool HasLeftIcon => !string.IsNullOrWhiteSpace(LeftIcon);

    static void OnLeftIconChanged(BindableObject bindable, object oldValue, object newValue)
    {
        ((RowGrid)bindable).OnPropertyChanged(nameof(HasLeftIcon));
    }
	
	// MAIN TEXT
	public static readonly BindableProperty MainTextProperty = 
		BindableProperty.Create(nameof(MainText), typeof(string), typeof(RowGrid), default(string)); 
	public string MainText 
	{ 
		get => (string)GetValue(MainTextProperty); 
		set => SetValue(MainTextProperty, value); 
	} 
	
	// SUBTEXT
    public static readonly BindableProperty SubTextProperty =
        BindableProperty.Create(nameof(SubText), typeof(string), typeof(RowGrid), default(string), propertyChanged: OnSubTextChanged);

    public string SubText
    {
        get => (string)GetValue(SubTextProperty);
        set => SetValue(SubTextProperty, value);
    }

    public bool HasSubText => !string.IsNullOrWhiteSpace(SubText);

    static void OnSubTextChanged(BindableObject bindable, object oldValue, object newValue)
    {
        // Debug trace so you can verify when the control receives the value and what it is
        //Debug.WriteLine($"RowGrid: OnSubTextChanged old='{oldValue ?? "null"}' new='{newValue ?? "null"}' HasSubText={(string.IsNullOrWhiteSpace(newValue as string) ? "false" : "true")}");
        ((RowGrid)bindable).OnPropertyChanged(nameof(HasSubText));
    }
	
	// RIGHT TEXT
    public static readonly BindableProperty RightTextProperty =
        BindableProperty.Create(nameof(RightText), typeof(string), typeof(RowGrid), default(string), propertyChanged: OnRightTextChanged);

    public string RightText
    {
        get => (string)GetValue(RightTextProperty);
        set => SetValue(RightTextProperty, value);
    }

    public bool HasRightText => !string.IsNullOrWhiteSpace(RightText);

    static void OnRightTextChanged(BindableObject bindable, object oldValue, object newValue)
    {
        ((RowGrid)bindable).OnPropertyChanged(nameof(HasRightText));
    }
	
	// INDENT LEVEL (voor categorieën)
    public static readonly BindableProperty IndentLevelProperty =
        BindableProperty.Create(nameof(IndentLevel), typeof(int), typeof(RowGrid), 0, propertyChanged: OnIndentLevelChanged);

    public int IndentLevel
    {
        get => (int)GetValue(IndentLevelProperty);
        set => SetValue(IndentLevelProperty, value);
    }

    public Thickness IndentMargin => new Thickness(IndentLevel * 16, 0, 0, 0);

    static void OnIndentLevelChanged(BindableObject bindable, object oldValue, object newValue)
    {
        ((RowGrid)bindable).OnPropertyChanged(nameof(IndentMargin));
    }
	
	// COMMAND
    public static readonly BindableProperty CommandProperty =
        BindableProperty.Create(nameof(Command), typeof(ICommand), typeof(RowGrid));

    public ICommand Command
    {
        get => (ICommand)GetValue(CommandProperty);
        set => SetValue(CommandProperty, value);
    }

	public static readonly BindableProperty CommandParameterProperty = 
		BindableProperty.Create(nameof(CommandParameter), typeof(object), typeof(RowGrid)); 
	
	public object CommandParameter 
	{
		get => GetValue(CommandParameterProperty); 
		set => SetValue(CommandParameterProperty, value); 
	} 

}