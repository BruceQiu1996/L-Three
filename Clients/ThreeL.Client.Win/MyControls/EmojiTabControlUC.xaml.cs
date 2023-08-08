using Microsoft.Extensions.DependencyInjection;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Imaging;
using ThreeL.Client.Win.Models;

namespace ThreeL.Client.Win.MyControls
{
    public partial class EmojiTabControlUC : UserControl
    { 
        public static readonly RoutedEvent selectEmojiClickEvent =
            EventManager.RegisterRoutedEvent("SelectEmojiClick", RoutingStrategy.Bubble, typeof(RoutedEventHandler), typeof(EmojiTabControlUC));

        public event RoutedEventHandler SelectEmojiClick
        {
            add
            {
                AddHandler(selectEmojiClickEvent, value);
            }

            remove
            {
                RemoveHandler(selectEmojiClickEvent, value);
            }
        }

        public EmojiTabControlUC()
        {
            InitializeComponent();
            //EmojiList = new ObservableCollection<EmojiEntity>(App.ServiceProvider.GetRequiredService<EmojiHelper>().EmojiList);
        }

        private static ObservableCollection<EmojiEntity> emojiList = new ObservableCollection<EmojiEntity>();

        /// <summary>
        /// emoji集合
        /// </summary>
        public static ObservableCollection<EmojiEntity> EmojiList
        {
            get
            {
                return emojiList;
            }

            set
            {
                emojiList = value;
            }
        }

        private KeyValuePair<string, BitmapImage> selectEmoji = new KeyValuePair<string, BitmapImage>();
        /// <summary>
        /// 选中项
        /// </summary>
        public KeyValuePair<string, BitmapImage> SelectEmoji
        {
            get
            {
                return selectEmoji;
            }

            set
            {
                selectEmoji = value;
            }
        }

        private void ListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            ListBox lb = sender as ListBox;
            if (lb.SelectedItem != null)
            {
                SelectEmoji = (KeyValuePair<string, BitmapImage>)lb.SelectedItem;
                SelectEmojiClickRoutedEventArgs args = new SelectEmojiClickRoutedEventArgs(selectEmojiClickEvent, this) 
                {
                    Emoji = selectEmoji
                };
                RaiseEvent(args);
            }
            else
                return;
        }
    }

    public class SelectEmojiClickRoutedEventArgs : RoutedEventArgs
    {
        public SelectEmojiClickRoutedEventArgs(RoutedEvent routedEvent, object source) : base(routedEvent,source)
        {
            
        }

        public KeyValuePair<string, BitmapImage> Emoji { get; set; }
    }
}
