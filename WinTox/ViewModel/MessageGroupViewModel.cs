using System.Collections.ObjectModel;

namespace WinTox.ViewModel
{
    internal class MessageGroupViewModel
    {
        public MessageGroupViewModel()
        {
            Messages = new ObservableCollection<MessageViewModel>();
        }

        public ObservableCollection<MessageViewModel> Messages { get; private set; }
    }
}