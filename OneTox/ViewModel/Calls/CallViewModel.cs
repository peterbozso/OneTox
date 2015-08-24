namespace OneTox.ViewModel.Calls
{
    public class CallViewModel
    {
        public CallViewModel(int friendNumber)
        {
            Audio = new AudioCallViewModel(friendNumber);
            Video = new VideoCallViewModel(friendNumber);
        }

        public AudioCallViewModel Audio { get; private set; }
        public VideoCallViewModel Video { get; private set; }
    }
}
