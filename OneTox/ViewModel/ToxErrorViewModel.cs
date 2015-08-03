using System;
using System.Diagnostics;
using SharpTox.Av;
using SharpTox.Core;

namespace OneTox.ViewModel
{
    /// <summary>
    ///     Implements the Singleton pattern. (https://msdn.microsoft.com/en-us/library/ff650849.aspx)
    ///     The idea behind this class is that wherever a Tox-related error happens in ToxModel or ToxAvModel, it is
    ///     sent to this class. The application itself subscribes to this class' one event (see App.xaml.cs), which
    ///     will notify the app and present an user-readable error message that it can display to the user.
    ///     But in most cases, this class will simply log to the debug output.
    ///     This way, Tox related errors' handling's scope is reduced only to ToxModel and this ViewModel.
    /// </summary>
    public class ToxErrorViewModel
    {
        private static ToxErrorViewModel _instance;

        public static ToxErrorViewModel Instance
        {
            get { return _instance ?? (_instance = new ToxErrorViewModel()); }
        }

        public event EventHandler<string> ToxErrorOccured;

        private void RaiseToxErrorOccured(string errorMessage)
        {
            ToxErrorOccured?.Invoke(this, errorMessage);
        }

        #region Core errors

        public void RelayError(ToxErrorSendMessage error)
        {
            if (error != ToxErrorSendMessage.Ok && error != ToxErrorSendMessage.FriendNotConnected)
                RaiseToxErrorOccured("An unexpected error occurred when sending your message to your friend: " + error);
        }

        public void RelayError(ToxErrorFriendAdd error)
        {
            switch (error)
            {
                case ToxErrorFriendAdd.AlreadySent:
                    RaiseToxErrorOccured(
                        "Friend request is already sent or you already have this user on your friend list.");
                    return;
                case ToxErrorFriendAdd.OwnKey:
                    RaiseToxErrorOccured("This ID is yours. You can't add yourself to your friend list.");
                    return;
                case ToxErrorFriendAdd.SetNewNospam:
                    RaiseToxErrorOccured(
                        "You already have this user on your friend list, but with a different no spam value.");
                    return;
                case ToxErrorFriendAdd.TooLong:
                    RaiseToxErrorOccured("The friend request message is too long.");
                    return;
                default:
                    if (error != ToxErrorFriendAdd.Ok)
                        RaiseToxErrorOccured("An unexpected error occurred when handling a friend request: " + error);
                    return;
            }
        }

        public void RelayError(ToxErrorFriendDelete error)
        {
            if (error != ToxErrorFriendDelete.Ok)
                RaiseToxErrorOccured("An unexpected error occurred when deleting your friend: " + error);
        }

        public void RelayError(ToxErrorFileSendChunk error)
        {
            if (error != ToxErrorFileSendChunk.Ok)
                Debug.WriteLine("An unexpected error occurred when sending a file chunk: " + error);
        }

        public void RelayError(ToxErrorFileControl error)
        {
            if (error != ToxErrorFileControl.Ok)
                Debug.WriteLine("An unexpected error occurred when controlling a file transfer: " + error);
        }

        public void RelayError(ToxErrorFileSend error)
        {
            if (error != ToxErrorFileSend.Ok)
                Debug.WriteLine("An unexpected error occurred when sending a file: " + error);
        }

        public void RelayError(ToxErrorFileSeek error)
        {
            if (error != ToxErrorFileSeek.Ok)
                Debug.WriteLine("An unexpected error occurred when seeking in a file: " + error);
        }

        #endregion

        #region Audio/Video errors

        public void RelayError(ToxAvErrorCall error)
        {
            if (error != ToxAvErrorCall.Ok)
                Debug.WriteLine("An unexpected error occurred when calling a friend: " + error);
        }

        public void RelayError(ToxAvErrorAnswer error)
        {
            if (error != ToxAvErrorAnswer.Ok)
                Debug.WriteLine("An unexpected error occurred when answering a call: " + error);
        }

        public void RelayError(ToxAvErrorCallControl error)
        {
            if (error != ToxAvErrorCallControl.Ok)
                Debug.WriteLine("An unexpected error occurred when sending a call control: " + error);
        }

        public void RelayError(ToxAvErrorSetBitrate error)
        {
            if (error != ToxAvErrorSetBitrate.Ok)
                Debug.WriteLine("An unexpected error occurred when setting bitrate: " + error);
        }

        public void RelayError(ToxAvErrorSendFrame error)
        {
            if (error != ToxAvErrorSendFrame.Ok)
                Debug.WriteLine("An unexpected error occurred when sending a frame: " + error);
        }

        #endregion
    }
}