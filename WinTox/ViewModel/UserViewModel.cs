using SharpTox.Core;
using WinTox.Model;

namespace WinTox.ViewModel
{
    internal class UserViewModel
    {
        public string Name
        {
            get { return ToxSingletonModel.Instance.Name; }
        }

        public string StatusMessage
        {
            get { return ToxSingletonModel.Instance.StatusMessage; }
        }

        public ToxUserStatus Status
        {
            get { return ToxSingletonModel.Instance.Status; }
        }

        public bool IsOnline
        {
            // TODO: Bind to it in the View!
            get { return ToxSingletonModel.Instance.IsConnected; }
        }
    }
}
