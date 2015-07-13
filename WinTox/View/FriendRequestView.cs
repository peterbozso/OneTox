using System;
using Windows.ApplicationModel.Core;
using Windows.UI.Core;
using Windows.UI.Popups;
using SharpTox.Core;
using WinTox.ViewModel.FriendRequests;

namespace WinTox.View
{
    /// <summary>
    ///     TODO: We shouldn't show a full-screen MessageDialog every time the user receives a friend request.
    ///     We should show a notification and when the user clicks/taps on it, only then we should show the dialog!
    /// </summary>
    public class FriendRequestView
    {
        public FriendRequestView()
        {
            FriendRequestsViewModel.Instance.FriendRequestReceived += FriendRequestReceivedHandler;
        }

        private async void FriendRequestReceivedHandler(object sender, ToxEventArgs.FriendRequestEventArgs e)
        {
            await CoreApplication.MainView.CoreWindow.Dispatcher.RunAsync(CoreDispatcherPriority.Normal, async () =>
            {
                var message = "From: " + e.PublicKey + "\n" + "Message: " + e.Message;
                var msgDialog = new MessageDialog(message, "Friend request received");
                msgDialog.Commands.Add(new UICommand("Accept", null, FriendRequestsViewModel.FriendRequestAnswer.Accept));
                msgDialog.Commands.Add(new UICommand("Decline", null,
                    FriendRequestsViewModel.FriendRequestAnswer.Decline));
                msgDialog.Commands.Add(new UICommand("Later", null, FriendRequestsViewModel.FriendRequestAnswer.Later));
                var answer = await msgDialog.ShowAsync();
                FriendRequestsViewModel.Instance.HandleFriendRequestAnswer(
                    (FriendRequestsViewModel.FriendRequestAnswer) answer.Id, e);
            });
        }
    }
}