using System;
using Windows.ApplicationModel.Core;
using Windows.UI.Popups;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Navigation;
using WinTox.Common;
using WinTox.ViewModel;

// The Hub Page item template is documented at http://go.microsoft.com/fwlink/?LinkID=321224

namespace WinTox.View
{
    /// <summary>
    /// A page that displays a grouped collection of items.
    /// </summary>
    public sealed partial class MainPage : Page
    {
        private NavigationHelper navigationHelper;
        private MainPageViewModel _viewModel;

        /// <summary>
        /// NavigationHelper is used on each page to aid in navigation and
        /// process lifetime management
        /// </summary>
        public NavigationHelper NavigationHelper
        {
            get { return this.navigationHelper; }
        }

        public MainPage()
        {
            this.InitializeComponent();
            this.navigationHelper = new NavigationHelper(this);
            this.navigationHelper.LoadState += navigationHelper_LoadState;
            _viewModel = DataContext as MainPageViewModel;
            _viewModel.FriendRequestReceived += FriendRequestReceivedHandler;
        }

        private void FriendRequestReceivedHandler(object sender, SharpTox.Core.ToxEventArgs.FriendRequestEventArgs e)
        {
            CoreApplication.MainView.CoreWindow.Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, async () => 
            {
                var message = "From: " + e.PublicKey + "\n" + "Message: " + e.Message;
                var msgDialog = new MessageDialog(message, "Friend request received");
                msgDialog.Commands.Add(new UICommand("Accept", null, MainPageViewModel.FriendRequestAnswer.Accept));
                msgDialog.Commands.Add(new UICommand("Decline", null, MainPageViewModel.FriendRequestAnswer.Decline));
                msgDialog.Commands.Add(new UICommand("Later", null, MainPageViewModel.FriendRequestAnswer.Later));
                var answer = await msgDialog.ShowAsync();
                _viewModel.HandleFriendRequestAnswer((MainPageViewModel.FriendRequestAnswer)answer.Id, e);
            });
        }

        /// <summary>
        /// Populates the page with content passed during navigation.  Any saved state is also
        /// provided when recreating a page from a prior session.
        /// </summary>
        /// <param name="sender">
        /// The source of the event; typically <see cref="NavigationHelper"/>
        /// </param>
        /// <param name="e">Event data that provides both the navigation parameter passed to
        /// <see cref="Frame.Navigate(Type, Object)"/> when this page was initially requested and
        /// a dictionary of state preserved by this page during an earlier
        /// session.  The state will be null the first time a page is visited.</param>
        private async void navigationHelper_LoadState(object sender, LoadStateEventArgs e)
        {
        }

        #region NavigationHelper registration

        /// The methods provided in this section are simply used to allow
        /// NavigationHelper to respond to the page's navigation methods.
        ///
        /// Page specific logic should be placed in event handlers for the
        /// <see cref="Common.NavigationHelper.LoadState"/>
        /// and <see cref="Common.NavigationHelper.SaveState"/>.
        /// The navigation parameter is available in the LoadState method
        /// in addition to page state preserved during an earlier session.
        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            navigationHelper.OnNavigatedTo(e);
        }

        protected override void OnNavigatedFrom(NavigationEventArgs e)
        {
            navigationHelper.OnNavigatedFrom(e);
        }

        #endregion NavigationHelper registration
    }
}
