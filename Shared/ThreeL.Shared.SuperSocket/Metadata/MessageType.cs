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
        Withdraw = 8,
        Time = 9,

        TextResp,
        ImageResp,
        AudioResp,
        VideoResp,
        FileResp,
        LocationResp,
        LinkResp,
        WithdrawResp,
        //command
        Login,
        LoginResponse,
        RequestForUserEndpoint,
        RequestForUserEndpointResponse,
        AddFriend,
        AddFriendResponse,
        ReplyAddFriend,
        ReplyAddFriendResponse,
        InviteFriendsIntoGroup,
        InviteFriendsIntoGroupResponse,
        LoadRecord
    }
}
