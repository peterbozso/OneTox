namespace OneTox.ViewModel.Messaging.RecentMessages
{
    /// <summary>
    ///     Implements the Singleton pattern. (https://msdn.microsoft.com/en-us/library/ff650849.aspx)
    /// </summary>
    public class RecentMessagesGlobalViewModel : RecentMessagesViewModelBase
    {
        private static RecentMessagesGlobalViewModel _instance;

        private RecentMessagesGlobalViewModel()
        {
        }

        public static RecentMessagesGlobalViewModel Instace
            => _instance ?? (_instance = new RecentMessagesGlobalViewModel());
    }
}