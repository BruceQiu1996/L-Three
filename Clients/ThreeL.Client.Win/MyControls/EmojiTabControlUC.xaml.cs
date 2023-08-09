using Microsoft.Extensions.DependencyInjection;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using ThreeL.Client.Shared.Configurations;
using ThreeL.Client.Shared.Dtos.ContextAPI;
using ThreeL.Client.Shared.Entities.Metadata;
using ThreeL.Client.Shared.Services;
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
        }

        private ObservableCollection<EmojiGroup> emojiList = new ObservableCollection<EmojiGroup>();

        /// <summary>
        /// emoji集合
        /// </summary>
        public ObservableCollection<EmojiGroup> EmojiList
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

        private EmojiEntity selectEmoji;
        /// <summary>
        /// 选中项
        /// </summary>
        public EmojiEntity SelectEmoji
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
                SelectEmoji = (EmojiEntity)lb.SelectedItem;
                SelectEmojiClickRoutedEventArgs args = new SelectEmojiClickRoutedEventArgs(selectEmojiClickEvent, this)
                {
                    Emoji = selectEmoji
                };
                RaiseEvent(args);
            }
            else
                return;
        }

        private bool _isLoaded = false;
        private async void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            if (!_isLoaded)
            {
                var resp = await App.ServiceProvider.GetRequiredService<ContextAPIService>()
                    .GetAsync<EmojiResponseDto>(Const.FETCH_EMOJIS);

                if (resp != null)
                {
                    EmojiList.Clear();
                    foreach (var group in resp.EmojiGroups)
                    {
                        EmojiList.Add(new EmojiGroup()
                        {
                            GroupIcon = $"http://{group.GroupIcon}",
                            GroupName = group.GroupName,
                            EmojiEntities = group.Emojis.Select(x => new EmojiEntity()
                            {
                                ImageType = ImageType.Network,
                                Url = $"http://{x}"
                            }).ToList()
                        });
                    }

                    _isLoaded = true;
                }
            }
        }
    }

    public class SelectEmojiClickRoutedEventArgs : RoutedEventArgs
    {
        public SelectEmojiClickRoutedEventArgs(RoutedEvent routedEvent, object source) : base(routedEvent, source)
        {

        }

        public EmojiEntity Emoji { get; set; }
    }
}
