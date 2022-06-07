namespace MauiApp1;

public partial class MainPage : ContentPage
{
	int count = 0;

	public MainPage()
	{
		InitializeComponent();
	}

	private void OnCounterClicked(object sender, EventArgs e)
	{
		count++;

		if (count == 1)
			CounterBtn.Text = $"Clicked {count} time";
		else
			CounterBtn.Text = $"Clicked {count} times";

		SemanticScreenReader.Announce(CounterBtn.Text);
	}
    private void startSearch(object sender, EventArgs e)
    {
        var abc = new ResultPage(seartchWord.Text);
        //Navigation.PushAsync(abc,true);
        Application.Current.MainPage = abc;
        //Navigation.PushAsync(new ResultPage(seartchWord.Text));
    }
}

