using System;
using System.Collections.Generic;
using OneTox.Config;
using OneTox.ViewModel.Friends;
using OneTox.ViewModel.Messaging;
using SharpTox.Core;

namespace OneTox.Model.Messaging
{
    public class MockMessageHistoryManager : IMessageHistoryManager
    {
        private const int KMessageNum = 10;

        private readonly string[] _messageTexts = new string[KMessageNum]
        {
            "Lorem ipsum dolor sit amet, consectetur adipiscing elit.",
            "Donec a libero sit amet purus pellentesque eleifend quis eu massa.",
            "Integer in mi posuere est efficitur lobortis.",
            "Phasellus sit amet justo varius, ultricies neque at, venenatis diam.",
            "Sed quis sem viverra, volutpat odio eu, pellentesque ex.",
            "Sed sed libero in nulla auctor molestie iaculis vitae neque.",
            "Praesent mattis ipsum quis mauris feugiat, id maximus tellus convallis.",
            "Integer malesuada magna nec dolor sodales, ac consectetur risus aliquet.",
            "In finibus orci nec semper dictum.",
            "Nulla gravida arcu sed porttitor porta."
        };

        public List<ToxMessageViewModelBase> GetMessageHistoryForFriend(FriendViewModel friend)
        {
            var messages = new List<ToxMessageViewModelBase>();

            for (var i = 0; i < KMessageNum; i++)
            {
                if (i%2 == 0)
                {
                    messages.Add(new ReceivedMessageViewModel(_messageTexts[i], DateTime.Now, ToxMessageType.Message,
                        friend));
                }
                else
                {
                    messages.Add(new SentMessageViewModel(new MockDataService(), _messageTexts[i], DateTime.Now,
                        ToxMessageType.Message, -1, friend, MessageDeliveryState.Delivered));
                }
            }

            return messages;
        }
    }
}