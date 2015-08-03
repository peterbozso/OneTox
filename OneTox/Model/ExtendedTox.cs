using System;
using SharpTox.Core;
using SharpTox.Encryption;

namespace OneTox.Model
{
    public class ExtendedTox : Tox
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
                FriendListChanged(friendNumber, FriendListChangedAction.Add);
            return friendNumber;
        }

        public new int AddFriend(ToxId id, string message)
        {
            var friendNumber = base.AddFriend(id, message);
            FriendListChanged(friendNumber, FriendListChangedAction.Add);
            return friendNumber;
        }

        public new int AddFriendNoRequest(ToxKey publicKey, out ToxErrorFriendAdd error)
        {
            var friendNumber = base.AddFriendNoRequest(publicKey, out error);
            if (error == ToxErrorFriendAdd.Ok)
                FriendListChanged(friendNumber, FriendListChangedAction.Add);
            return friendNumber;
        }

        public new int AddFriendNoRequest(ToxKey publicKey)
        {
            var friendNumber = base.AddFriendNoRequest(publicKey);
            FriendListChanged(friendNumber, FriendListChangedAction.Add);
            return friendNumber;
        }

        public new bool DeleteFriend(int friendNumber, out ToxErrorFriendDelete error)
        {
            var success = base.DeleteFriend(friendNumber, out error);
            if (success)
            {
                FriendListChanged(friendNumber, FriendListChangedAction.Remove);
            }
            return success;
        }

        public new bool DeleteFriend(int friendNumber)
        {
            var success = base.DeleteFriend(friendNumber);
            if (success)
            {
                FriendListChanged(friendNumber, FriendListChangedAction.Remove);
            }
            return success;
        }

        public event EventHandler<FriendListChangedEventArgs> OnFriendListChanged;

        private void FriendListChanged(int friendNumber, FriendListChangedAction action)
        {
            OnFriendListChanged?.Invoke(this,
                new FriendListChangedEventArgs {FriendNumber = friendNumber, Action = action});
        }
    }
}