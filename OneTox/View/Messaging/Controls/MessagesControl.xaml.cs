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

            // We can't do this in XAML, otherwise the RichTextBlock would be still null when this handler is called:
            DataContextChanged += OnDataContextChanged;
        }

        private void OnDataContextChanged(FrameworkElement sender, DataContextChangedEventArgs args)
        {
            // Clean up after the old DataContext (if there is one):
            if (_messageGroups != null)
            {
                // Deregister event handlers from previous DataContext:
                _messageGroups.CollectionChanged -= MessageGroupsChangedHandler;
                foreach (var group in _messageGroups)
                {
                    group.MessagesAdded -= MessagesAddedHandler;
                }

                // Also remove all paragraphs of the previous conversation we just switched from:
                _paragraphs.Clear();
            }

            // If the new DataContext is null (we navigated from chatting to settings for example), there's nothing more to do:
            if (DataContext == null)
                return;

            // Store the new DataContext and register event handlers for it:
            _messageGroups = DataContext as ObservableCollection<MessageGroupViewModel>;
            _messageGroups.CollectionChanged += MessageGroupsChangedHandler;

            // Add history:
            Messages.Blocks.Clear();
            AddNewMessageGroups(_messageGroups);

            // And repaint the bubbles:
            _bubblePainter.RepaintAllBubbles(_paragraphs);
        }

        private void MessagesControlSizeChanged(object sender, SizeChangedEventArgs e)
        {
            _bubblePainter.RepaintAllBubbles(_paragraphs);
        }

        private void MessageGroupsChangedHandler(object sender, NotifyCollectionChangedEventArgs eventArgs)
        {
            AddNewMessageGroups(eventArgs.NewItems);
        }

        private void AddNewMessageGroups(IList newMessageGroups)
        {
            foreach (MessageGroupViewModel group in newMessageGroups)
            {
                var bubbleMargin = BubblePainter.KBubbleMargin;

                var paragraph = new Paragraph
                {
                    TextAlignment = group.Sender is FriendViewModel ? TextAlignment.Left : TextAlignment.Right,
                    Margin =
                        group.Sender is FriendViewModel
                            ? new Thickness(12 + bubbleMargin, 0 + bubbleMargin, 120 + bubbleMargin, 16 + bubbleMargin)
                            : new Thickness(120 + bubbleMargin, 0 + bubbleMargin, 12 + bubbleMargin, 16 + bubbleMargin),
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
                    Foreground = new MessageToTextColorConverter().Convert(message, null, null, "") as SolidColorBrush,
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
            public const int KBubbleMargin = 8;

            private readonly Canvas _bubbleRects;

            private readonly Dictionary<Paragraph, Rectangle> _rectangles = new Dictionary<Paragraph, Rectangle>();

            public BubblePainter(Canvas bubbleRects)
            {
                _bubbleRects = bubbleRects;
            }

            public void RepaintAllBubbles(Dictionary<MessageGroupViewModel, Paragraph> paragraphs)
            {
                _bubbleRects.Children.Clear();
                _rectangles.Clear();

                foreach (var pair in paragraphs)
                {
                    PaintBubbleForParagraph(pair.Value, pair.Key.Sender);
                }
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

                double left, right;

                if (paragraph.Inlines.Count == 1) // Not much fuss if there is only one line.
                {
                    left = GetLeftMost(paragraph.Inlines[0]);
                    right = GetRightMost(paragraph.Inlines[0]);
                    ;
                }
                else
                {
                    if (!_rectangles.ContainsKey(paragraph))
                    {
                        // It means we just resized the window/control, and cleared all rectangles in order to redraw them.
                        // So we have to look for the leftmost and rightmost bounds for each paragraph:

                        if (sender is UserViewModel)
                        {
                            left = paragraph.Inlines.Select(GetLeftMost).Concat(new[] {Double.MaxValue}).Min();
                        }
                        else
                        {
                            left = start.Left;
                        }

                        if (sender is FriendViewModel)
                        {
                            right = paragraph.Inlines.Select(GetRightMost).Concat(new[] {Double.MinValue}).Max();
                        }
                        else
                        {
                            right = end.Right;
                        }
                    }
                    else
                    {
                        // Otherwise there is already a rectangle for the given paragraph, so we just have to adjust it. If we have to at all!

                        var oldRectangle = _rectangles[paragraph];
                        var lastLine = paragraph.Inlines.Last();

                        if (sender is UserViewModel)
                        {
                            var oldLeft = (double) oldRectangle.GetValue(Canvas.LeftProperty) + KBubbleMargin;
                            var lastLineLeft = GetLeftMost(lastLine);

                            left = lastLineLeft < oldLeft ? lastLineLeft : oldLeft;
                        }
                        else
                        {
                            left = start.Left;
                        }

                        if (sender is FriendViewModel)
                        {
                            var oldRight = (double) oldRectangle.GetValue(Canvas.LeftProperty) +
                                           oldRectangle.ActualWidth - KBubbleMargin;
                            var lastLineRight = GetRightMost(lastLine);

                            right = lastLineRight > oldRight ? lastLineRight : oldRight;
                        }
                        else
                        {
                            right = end.Right;
                        }
                    }
                }

                left -= KBubbleMargin;
                top -= KBubbleMargin;
                right += KBubbleMargin;
                bottom += KBubbleMargin;

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

            private double GetLeftMost(Inline inline)
            {
                var leftMost = Double.MaxValue;

                for (int offset = 0;
                    offset < inline.ContentEnd.Offset - inline.ContentStart.Offset;
                    offset++)
                {
                    var currLeft =
                        inline.ContentStart.GetPositionAtOffset(offset, LogicalDirection.Forward)
                            .GetCharacterRect(LogicalDirection.Forward)
                            .Left;
                    if (currLeft < leftMost)
                    {
                        leftMost = currLeft;
                    }
                }

                return leftMost;
            }

            private double GetRightMost(Inline inline)
            {
                var rightMost = Double.MinValue;

                for (int offset = 0;
                    offset < inline.ContentEnd.Offset - inline.ContentStart.Offset;
                    offset++)
                {
                    var currRight =
                        inline.ContentStart.GetPositionAtOffset(offset, LogicalDirection.Forward)
                            .GetCharacterRect(LogicalDirection.Forward)
                            .Right;
                    if (currRight > rightMost)
                    {
                        rightMost = currRight;
                    }
                }

                return rightMost;
            }
        }
    }
}