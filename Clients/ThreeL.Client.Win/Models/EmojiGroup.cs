using System.Collections.Generic;

namespace ThreeL.Client.Win.Models
{
    public class EmojiGroup
    {
        public string GroupName { get; set; }
        public string GroupIcon { get; set; }
        public List<EmojiEntity> EmojiEntities { get; set; }
    }
}
