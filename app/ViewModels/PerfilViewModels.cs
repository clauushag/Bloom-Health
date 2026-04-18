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
            OnPropertyChanged();
        }
    }
    
    public double ProgresoXP
    {
        get
        {
            if (AvatarActual == null)
                return 0;

            return AvatarActual.XP / 100.0;
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
