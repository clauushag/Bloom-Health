using Plugin.LocalNotification;
using Plugin.LocalNotification.AndroidOption;
using Plugin.LocalNotification.Core.Models;
namespace app.Services;

public static class NotificacionService
{
    // IDs únicos para cada notificación
    private const int ID_RECORDATORIO_ANIMO = 1001;

    public static async Task InicializarAsync()
    {
        await LocalNotificationCenter.Current.RequestNotificationPermission();
        await LocalNotificationCenter.Current.RequestNotificationPermission();
    
        var notificacion = new NotificationRequest
        {
            NotificationId = ID_RECORDATORIO_ANIMO,
            Title          = "¿Cómo estás hoy? 🌱",
            Description    = "No olvides registrar tu estado anímico",
            Schedule       = new NotificationRequestSchedule
            {
                NotifyTime = DateTime.Now.AddSeconds(10), // 10 segundos tras cerrar
            }
        };

        await LocalNotificationCenter.Current.Show(notificacion);
    }

    private static async Task ProgramarRecordatorioAnimoAsync()
    {
        // Evita reprogramar si ya está puesta
        if (Preferences.Get("notif_animo_programada", false)) return;

        var notificacion = new NotificationRequest
        {
            NotificationId = ID_RECORDATORIO_ANIMO,
            Title          = "¿Cómo estás hoy? 🌱",
            Description    = "No olvides registrar tu estado anímico",
            Schedule       = new NotificationRequestSchedule
            {
                NotifyTime = DateTime.Now.AddMinutes(2), // Para pruebas, luego cambia a .AddDays(1) o el tiempo que quieras
                RepeatType = NotificationRepeat.Daily
            }
        };

        await LocalNotificationCenter.Current.Show(notificacion);
        Preferences.Set("notif_animo_programada", true);
    }

    // Llámalo si el usuario ya registró hoy (para no molestar)
    public static void CancelarRecordatorioAnimo()
    {
        LocalNotificationCenter.Current.Cancel(ID_RECORDATORIO_ANIMO);
        Preferences.Set("notif_animo_programada", false); // se reprograma mañana
    }
}