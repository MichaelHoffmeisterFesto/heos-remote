using System.Collections.ObjectModel;
using System.Windows.Input;

namespace heos_maui_app;

public class ContextMenuItem
{
    public string Title { get; set; } = "";
    public string? ImageUrl { get; set; } = null;
    public object? Tag { get; set; } = null;
    public bool ClosePage { get; set; } = false;
}

public partial class ContextMenuPage : ContentPage
{
    public bool Result { get; set; } = false;
    public ContextMenuItem? ResultItem { get; set; } = null;

    public string Caption { get; set; } = "Caption";

    public ObservableCollection<ContextMenuItem> Items { get; set; } = new ObservableCollection<ContextMenuItem>();

    public ICommand RemoveEquipmentCommand => new Command<object>(CollectionButton_Clicked);

    public ContextMenuPage()
	{
		InitializeComponent();
        this.BindingContext = this;
	}

    public ContextMenuPage(string caption, IEnumerable<ContextMenuItem> items)
    {
        Caption = caption;
        Items = new ObservableCollection<ContextMenuItem>(items);
        InitializeComponent();
        this.BindingContext = this;
    }

    public ContextMenuPage(string caption, params string[] itemTitles)
    {
        Caption = caption;
        Items = new ObservableCollection<ContextMenuItem>(itemTitles.Select(x => new ContextMenuItem() { Title = x }));
        InitializeComponent();
        this.BindingContext = this;
    }

    private async void Button_Clicked(object sender, EventArgs e)
    {
        if (sender == ButtonBack)
        {
            Result = false;
            await Navigation.PopModalAsync();
        }
    }

    private async void CollectionButton_Clicked(object tag)
    {
        foreach (var item in Items)
        {
            if (item.Tag == tag)
            {
                Result = true;
                ResultItem = item;
                await Navigation.PopModalAsync();
                return;
            }
        }
    }


}