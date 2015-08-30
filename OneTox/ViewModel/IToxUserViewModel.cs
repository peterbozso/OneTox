using Windows.UI.Xaml.Media.Imaging;

namespace OneTox.ViewModel
{
    public interface IToxUserViewModel
    {
        BitmapImage Avatar { get; }
        bool IsConnected { get; }
        string Name { get; }
        ExtendedToxUserStatus Status { get; }
        string StatusMessage { get; }
    }

    public enum ExtendedToxUserStatus
    {
        Available = 0,
        Away = 1,
        Busy = 2,
        Offline = 3
    }
}