using Microsoft.Extensions.Logging;
using app.Data;
using ZXing.Net.Maui.Controls;

namespace app;

public static class MauiProgram
{
	public static MauiApp CreateMauiApp()
	{
		var builder = MauiApp.CreateBuilder();
		builder
			.UseMauiApp<App>()
			.UseBarcodeReader()
			.ConfigureFonts(fonts =>
			{
				fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
				fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
			});
		// 1. Configuramos la ruta del archivo SQLite
        string dbPath = Path.Combine(FileSystem.AppDataDirectory, "bloomhealth.db3");

		// Te imprimirá la ruta exacta en la consola de VS Code
		Console.WriteLine($"---> LA RUTA DE MI BASE DE DATOS ES: {dbPath}");

        // 2. Registramos la base de datos como un servicio
        builder.Services.AddSingleton(s => new SaludDatabase(dbPath));

        // 3. Registramos la página principal para poder inyectarle la base de datos
        builder.Services.AddTransient<MainPage>();
		builder.Services.AddTransient<Menu>();          
		builder.Services.AddTransient<NutricionPage>(); 

#if DEBUG
		builder.Logging.AddDebug();
#endif

		return builder.Build();
	}
}
