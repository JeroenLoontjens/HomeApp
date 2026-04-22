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

    // LEFT ICON BACKGROUND COLOR (badge)
    public static readonly BindableProperty LeftIconBackgroundColorProperty =
        BindableProperty.Create(
            nameof(LeftIconBackgroundColor),
            typeof(Microsoft.Maui.Graphics.Color),
            typeof(RowGrid),
            Microsoft.Maui.Graphics.Colors.DarkSlateBlue);

    public Microsoft.Maui.Graphics.Color LeftIconBackgroundColor
    {
        get => (Microsoft.Maui.Graphics.Color)GetValue(LeftIconBackgroundColorProperty);
        set => SetValue(LeftIconBackgroundColorProperty, value);
    }

    // MAIN TEXT
    public static readonly BindableProperty MainTextProperty = 
		BindableProperty.Create(nameof(MainText), typeof(string), typeof(RowGrid), default(string)); 
	public string MainText 
	{ 
		get => (string)GetValue(MainTextProperty); 
		set => SetValue(MainTextProperty, value); 
	}

    public static readonly BindableProperty MainFontSizeProperty =
    BindableProperty.Create(nameof(MainFontSize), typeof(double), typeof(RowGrid), 16d);

    public double MainFontSize
    {
        get => (double)GetValue(MainFontSizeProperty);
        set => SetValue(MainFontSizeProperty, value);
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

    public static readonly BindableProperty SubFontSizeProperty =
        BindableProperty.Create(nameof(SubFontSize), typeof(double), typeof(RowGrid), 12d);

    public double SubFontSize
    {
        get => (double)GetValue(SubFontSizeProperty);
        set => SetValue(SubFontSizeProperty, value);
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

    public static readonly BindableProperty RightFontSizeProperty =
    BindableProperty.Create(nameof(RightFontSize), typeof(double), typeof(RowGrid), 16d);

    public double RightFontSize
    {
        get => (double)GetValue(RightFontSizeProperty);
        set => SetValue(RightFontSizeProperty, value);
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

    //PADDING AND MARGIN PER ROW
    public static readonly BindableProperty RowPaddingProperty =
    BindableProperty.Create(nameof(RowPadding), typeof(Thickness), typeof(RowGrid), new Thickness(12));

    public Thickness RowPadding
    {
        get => (Thickness)GetValue(RowPaddingProperty);
        set => SetValue(RowPaddingProperty, value);
    }

    public static readonly BindableProperty RowMarginProperty =
        BindableProperty.Create(nameof(RowMargin), typeof(Thickness), typeof(RowGrid), new Thickness(0, 0, 0, 8));

    public Thickness RowMargin
    {
        get => (Thickness)GetValue(RowMarginProperty);
        set => SetValue(RowMarginProperty, value);
    }

    // ROW SPACING
    public static readonly BindableProperty RowSpacingProperty =
        BindableProperty.Create(nameof(RowSpacing), typeof(double), typeof(RowGrid), 2d);

    public double RowSpacing
    {
        get => (double)GetValue(RowSpacingProperty);
        set => SetValue(RowSpacingProperty, value);
    }

    // COLUMN SPACING (optioneel maar handig)
    public static readonly BindableProperty ColumnSpacingProperty =
        BindableProperty.Create(nameof(ColumnSpacing), typeof(double), typeof(RowGrid), 10d);

    public double ColumnSpacing
    {
        get => (double)GetValue(ColumnSpacingProperty);
        set => SetValue(ColumnSpacingProperty, value);
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