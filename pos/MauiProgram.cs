using CommunityToolkit.Maui;
using Microsoft.Extensions.Logging;
using pos.Data;
using pos.Pages;
using pos.Popups;
using pos.ViewModels;

namespace pos;

public static class MauiProgram
{
	public static MauiApp CreateMauiApp()
	{
		var builder = MauiApp.CreateBuilder();
		builder
			.UseMauiApp<App>()
            .ConfigureFonts(fonts =>
			{
				fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
				fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
			})
			.UseMauiCommunityToolkit();

#if DEBUG
        builder.Logging.AddDebug();
#endif
		builder.Services.AddSingleton<DB_Services>();
        builder.Services.AddSingleton<HomeViewModel>();
        builder.Services.AddSingleton<MainPage>();
		builder.Services.AddSingleton<AddProductModel>();
		builder.Services.AddSingleton<AddProductPage>();
        builder.Services.AddSingleton<OrderModel>();
        builder.Services.AddSingleton<OrderPage>();
        builder.Services.AddTransientPopup<OrderItemPopup, OrderModel>();
        //builder.Services.AddSingleton<OrderItemPopup>();

        return builder.Build();
	}
}
