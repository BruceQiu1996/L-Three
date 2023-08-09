namespace ThreeL.Shared.SuperSocket.Metadata
{
    public enum MessageType : byte
    {
        Text = 1,
        Image = 2,
        Audio = 3,
        Video = 4,
        File = 5,
        Location = 6,
        Link = 7,
        Recall = 8,

        TextResp,
        ImageResp,
        AudioResp,
        VideoResp,
        FileResp,
        LocationResp,
        LinkResp,
        RecallResp,
        //command
        Login,
        LoginResponse,
        RequestForUserEndpoint,
        RequestForUserEndpointResponse,
    }
}
