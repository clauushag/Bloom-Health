namespace app;

public partial class AppShell : Shell
{
	public AppShell()
	{
		InitializeComponent();
		Routing.RegisterRoute(nameof(Menu), typeof(Menu));
		Routing.RegisterRoute(nameof(crearPerfil), typeof(crearPerfil));
		Routing.RegisterRoute(nameof(MainPage), typeof(MainPage));
		Routing.RegisterRoute(nameof(PerfilPage), typeof(PerfilPage));
		Routing.RegisterRoute(nameof(ScannerPage), typeof(ScannerPage));
		Routing.RegisterRoute(nameof(RetosPage), typeof(RetosPage));

	}
}
