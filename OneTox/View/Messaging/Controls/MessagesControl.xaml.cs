using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
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
                AddNewMessages(paragraph, group.Messages);

                _paragraphs[group] = paragraph;

                group.MessagesAdded += MessagesAddedHandler;
            }
        }

        private void MessagesAddedHandler(object sender, IList newMessages)
        {
            var group = sender as MessageGroupViewModel;
            if (!_paragraphs.ContainsKey(group))
                return;
            AddNewMessages(_paragraphs[group], newMessages);
        }

        private void AddNewMessages(Paragraph paragraph, IList newMessages)
        {
            foreach (ToxMessageViewModelBase message in newMessages)
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

            Messages.UpdateLayout();
            _bubblePainter.PaintBubbleForParagraph(paragraph);
        }
        
        /// <summary>
        /// This class' responsibility is to draw the chat bubbles for message groups/paragraphs.
        /// </summary>
        private class BubblePainter
        {
            private readonly Canvas _bubbleRects;

            private readonly Dictionary<Paragraph, Rectangle> _rectangles = new Dictionary<Paragraph, Rectangle>();

            public BubblePainter(Canvas bubbleRects)
            {
                _bubbleRects = bubbleRects;
            }

            public void PaintBubbleForParagraph(Paragraph paragraph)
            {
                var newRectangle = GetRectangleForParagraph(paragraph);
                if (_rectangles.ContainsKey(paragraph))
                {
                    var oldRectangle = _rectangles[paragraph];
                    _bubbleRects.Children.Remove(oldRectangle);
                }
                _rectangles[paragraph] = newRectangle;
                _bubbleRects.Children.Add(newRectangle);
            }

            private Rectangle GetRectangleForParagraph(Paragraph paragraph)
            {
                var start = paragraph.ContentStart.GetCharacterRect(LogicalDirection.Backward);
                var end = paragraph.ContentEnd.GetCharacterRect(LogicalDirection.Forward);
                var bubbleRect = new Rectangle
                {
                    Width = Math.Abs(start.Left - end.Right),
                    Height = Math.Abs(start.Top - end.Bottom),
                    Fill = new SolidColorBrush(Colors.Aqua)
                };
                bubbleRect.SetValue(Canvas.LeftProperty, start.Left);
                bubbleRect.SetValue(Canvas.TopProperty, start.Top);
                return bubbleRect;
            }
        }
    }
}