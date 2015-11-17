using System;
using System.Collections;
using System.Collections.ObjectModel;
using Windows.UI;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Documents;
using Windows.UI.Xaml.Media;
using OneTox.ViewModel.Friends;
using OneTox.ViewModel.Messaging;

namespace OneTox.View.Messaging.Controls
{
    public class RichTextBlockExtension : DependencyObject
    {
        public static readonly DependencyProperty MessagesProperty = DependencyProperty.RegisterAttached(
            "Messages", typeof (ObservableCollection<MessageGroupViewModel>), typeof (RichTextBlockExtension),
            new PropertyMetadata(null, (sender, args) =>
            {
                if (!(sender is RichTextBlock))
                {
                    throw new ArgumentException("You can only use the Messages attached property on a RichTextBlock!");
                }

                if (!(args.NewValue is ObservableCollection<MessageGroupViewModel>))
                {
                    throw new ArgumentException(
                        "You can only set Messages to an ObservableCollection<MessageGroupViewModel>!");
                }

                var richTextBlock = (RichTextBlock) sender;
                var messageGroups = (ObservableCollection<MessageGroupViewModel>) args.NewValue;

                richTextBlock.Blocks.Clear();
                AddNewMessageGroups(richTextBlock, messageGroups);

                messageGroups.CollectionChanged +=
                    (o, eventArgs) => { AddNewMessageGroups(richTextBlock, eventArgs.NewItems); };
            }));

        private static void AddNewMessageGroups(RichTextBlock richTextBlock, IList messageGroups)
        {
            foreach (MessageGroupViewModel group in messageGroups)
            {
                var paragraph = new Paragraph
                {
                    TextAlignment = group.Sender is FriendViewModel ? TextAlignment.Left : TextAlignment.Right,
                    Margin = group.Sender is FriendViewModel ? new Thickness(12, 0, 120, 16) : new Thickness(120, 0, 12, 16)
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

                //paragraph.Inlines.Add(new InlineUIContainer
                //{
                //    Child = new Grid
                //    {
                //        Children =
                //        {
                //            new RichTextBlock
                //            {
                //                Blocks =
                //                {
                //                    new Paragraph
                //                    {
                //                        Inlines =
                //                        {
                //                            new Run {
                //                                Text = message.Text
                //                            }
                //                        }
                //                    }
                //                }
                //            }
                //        },
                //        Background = new SolidColorBrush(Colors.Aquamarine)
                //    }
                //});
            }
        }

        public static ObservableCollection<MessageGroupViewModel> GetMessages(
            DependencyObject d)
        {
            return (ObservableCollection<MessageGroupViewModel>) d.GetValue(MessagesProperty);
        }

        public static void SetMessages(
            DependencyObject d, ObservableCollection<MessageGroupViewModel> value)
        {
            d.SetValue(MessagesProperty, value);
        }
    }
}