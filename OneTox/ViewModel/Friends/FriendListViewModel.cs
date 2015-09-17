using System.Collections.ObjectModel;
using System.Linq;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Threading;
using OneTox.Config;
using OneTox.Model;
using OneTox.Model.Tox;

namespace OneTox.ViewModel.Friends
{
    public class FriendListViewModel : ViewModelBase
    {
        private readonly IDataService _dataService;
        private readonly IToxModel _toxModel;
        private FriendViewModel _selectedFriend;

        public FriendListViewModel(IDataService dataService)
        {
            _dataService = dataService;
            _toxModel = dataService.ToxModel;

            Friends = new ObservableCollection<FriendViewModel>();

            foreach (var friendNumber in _toxModel.Friends)
            {
                // TODO: Remember which friend we talked last and set the selection to that one by default!
                if (Friends.Count == 0)
                {
                    SelectedFriend = new FriendViewModel(_dataService, friendNumber);
                    Friends.Add(SelectedFriend);
                    continue;
                }
                //

                Friends.Add(new FriendViewModel(dataService, friendNumber));
            }

            _toxModel.FriendListChanged += FriendListChangedHandler;
        }

        public ObservableCollection<FriendViewModel> Friends { get; set; }

        public FriendViewModel SelectedFriend
        {
            get { return _selectedFriend; }
            set
            {
                if (value == _selectedFriend)
                    return;
                _selectedFriend = value;
                RaisePropertyChanged();
            }
        }

        private FriendViewModel FindFriend(int friendNumber)
        {
            return Friends.FirstOrDefault(friend => friend.FriendNumber == friendNumber);
        }

        private void FriendListChangedHandler(object sender, FriendListChangedEventArgs e)
        {
            DispatcherHelper.CheckBeginInvokeOnUI(
                () =>
                {
                    switch (e.Action)
                    {
                        case FriendListChangedAction.Add:
                            Friends.Add(new FriendViewModel(_dataService, e.FriendNumber));
                            return;

                        case FriendListChangedAction.Remove:
                            var friendToRemove = FindFriend(e.FriendNumber);

                            if (friendToRemove == SelectedFriend)
                                // It means that we just removed the currently selected friend.
                            {
                                // So select the one right above it:
                                var indexOfFriend = Friends.IndexOf(friendToRemove);
                                SelectedFriend = (indexOfFriend - 1) > 0 ? Friends[indexOfFriend - 1] : Friends[0];
                                // TODO: Handle case of removal of the last friend!!!
                            }

                            Friends.Remove(friendToRemove);
                            return;

                        case FriendListChangedAction.Reset:
                            Friends.Clear();
                            foreach (var friendNumber in _toxModel.Friends)
                            {
                                Friends.Add(new FriendViewModel(_dataService, friendNumber));
                            }
                            return;
                    }
                });
        }
    }
}