using System;
using SharpTox.Core;

namespace WinTox.ViewModel
{
    /// <summary>
    ///     Implements the Singleton pattern. (https://msdn.microsoft.com/en-us/library/ff650849.aspx)
    ///     The idea behind this class is that wherever a Tox-related error happens in ToxModel, it is sent to this class.
    ///     The application itself subsribes to this class' one event (see App.xaml.cs), which will notify the app and
    ///     present an user-readable error message that it can display to the user.
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

        public void RelayError(ToxErrorSendMessage error)
        {
            if (error != ToxErrorSendMessage.Ok && error != ToxErrorSendMessage.FriendNotConnected)
                RaiseToxErrorOccured("An unexpected error occured when sending your message to your friend: " + error);
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
                        RaiseToxErrorOccured("An unexpected error occured when handling a friend request: " + error);
                    return;
            }
        }

        public void RelayError(ToxErrorFriendDelete error)
        {
            if (error != ToxErrorFriendDelete.Ok)
                RaiseToxErrorOccured("An unexpected error occured when deleting your friend: " + error);
        }

        public void RelayError(ToxErrorFileSendChunk error)
        {
            /*
            if (error != ToxErrorFileSendChunk.Ok)
                RaiseToxErrorOccured("An unexpected error occured when sending a file chunk: " + error);
            */
        }

        public void RelayError(ToxErrorFileControl error)
        {
            /*
            if (error != ToxErrorFileControl.Ok)
                RaiseToxErrorOccured("An unexpected error occured when trying to control a file transfer: " + error);
            */
        }

        public void RelayError(ToxErrorFileSend error)
        {
            /*
            switch (error)
            {
                case ToxErrorFileSend.NameTooLong:
                    RaiseToxErrorOccured("The file's name you just tried to send is too long.");
                    return;
                case ToxErrorFileSend.TooMany:
                    RaiseToxErrorOccured("There are too many ongoing file transfers for this friend.");
                    return;
                default:
                    if (error != ToxErrorFileSend.Ok)
                        RaiseToxErrorOccured("An unexpected error occured when trying to send a file: " + error);
                    return;
            }
            */
        }

        private void RaiseToxErrorOccured(string errorMessage)
        {
            if (ToxErrorOccured != null)
                ToxErrorOccured(this, errorMessage);
        }
    }
}