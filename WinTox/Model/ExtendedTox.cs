using SharpTox.Core;
using SharpTox.Encryption;

namespace WinTox.Model
{
    public class ExtendedTox : Tox
    {
        public delegate void FriendListModifiedEventHandler(
            int friendNumber, FriendListModificationType modificationType);

        public delegate void UserDataModifiedEventHandler();

        public enum FriendListModificationType
        {
            Add,
            Remove,
            Reset
        }

        public ExtendedTox(ToxOptions options)
            : base(options)
        {
        }

        public ExtendedTox(ToxOptions options, ToxData data = null, ToxEncryptionKey key = null) :
            base(options, data, key)
        {
        }

        public new string Name
        {
            get { return base.Name; }
            set
            {
                base.Name = value;
                UserDataModified();
            }
        }

        public new string StatusMessage
        {
            get { return base.StatusMessage; }
            set
            {
                base.StatusMessage = value;
                UserDataModified();
            }
        }

        public new ToxUserStatus Status
        {
            get { return base.Status; }
            set
            {
                base.Status = value;
                UserDataModified();
            }
        }

        public new int AddFriend(ToxId id, string message, out ToxErrorFriendAdd error)
        {
            var friendNumber = base.AddFriend(id, message, out error);
            if (error == ToxErrorFriendAdd.Ok)
                FriendListModified(friendNumber, FriendListModificationType.Add);
            return friendNumber;
        }

        public new int AddFriend(ToxId id, string message)
        {
            var friendNumber = base.AddFriend(id, message);
            FriendListModified(friendNumber, FriendListModificationType.Add);
            return friendNumber;
        }

        public new int AddFriendNoRequest(ToxKey publicKey, out ToxErrorFriendAdd error)
        {
            var friendNumber = base.AddFriendNoRequest(publicKey, out error);
            if (error == ToxErrorFriendAdd.Ok)
                FriendListModified(friendNumber, FriendListModificationType.Add);
            return friendNumber;
        }

        public new int AddFriendNoRequest(ToxKey publicKey)
        {
            var friendNumber = base.AddFriendNoRequest(publicKey);
            FriendListModified(friendNumber, FriendListModificationType.Add);
            return friendNumber;
        }

        public new bool DeleteFriend(int friendNumber, out ToxErrorFriendDelete error)
        {
            var success = base.DeleteFriend(friendNumber, out error);
            if (success)
            {
                FriendListModified(friendNumber, FriendListModificationType.Remove);
            }
            return success;
        }

        public new bool DeleteFriend(int friendNumber)
        {
            var success = base.DeleteFriend(friendNumber);
            if (success)
            {
                FriendListModified(friendNumber, FriendListModificationType.Remove);
            }
            return success;
        }

        public event FriendListModifiedEventHandler OnFriendListModified;

        private void FriendListModified(int friendNumber, FriendListModificationType modificationType)
        {
            if (OnFriendListModified != null)
            {
                OnFriendListModified(friendNumber, modificationType);
            }
        }

        public event UserDataModifiedEventHandler OnUserDataModified;

        private void UserDataModified()
        {
            if (OnUserDataModified != null)
                OnUserDataModified();
        }
    }
}