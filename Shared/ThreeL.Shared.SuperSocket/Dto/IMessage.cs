namespace ThreeL.Shared.SuperSocket.Dto
{
    public interface IMessage
    {
        string MessageId { get; set; }
        string ReplyMessageId { get; set; }
    }
}
