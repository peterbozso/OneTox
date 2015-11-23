using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Linq;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Documents;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Shapes;
using OneTox.View.Messaging.Converters;
using OneTox.ViewModel;
using OneTox.ViewModel.Friends;
using OneTox.ViewModel.Messaging;

namespace OneTox.View.Messaging.Controls
{
    public sealed partial class MessagesControl : UserControl
    {
        private readonly BubblePainter _bubblePainter;

        private readonly Dictionary<MessageGroupViewModel, Paragraph> _paragraphs =
            new Dictionary<MessageGroupViewModel, Paragraph>();

        private ObservableCollection<MessageGroupViewModel> _messageGroups;

        public MessagesControl()
        {
            InitializeComponent();

            _bubblePainter = new BubblePainter(BubbleRects);
        }

        private void MessagesControlLoaded(object sender, RoutedEventArgs e)
        {
            // Register event handlers.

            _messageGroups = DataContext as ObservableCollection<MessageGroupViewModel>;

            Messages.Blocks.Clear();
            AddNewMessageGroups(_messageGroups);

            _messageGroups.CollectionChanged += MessageGroupsChangedHandler;
        }

        private void MessagesControlUnloaded(object sender, RoutedEventArgs e)
        {
            // Deregister event handlers.

            _messageGroups.CollectionChanged -= MessageGroupsChangedHandler;

            foreach (var group in _messageGroups)
            {
                group.MessagesAdded -= MessagesAddedHandler;
            }
        }

        private void MessageGroupsChangedHandler(object sender, NotifyCollectionChangedEventArgs eventArgs)
        {
            AddNewMessageGroups(eventArgs.NewItems);
        }

        private void AddNewMessageGroups(IList newMessageGroups)
        {
            foreach (MessageGroupViewModel group in newMessageGroups)
            {
                var paragraph = new Paragraph
                {
                    TextAlignment = group.Sender is FriendViewModel ? TextAlignment.Left : TextAlignment.Right,
                    Margin =
                        group.Sender is FriendViewModel ? new Thickness(12, 0, 120, 16) : new Thickness(120, 0, 12, 16)
                };

                Messages.Blocks.Add(paragraph);
                AddNewMessages(paragraph, group.Messages, group.Sender);

                _paragraphs[group] = paragraph;

                group.MessagesAdded += MessagesAddedHandler;
            }
        }

        private void MessagesAddedHandler(object sender, IList newMessages)
        {
            var group = sender as MessageGroupViewModel;
            if (!_paragraphs.ContainsKey(group))
                return;
            AddNewMessages(_paragraphs[group], newMessages, group.Sender);
        }

        private void AddNewMessages(Paragraph paragraph, IList newMessages, IToxUserViewModel sender)
        {
            foreach (ToxMessageViewModelBase message in newMessages)
            {
                if (paragraph.Inlines.Count != 0)
                {
                    paragraph.Inlines.Add(new LineBreak());
                }

                paragraph.Inlines.Add(new Run
                {
                    Text = message.Text,
                    Foreground = new MessageToTextColorConverter().Convert(message, null, null, "") as SolidColorBrush
                });
            }

            Messages.UpdateLayout();
            _bubblePainter.PaintBubbleForParagraph(paragraph, sender);
        }

        /// <summary>
        ///     This class' responsibility is to draw the chat bubbles for message groups/paragraphs.
        /// </summary>
        private class BubblePainter
        {
            private readonly Canvas _bubbleRects;

            private readonly Dictionary<Paragraph, Rectangle> _rectangles = new Dictionary<Paragraph, Rectangle>();

            public BubblePainter(Canvas bubbleRects)
            {
                _bubbleRects = bubbleRects;
            }

            public void PaintBubbleForParagraph(Paragraph paragraph, IToxUserViewModel sender)
            {
                var newRectangle = GetRectangleForParagraph(paragraph, sender);
                if (_rectangles.ContainsKey(paragraph))
                {
                    var oldRectangle = _rectangles[paragraph];
                    _bubbleRects.Children.Remove(oldRectangle);
                }
                _rectangles[paragraph] = newRectangle;
                _bubbleRects.Children.Add(newRectangle);
            }

            private Rectangle GetRectangleForParagraph(Paragraph paragraph, IToxUserViewModel sender)
            {
                var start = paragraph.ContentStart.GetCharacterRect(LogicalDirection.Backward);
                var end = paragraph.ContentEnd.GetCharacterRect(LogicalDirection.Forward);
                var top = start.Top;
                var bottom = end.Bottom;

                double left;
                if (sender is UserViewModel)
                {
                    left = (from inline in paragraph.Inlines
                        select inline.ContentStart.GetCharacterRect(LogicalDirection.Backward).Left).Min();
                }
                else
                {
                    left = start.Left;
                }

                double right;
                if (sender is FriendViewModel)
                {
                    right = (from inline in paragraph.Inlines
                        select inline.ContentEnd.GetCharacterRect(LogicalDirection.Forward).Right).Max();
                }
                else
                {
                    right = end.Right;
                }

                var bubbleRect = new Rectangle
                {
                    Width = Math.Abs(left - right),
                    Height = Math.Abs(top - bottom),
                    Fill =
                        new SenderTypeToMessageBackgroundColorConverter().Convert(sender, null, null, "") as
                            SolidColorBrush
                };
                bubbleRect.SetValue(Canvas.LeftProperty, left);
                bubbleRect.SetValue(Canvas.TopProperty, top);
                return bubbleRect;
            }
        }
    }
}