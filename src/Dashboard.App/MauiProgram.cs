using Dashboard.App.Platforms.Android.Services;
using Dashboard.Core.Abstractions;
using Dashboard.Core.Abstractions.Calendar;
using Dashboard.Core.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Dashboard.App;

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
			});

#if DEBUG
		builder.Configuration.AddUserSecrets<App>(optional: true);
		builder.Logging.AddDebug();
#endif

		builder.Services.AddSingleton<ISecureStorageWrapper, SecureStorageWrapper>();
		builder.Services.AddSingleton<SecureStorageTokenProvider>();

		builder.Services.AddSingleton<ICalendarContentReader, DefaultCalendarContentReader>();
		builder.Services.AddSingleton<ICalendarPermissionRequester, AndroidCalendarPermissionRequester>();
		builder.Services.AddCalendarService();

#if DEBUG
		builder.Services.AddSingleton<ITokenProvider>(sp => new CompositeTokenProvider(
			sp.GetRequiredService<SecureStorageTokenProvider>(),
			sp.GetRequiredService<IConfiguration>()));
#else
		builder.Services.AddSingleton<ITokenProvider>(sp => sp.GetRequiredService<SecureStorageTokenProvider>());
#endif

		return builder.Build();
	}
}
