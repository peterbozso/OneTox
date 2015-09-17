using System;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using GalaSoft.MvvmLight;

namespace OneTox.ViewModel.Messaging
{
    public class MessageGroupViewModel : ViewModelBase
    {
        private DateTime _timestamp;

        public MessageGroupViewModel(IToxUserViewModel sender)
        {
            Sender = sender;
            Messages = new ObservableCollection<ToxMessageViewModelBase>();
            Messages.CollectionChanged += MessagesCollectionChangedHandler;
        }

        public ObservableCollection<ToxMessageViewModelBase> Messages { get; }
        public IToxUserViewModel Sender { get; }

        public DateTime Timestamp
        {
            get { return _timestamp; }
            private set { Set(ref _timestamp, value); }
        }

        private void MessagesCollectionChangedHandler(object sender, NotifyCollectionChangedEventArgs e)
        {
            Timestamp = ((ToxMessageViewModelBase) e.NewItems[e.NewItems.Count - 1]).Timestamp;
        }
    }
}