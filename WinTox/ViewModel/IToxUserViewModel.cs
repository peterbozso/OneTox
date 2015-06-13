using Windows.UI.Xaml.Media.Imaging;

namespace WinTox.ViewModel
{
    public enum ExtendedToxUserStatus
    {
        Available = 0,
        Away = 1,
        Busy = 2,
        Offline = 3
    }

    public interface IToxUserViewModel
    {
        BitmapImage Avatar { get; }
        string Name { get; }
        string StatusMessage { get; }
        ExtendedToxUserStatus Status { get; }
        bool IsConnected { get; }
    }
}