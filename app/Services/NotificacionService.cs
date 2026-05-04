using Plugin.LocalNotification;
using Plugin.LocalNotification.AndroidOption;
using Plugin.LocalNotification.Core.Models;
namespace app.Services;

public static class NotificacionService
{
    // IDs únicos para cada notificación
    private const int ID_RECORDATORIO_ANIMO = 1001;
    private const int ID_RECORDATORIO_MENSTRUACION = 1002;

    public static async Task InicializarAsync()
    {
        await LocalNotificationCenter.Current.RequestNotificationPermission();
    
        _=ProgramarRecordatorioAnimoAsync();

    }

    private static async Task ProgramarRecordatorioAnimoAsync()
    {
        // Evita reprogramar si ya está puesta
        if (Preferences.Get("notif_animo_programada", false)) return;

        NotificationRequest notificacion = new NotificationRequest
        {
            NotificationId = ID_RECORDATORIO_ANIMO,
            Title          = "¿Cómo estás hoy? 🌱",
            Description    = "No olvides registrar tu estado anímico",
            Schedule       = new NotificationRequestSchedule
            {
                NotifyTime = DateTime.Parse("09:00:00"), //todos los dias a las 9 am
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

    public static async Task ProgramarRecordatorioMensatruacionAsync(int diasHastaProximoPeriodo)
    {
        NotificationRequest notification = new NotificationRequest
        {
            NotificationId = ID_RECORDATORIO_MENSTRUACION,
            Title          = "¿Tu periodo comenzó? 🌸",
            Description    = "Registra el inicio de tu menstruación para un mejor seguimiento",
            Schedule       = new NotificationRequestSchedule
            {
                NotifyTime = DateTime.Now.AddDays(diasHastaProximoPeriodo)// cada X días (ajustable según ciclo)
            }
        };
        await LocalNotificationCenter.Current.Show(notification);
    }
}