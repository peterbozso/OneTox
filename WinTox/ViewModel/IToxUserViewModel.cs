using Windows.UI.Xaml.Media.Imaging;
using SharpTox.Core;

namespace WinTox.ViewModel
{
    public interface IToxUserViewModel
    {
        BitmapImage Avatar { get; }
        string Name { get; }
        string StatusMessage { get; }
        ToxUserStatus Status { get; }
        bool IsConnected { get; }
    }
}