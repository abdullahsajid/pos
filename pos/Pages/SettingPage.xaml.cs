namespace pos.Pages;
using pos.Data;
using pos.ViewModels;

public partial class SettingPage : ContentPage
{
	private readonly DB_Services _dbServices;
    public SettingPage(SettingsModel settingModel,DB_Services dbService)
	{
		InitializeComponent();
		BindingContext = settingModel;
		_dbServices = dbService;
		Loaded += MainPage_Loaded;
    }

	private async void MainPage_Loaded(object sender, EventArgs e)
	{
		await (BindingContext as SettingsModel).InitializeAsync();
    }	
}