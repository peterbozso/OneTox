using System;
using System.Collections.ObjectModel;
using SharpTox.Core;
using WinTox.Model;

namespace WinTox.ViewModel.FriendRequests
{
    /// <summary>
    ///     Implements the Singleton pattern. (https://msdn.microsoft.com/en-us/library/ff650849.aspx)
    /// </summary>
    public class FriendRequestsViewModel
    {
        public enum FriendRequestAnswer
        {
            Accept,
            Decline,
            Later
        }

        private static FriendRequestsViewModel _instance;

        private FriendRequestsViewModel()
        {
            FriendRequests = new ObservableCollection<OneFriendRequestViewModel>();
            ToxModel.Instance.FriendRequestReceived += FriendRequestReceivedHandler;
        }

        public static FriendRequestsViewModel Instance
        {
            get { return _instance ?? (_instance = new FriendRequestsViewModel()); }
        }

        public ObservableCollection<OneFriendRequestViewModel> FriendRequests { get; private set; }
        public event EventHandler<ToxEventArgs.FriendRequestEventArgs> FriendRequestReceived;

        public void HandleFriendRequestAnswer(FriendRequestAnswer answer, ToxEventArgs.FriendRequestEventArgs e)
        {
            switch (answer)
            {
                case FriendRequestAnswer.Accept:
                    ToxModel.Instance.AddFriendNoRequest(e.PublicKey);
                    return;
                case FriendRequestAnswer.Decline:
                    return;
                case FriendRequestAnswer.Later:
                    FriendRequests.Add(new OneFriendRequestViewModel(e.PublicKey, e.Message));
                    return;
            }
        }

        private void FriendRequestReceivedHandler(object sender, ToxEventArgs.FriendRequestEventArgs e)
        {
            if (FriendRequestReceived != null)
                FriendRequestReceived(sender, e);
        }
    }
}