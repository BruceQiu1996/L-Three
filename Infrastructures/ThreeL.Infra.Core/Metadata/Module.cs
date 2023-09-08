using System.ComponentModel;

namespace ThreeL.Infra.Core.Metadata
{
    public enum Module
    {
        [Description("ContextApi_业务")]
        CONTEXT_API_SERVICE,
        [Description("ContextApi_Grpc")]
        CONTEXT_API_GRPC,
        [Description("ContextApi_Http")]
        CONTEXT_API_HTTP,
        [Description("SocketServer_Grpc")]
        SOCKET_SERVER_GRPC,
        [Description("SocketServer_登录业务")]
        SOCKET_SERVER_TCPHANDLER_LOGIN,
        [Description("ContextApi_文本聊天业务")]
        SOCKET_SERVER_TCPHANDLER_CHAT_TEXT,
        [Description("ContextApi_文件聊天业务")]
        SOCKET_SERVER_TCPHANDLER_CHAT_FILE,
        [Description("ContextApi_图片聊天业务")]
        SOCKET_SERVER_TCPHANDLER_CHAT_IMAGE,
        [Description("WinClient_UI_EXCEPTION")]
        CLIENT_WIN_UI_EXCEPTION,
        [Description("WinClient_THREAD_EXCEPTION")]
        CLIENT_WIN_THREAD_EXCEPTION,
        [Description("WinClient_Task_EXCEPTION")]
        CLIENT_WIN_TASK_EXCEPTION,
        [Description("WinClient")]
        CLIENT_WIN
    }
}
