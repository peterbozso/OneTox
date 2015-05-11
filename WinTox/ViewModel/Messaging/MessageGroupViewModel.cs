using System.Collections.ObjectModel;

namespace WinTox.ViewModel.Messaging
{
    public class MessageGroupViewModel
    {
        public MessageGroupViewModel(IToxUserViewModel sender)
        {
            Sender = sender;
            Messages = new ObservableCollection<MessageViewModel>();
        }

        public IToxUserViewModel Sender { get; private set; }
        public ObservableCollection<MessageViewModel> Messages { get; private set; }
    }
}