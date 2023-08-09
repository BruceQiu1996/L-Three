using CommunityToolkit.Mvvm.ComponentModel;
using System;

namespace ThreeL.Client.Win.ViewModels
{
    public class MessageViewModel : ObservableObject
    {
        public string MessageId { get; set; }
        public DateTime SendTime { get; set; }
        public bool FromSelf { get; set; }
        public long From { get; set; }
        public long To { get; set; }
    }
}
