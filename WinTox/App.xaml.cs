using System;
using System.Diagnostics;
using System.Threading.Tasks;
using Windows.ApplicationModel;
using Windows.ApplicationModel.Activation;
using Windows.ApplicationModel.Core;
using Windows.Foundation;
using Windows.Globalization;
using Windows.UI.ApplicationSettings;
using Windows.UI.Core;
using Windows.UI.Popups;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Navigation;
using WinTox.Common;
using WinTox.Model;
using WinTox.Model.Avatars;
using WinTox.View;
using WinTox.ViewModel;
using WinTox.ViewModel.FriendRequests;

// The Hub App template is documented at http://go.microsoft.com/fwlink/?LinkId=321221

namespace WinTox
{
    /// <summary>
    ///     Provides application-specific behavior to supplement the default Application class.
    /// </summary>
    sealed partial class App : Application
    {
        private FriendRequestView _friendRequestView;
        private IAsyncOperation<IUICommand> _showErrorDialogCommand;

        /// <summary>
        ///     Initializes the singleton Application object.  This is the first line of authored code
        ///     executed, and as such is the logical equivalent of main() or WinMain().
        /// </summary>
        public App()
        {
            InitializeComponent();
            Suspending += OnSuspending;
            Resuming += OnResuming;
        }

        /// <summary>
        ///     Invoked when the application is launched normally by the end user.  Other entry points
        ///     will be used such as when the application is launched to open a specific file.
        /// </summary>
        /// <param name="e">Details about the launch request and process.</param>
        protected override async void OnLaunched(LaunchActivatedEventArgs e)
        {
#if DEBUG
            if (Debugger.IsAttached)
            {
                DebugSettings.EnableFrameRateCounter = true;
            }
#endif

            var rootFrame = Window.Current.Content as Frame;

            // Do not repeat app initialization when the Window already has content,
            // just ensure that the window is active

            if (rootFrame == null)
            {
                // Create a Frame to act as the navigation context and navigate to the first page
                rootFrame = new Frame();
                // Associate the frame with a SuspensionManager key
                SuspensionManager.RegisterFrame(rootFrame, "AppFrame");
                // Set the default language
                rootFrame.Language = ApplicationLanguages.Languages[0];

                rootFrame.NavigationFailed += OnNavigationFailed;

                await HandlePreviousExecutionState(e.PreviousExecutionState);

                await InitializeSingletons();

                _friendRequestView = new FriendRequestView();

                // Place the frame in the current Window
                Window.Current.Content = rootFrame;
            }
            if (rootFrame.Content == null)
            {
                // When the navigation stack isn't restored navigate to the first page,
                // configuring the new page by passing required information as a navigation
                // parameter
                rootFrame.Navigate(typeof (MainPage), e.Arguments);
            }
            // Ensure the current window is active
            Window.Current.Activate();
        }

        private async Task InitializeSingletons()
        {
            ToxModel.Instance.Start();
            await AvatarManager.Instance.LoadAvatars();
            ToxErrorViewModel.Instance.ToxErrorOccured += ToxErrorOccuredHandler;
            await FriendRequestsViewModel.Instance.RestoreDataAsync();
        }

        private async void ToxErrorOccuredHandler(object sender, string errorMessage)
        {
            if (_showErrorDialogCommand != null)
            {
                return;
            }

            await CoreApplication.MainView.CoreWindow.Dispatcher.RunAsync(CoreDispatcherPriority.Normal, async () =>
            {
                var msgDialog = new MessageDialog(errorMessage, "Error occured");
                _showErrorDialogCommand = msgDialog.ShowAsync();
                await _showErrorDialogCommand;
                _showErrorDialogCommand = null;
            });
        }

        private async Task HandlePreviousExecutionState(ApplicationExecutionState previousExecutionState)
        {
            if (previousExecutionState == ApplicationExecutionState.Terminated ||
                previousExecutionState == ApplicationExecutionState.ClosedByUser ||
                previousExecutionState == ApplicationExecutionState.NotRunning)
            {
                var successfulRestoration = true;
                try
                {
                    await ToxModel.Instance.RestoreDataAsync();
                }
                catch
                {
                    successfulRestoration = false;
                }
                // If the restoration was unsuccessful, it means that we are starting up the app the
                // very firs time or something went wrong restoring data.
                // So we save the current Tox instance (newly created, not loaded) as the default one.
                if (!successfulRestoration)
                    await ToxModel.Instance.SaveDataAsync();

                if (previousExecutionState != ApplicationExecutionState.NotRunning)
                    // We only have to restore session state in the other two cases.
                    // See: https://msdn.microsoft.com/en-us/library/ie/windows.applicationmodel.activation.applicationexecutionstate
                {
                    try
                    {
                        await SuspensionManager.RestoreAsync();
                    }
                    catch (SuspensionManagerException)
                    {
                        // Something went wrong restoring state.
                        // Assume there is no state and continue.
                    }
                }
            }
        }

        /// <summary>
        ///     Invoked when Navigation to a certain page fails
        /// </summary>
        /// <param name="sender">The Frame which failed navigation</param>
        /// <param name="e">Details about the navigation failure</param>
        private void OnNavigationFailed(object sender, NavigationFailedEventArgs e)
        {
            throw new Exception("Failed to load Page " + e.SourcePageType.FullName);
        }

        /// <summary>
        ///     Invoked when application execution is being suspended.  Application state is saved
        ///     without knowing whether the application will be terminated or resumed with the contents
        ///     of memory still intact.
        /// </summary>
        /// <param name="sender">The source of the suspend request.</param>
        /// <param name="e">Details about the suspend request.</param>
        private async void OnSuspending(object sender, SuspendingEventArgs e)
        {
            var deferral = e.SuspendingOperation.GetDeferral();

            // Disabled until "GetNavigationState doesn't support serialization of a parameter type which was passed to Frame.Navigate." exception is fixed.
            // (It's the FriendViewModel on MainPage -> ChatPage navigation.)
            // await SuspensionManager.SaveAsync();

            await ToxModel.Instance.SaveDataAsync();
            // await FileTransferManager.Instance.StoreBrokenTransfers(); TODO
            deferral.Complete();
        }

        private async void OnResuming(object sender, object e)
        {
            // See OnSuspending()!
            // await SuspensionManager.RestoreAsync();

            await ToxModel.Instance.RestoreDataAsync();
            ToxModel.Instance.Start();
            await FriendRequestsViewModel.Instance.RestoreDataAsync();
        }

        #region Profile settings flyout setup

        protected override void OnWindowCreated(WindowCreatedEventArgs args)
        {
            SettingsPane.GetForCurrentView().CommandsRequested += OnCommandsRequested;
        }

        private static void OnCommandsRequested(SettingsPane sender, SettingsPaneCommandsRequestedEventArgs args)
        {
            args.Request.ApplicationCommands.Add(new SettingsCommand(
                "Profile settings", "Profile settings", handler => ShowProfileSettingsFlyout()));
        }

        public static void ShowProfileSettingsFlyout()
        {
            new ProfileSettingsFlyout().Show();
        }

        #endregion
    }
}