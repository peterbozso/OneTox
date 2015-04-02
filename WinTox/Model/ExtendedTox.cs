using SharpTox.Core;
using SharpTox.Encryption;

namespace WinTox.Model
{
    internal class ExtendedTox : Tox
    {
        public ExtendedTox(ToxOptions options)
            : base(options)
        {
        }

        public ExtendedTox(ToxOptions options, ToxData data = null, ToxEncryptionKey key = null) :
            base(options, data, key)
        {
        }

        public new int AddFriend(ToxId id, string message, out ToxErrorFriendAdd error)
        {
            var friendNumber = base.AddFriend(id, message, out error);
            if (error == ToxErrorFriendAdd.Ok)
                FriendAdded(friendNumber);
            return friendNumber;
        }

        public new int AddFriend(ToxId id, string message)
        {
            var friendNumber = base.AddFriend(id, message);
            FriendAdded(friendNumber);
            return friendNumber;
        }

        public new int AddFriendNoRequest(ToxKey publicKey, out ToxErrorFriendAdd error)
        {
            var friendNumber = base.AddFriendNoRequest(publicKey, out error);
            if (error == ToxErrorFriendAdd.Ok)
                FriendAdded(friendNumber);
            return friendNumber;
        }

        public new int AddFriendNoRequest(ToxKey publicKey)
        {
            var friendNumber = base.AddFriendNoRequest(publicKey);
            FriendAdded(friendNumber);
            return friendNumber;
        }

        public delegate void FriendAddedEventHandler(int friendNumber);

        public event FriendAddedEventHandler OnFriendAdded;

        private void FriendAdded(int friendNumber)
        {
            if (OnFriendAdded != null)
            {
                OnFriendAdded(friendNumber);
            }
        }
    }
}
