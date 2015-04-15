using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

// From: https://marcominerva.wordpress.com/2013/07/30/using-windows-8-1-flyout-xaml-control-with-mvvm/

namespace WinTox.ViewModel
{
    internal class AddFriendFlyoutHelpers
    {
        public static void SetIsOpen(DependencyObject d, bool value)
        {
            d.SetValue(IsOpenProperty, value);
        }

        public static bool GetIsOpen(DependencyObject d)
        {
            return (bool) d.GetValue(IsOpenProperty);
        }

        public static void SetParent(DependencyObject d, Button value)
        {
            d.SetValue(ParentProperty, value);
        }

        public static Button GetParent(DependencyObject d)
        {
            return (Button) d.GetValue(ParentProperty);
        }

        private static void OnParentPropertyChanged(DependencyObject d,
            DependencyPropertyChangedEventArgs e)
        {
            var flyout = d as Flyout;
            if (flyout != null)
            {
                flyout.Opening += (s, args) => { flyout.SetValue(IsOpenProperty, true); };

                flyout.Closed += (s, args) => { flyout.SetValue(IsOpenProperty, false); };
            }
        }

        private static void OnIsOpenPropertyChanged(DependencyObject d,
            DependencyPropertyChangedEventArgs e)
        {
            var flyout = d as Flyout;
            var parent = (Button) d.GetValue(ParentProperty);

            if (flyout != null && parent != null)
            {
                var newValue = (bool) e.NewValue;

                if (newValue)
                    flyout.ShowAt(parent);
                else
                    flyout.Hide();
            }
        }

        public static readonly DependencyProperty IsOpenProperty =
            DependencyProperty.RegisterAttached("IsOpen", typeof (bool),
                typeof (AddFriendFlyoutHelpers), new PropertyMetadata(false, OnIsOpenPropertyChanged));

        public static readonly DependencyProperty ParentProperty =
            DependencyProperty.RegisterAttached("Parent", typeof (Button),
                typeof (AddFriendFlyoutHelpers), new PropertyMetadata(null, OnParentPropertyChanged));
    }
}