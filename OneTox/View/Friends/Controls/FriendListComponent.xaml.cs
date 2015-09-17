using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

namespace OneTox.View.Friends.Controls
{
    public sealed partial class FriendListComponent : UserControl
    {
        public static readonly DependencyProperty SelectionModeProperty = DependencyProperty.Register(
            "SelectionMode", typeof (ListViewSelectionMode), typeof (FriendListComponent), null);

        public static readonly DependencyProperty SelectedItemProperty = DependencyProperty.Register(
            "SelectedItem", typeof (object), typeof (FriendListComponent), null);

        public FriendListComponent()
        {
            InitializeComponent();
        }

        public object SelectedItem
        {
            get { return GetValue(SelectedItemProperty); }
            set { SetValue(SelectedItemProperty, value); }
        }

        public ListViewSelectionMode SelectionMode
        {
            get { return (ListViewSelectionMode) GetValue(SelectionModeProperty); }
            set { SetValue(SelectionModeProperty, value); }
        }

        public event ItemClickEventHandler ItemClick;

        private void ListViewItemClick(object sender, ItemClickEventArgs e)
        {
            ItemClick?.Invoke(this, e);
        }
    }
}