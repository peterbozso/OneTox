using System;
using System.Collections;
using System.Collections.ObjectModel;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Documents;
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
                var paragraph = new Paragraph();
                AddNewMessages(paragraph, group.Messages);
                richTextBlock.Blocks.Add(paragraph);

                group.Messages.CollectionChanged += (sender, args) => { AddNewMessages(paragraph, args.NewItems); };
            }
        }

        private static void AddNewMessages(Paragraph paragraph, IList messages)
        {
            foreach (ToxMessageViewModelBase message in messages)
            {
                paragraph.Inlines.Add(new LineBreak());
                paragraph.Inlines.Add(new Run
                {
                    Text = message.Text
                });
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