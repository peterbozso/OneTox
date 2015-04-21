using System;
using System.Diagnostics;
using System.Threading.Tasks;
using Windows.ApplicationModel;
using Windows.ApplicationModel.Activation;
using Windows.Globalization;
using Windows.UI.ApplicationSettings;
using Windows.UI.Popups;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Navigation;
using SharpTox.Core;
using WinTox.Common;
using WinTox.Model;
using WinTox.View;
using WinTox.ViewModel;

// The Hub App template is documented at http://go.microsoft.com/fwlink/?LinkId=321221

namespace WinTox
{
    /// <summary>
    ///     Provides application-specific behavior to supplement the default Application class.
    /// </summary>
    sealed partial class App : Application
    {
        public static ToxModel ToxModel;
        public static UserViewModel UserViewModel;

        /// <summary>
        ///     Initializes the singleton Application object.  This is the first line of authored code
        ///     executed, and as such is the logical equivalent of main() or WinMain().
        /// </summary>
        public App()
        {
            InitializeComponent();
            Suspending += OnSuspending;
            Resuming += OnResuming;
            ToxModel = new ToxModel(new ExtendedTox(new ToxOptions(true, true)));
            UserViewModel = new UserViewModel();
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

                ToxModel.Start();

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

        private async Task HandlePreviousExecutionState(ApplicationExecutionState previousExecutionState)
        {
            if (previousExecutionState == ApplicationExecutionState.Terminated ||
                previousExecutionState == ApplicationExecutionState.ClosedByUser ||
                previousExecutionState == ApplicationExecutionState.NotRunning)
            {
                var successfulRestoration = true;
                try
                {
                    await ToxModel.RestoreDataAsync();
                }
                catch
                {
                    successfulRestoration = false;
                }
                // If the restoration was unsuccessful, it means that we are starting up the app the
                // very firs time or something went wrong restoring data.
                // So we save the current Tox instance (set in App's constructor) as the default one.
                if (!successfulRestoration)
                    await ToxModel.SaveDataAsync();

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
            await SuspensionManager.SaveAsync();
            await ToxModel.SaveDataAsync();
            deferral.Complete();
        }

        private async void OnResuming(object sender, object e)
        {
            await SuspensionManager.RestoreAsync();
            await ToxModel.RestoreDataAsync();
            ToxModel.Start();
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