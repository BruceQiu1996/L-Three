using System.Collections.Concurrent;

namespace ThreeL.Client.Shared.Utils
{
    public class MessageFileLocationMapper
    {
        private ConcurrentDictionary<string, string> _messageFileLocationMapping = new ConcurrentDictionary<string, string>();

        public void AddOrUpdate(string messageId, string fileLocation)
        {
            _messageFileLocationMapping.AddOrUpdate(messageId, fileLocation, (x, y) =>
            {
                return y;
            });
        }

        public string FindFileLocationByMessageId(string messageId)
        {
            var location = _messageFileLocationMapping[messageId];
            if (File.Exists(location))
                return location;

            return null;
        }
    }
}
