namespace OneTox.Model
{
    public enum FriendListChangedAction
    {
        Add,
        Remove,
        Reset
    }

    public class FriendListChangedEventArgs
    {
        public FriendListChangedAction Action;
        public int FriendNumber;
    }
}
