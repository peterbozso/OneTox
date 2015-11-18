using System;
using System.Collections;
using System.Collections.ObjectModel;
using Windows.UI;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Documents;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Shapes;
using OneTox.ViewModel.Friends;
using OneTox.ViewModel.Messaging;

namespace OneTox.View.Messaging.Controls
{
    public sealed partial class MessagesControl : UserControl
    {
        private readonly ObservableCollection<MessageGroupViewModel> _messageGroups;

        public MessagesControl()
        {
            InitializeComponent();

            _messageGroups = DataContext as ObservableCollection<MessageGroupViewModel>;

            Messages.Blocks.Clear();
            AddNewMessageGroups(Messages, _messageGroups);

            _messageGroups.CollectionChanged +=
                (o, eventArgs) => { AddNewMessageGroups(Messages, eventArgs.NewItems); };
        }


        private static void AddNewMessageGroups(RichTextBlock richTextBlock, IList messageGroups)
        {
            foreach (MessageGroupViewModel group in messageGroups)
            {
                var paragraph = new Paragraph
                {
                    TextAlignment = group.Sender is FriendViewModel ? TextAlignment.Left : TextAlignment.Right,
                    Margin =
                        group.Sender is FriendViewModel ? new Thickness(12, 0, 120, 16) : new Thickness(120, 0, 12, 16)
                };

                AddNewMessages(paragraph, group.Messages);
                richTextBlock.Blocks.Add(paragraph);

                group.Messages.CollectionChanged += (sender, args) => { AddNewMessages(paragraph, args.NewItems); };
            }
        }

        private static void AddNewMessages(Paragraph paragraph, IList messages)
        {
            foreach (ToxMessageViewModelBase message in messages)
            {
                if (paragraph.Inlines.Count != 0)
                {
                    paragraph.Inlines.Add(new LineBreak());
                }

                paragraph.Inlines.Add(new Run
                {
                    Text = message.Text
                });
            }
        }

        private void Messages_SelectionChanged(object sender, RoutedEventArgs e)
        {
            var start = Messages.ContentStart.GetCharacterRect(LogicalDirection.Forward);
            var end = Messages.ContentEnd.GetCharacterRect(LogicalDirection.Forward);
            var bubbleRect = new Rectangle
            {
                Width = Math.Abs(start.Left - end.Right),
                Height = Math.Abs(start.Top - end.Bottom),
                Fill = new SolidColorBrush(Colors.Aqua)
            };
            bubbleRect.SetValue(Canvas.LeftProperty, start.Left);
            bubbleRect.SetValue(Canvas.TopProperty, start.Top);
            BubbleRects.Children.Add(bubbleRect);
        }
    }
}