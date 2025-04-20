namespace heos_maui_app;

public partial class SetupPage : ContentPage
{
    public string Text { get; set; } = "Hello, World!";
    public bool Result { get; set; } = false;

    public SetupPage()
	{
		InitializeComponent();
	}

    private async void Button_Clicked(object sender, EventArgs e)
    {
        if (sender == ButtonReset)
        {
            using var stream = await FileSystem.OpenAppPackageFileAsync("reset_heos_config.json");
            using var reader = new StreamReader(stream);

            var contents = reader.ReadToEnd();

            Editor.Text = contents;
        }

        if (sender == ButtonCancel)
        {
            Result = false;
            await Navigation.PopModalAsync();
        }

        if (sender == ButtonSave)
        {
            Result = true;
            Text = Editor.Text;
            await Navigation.PopModalAsync();
        }
    }

    private void ContentPage_Loaded(object sender, EventArgs e)
    {
        Editor.Text = Text;
    }
}