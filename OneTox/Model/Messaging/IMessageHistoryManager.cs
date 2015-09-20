using System.Collections.Generic;
using OneTox.ViewModel.Friends;
using OneTox.ViewModel.Messaging;

namespace OneTox.Model.Messaging
{
    public interface IMessageHistoryManager
    {
        List<ToxMessageViewModelBase> GetMessageHistoryForFriend(FriendViewModel friend);
    }
}