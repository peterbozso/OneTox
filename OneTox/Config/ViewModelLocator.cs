using System.Diagnostics;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Ioc;
using GalaSoft.MvvmLight.Views;
using Microsoft.Practices.ServiceLocation;
using OneTox.ViewModel;
using OneTox.ViewModel.FriendRequests;
using OneTox.ViewModel.Friends;
using OneTox.ViewModel.ProfileSettings;

namespace OneTox.Config
{
    internal class ViewModelLocator
    {
        static ViewModelLocator()
        {
            ServiceLocator.SetLocatorProvider(() => SimpleIoc.Default);

            RegisterNavigationService();
            RegisterDialogService();
            RegisterDataService();
            RegisterViewModels();
        }

        public FriendListViewModel FriendList => ServiceLocator.Current.GetInstance<FriendListViewModel>();

        public FriendRequestsViewModel FriendRequests => ServiceLocator.Current.GetInstance<FriendRequestsViewModel>();

        public ProfileSettingsViewModel ProfileSettings
            => ServiceLocator.Current.GetInstance<ProfileSettingsViewModel>();

        public ProfileManagementViewModel ProfileManagement
            => ServiceLocator.Current.GetInstance<ProfileManagementViewModel>();

        public AddFriendViewModel AddFriend => ServiceLocator.Current.GetInstance<AddFriendViewModel>();

        private static void RegisterNavigationService()
        {
            SimpleIoc.Default.Register(CreateNavigationService);
        }

        private static NavigationService CreateNavigationService()
        {
            var navigationService = new NavigationService();

            Debug.WriteLine("STUB: CreateNavigationService()!");

            return navigationService;
        }

        private static void RegisterDialogService()
        {
            SimpleIoc.Default.Register<IDialogService, DialogService>();
        }

        private static void RegisterDataService()
        {
            if (ViewModelBase.IsInDesignModeStatic)
            {
                SimpleIoc.Default.Register<IDataService, MockDataService>();
            }
            else
            {
                SimpleIoc.Default.Register<IDataService, DataService>();
            }
        }

        private static void RegisterViewModels()
        {
            SimpleIoc.Default.Register<FriendListViewModel>();
            SimpleIoc.Default.Register<FriendRequestsViewModel>();
            SimpleIoc.Default.Register<ProfileSettingsViewModel>();
            SimpleIoc.Default.Register<ProfileManagementViewModel>();
            SimpleIoc.Default.Register<AddFriendViewModel>();
        }
    }
}