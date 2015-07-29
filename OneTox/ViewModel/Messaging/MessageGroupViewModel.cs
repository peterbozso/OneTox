using System.Collections.ObjectModel;

namespace OneTox.ViewModel.Messaging
{
    public class MessageGroupViewModel
    {
        public MessageGroupViewModel(IToxUserViewModel sender)
        {
            Sender = sender;
            Messages = new ObservableCollection<ToxMessageViewModelBase>();
        }

        public IToxUserViewModel Sender { get; private set; }
        public ObservableCollection<ToxMessageViewModelBase> Messages { get; private set; }
    }
}