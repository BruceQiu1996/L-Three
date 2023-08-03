using CommunityToolkit.Mvvm.ComponentModel;
using System;

namespace ThreeL.Client.Win.ViewModels
{
    public class MessageViewModel : ObservableObject
    {
        public string MessageId { get; set; }
        public DateTime SendTime { get; set; }
        public bool FromSelf { get; set; }
    }
}
