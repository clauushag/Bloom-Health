using app.Models;

using System.ComponentModel;
using System.Runtime.CompilerServices;

public class PerfilViewModel : INotifyPropertyChanged
{

    private Usuario _usuarioActual;
    public Usuario UsuarioActual
    {
        get => _usuarioActual;
        set
        {
            _usuarioActual = value;
            OnPropertyChanged();
        }
    }

    private Avatar _avatarActual;
    public Avatar AvatarActual
    {
        get => _avatarActual;
        set
        {
            _avatarActual = value;
            OnPropertyChanged(nameof(AvatarActual));
            OnPropertyChanged(nameof(ProgresoXP));   // ← actualiza la barra
            OnPropertyChanged(nameof(ImagenPlanta)); // ← actualiza la imagen
        }
    }

    // Añade estas dos propiedades si no las tienes ya
    public double ProgresoXP => AvatarActual != null ? Math.Min(AvatarActual.XP / 100.0, 1.0) : 0;

    public string ImagenPlanta
    {
        get
        {
            if (AvatarActual == null) return "planta_marchita.png";
            return AvatarActual.XP switch
            {
                < 100 => "planta_marchita.png",
                < 250 => "planta_debil.png",
                < 500 => "planta_normal.png",
                < 800 => "plantafuerte.png",
                _ => "planta_radiante.png"
            };
        }
    }


    public PerfilViewModel()
    {
        UsuarioActual = new Usuario();
        AvatarActual = new Avatar();
    }
    public event PropertyChangedEventHandler PropertyChanged;


    protected void OnPropertyChanged([CallerMemberName] string propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }



}
