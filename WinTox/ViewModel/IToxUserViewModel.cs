using SharpTox.Core;

namespace WinTox.ViewModel
{
    public interface IToxUserViewModel
    {
        string Name { get; }
        string StatusMessage { get; }
        ToxUserStatus Status { get; }
        bool IsOnline { get; }
    }
}