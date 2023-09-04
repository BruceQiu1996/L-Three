namespace ThreeL.Client.Shared.Configurations
{
    public class Const
    {
        public const string LOGIN = "user/login";
        public const string USER = "user/{0}";
        public const string FIND_USER = "user/search/{0}";
        public const string AVATAR_EXIST = "user/avatar/{0}";
        public const string UPLOAD_AVATAR = "user/upload/avatar/{0}";
        public const string REFRESH_TOKEN = "user/refresh/token";
        public const string GROUP_CREATION = "user/group/{0}";
        public const string GROUP_DETAIL = "user/group/{0}";
        public const string UPLOAD_FILE = "files/{0}/{1}";
        public const string DOWNLOAD_FILE = "files/download/{0}";
        public const string DOWNLOAD_AVATAR = "user/download/avatar/{0}/{1}";
        public const string FETCH_RELATIONS = "relations/{0}";
        public const string FETCH_FRIEND_APPLYS = "relations/applys";
        public const string FETCH_EMOJIS = "emojis";
        public const string FILE_EXIST = "files/{0}";
        public const string FETCH_RELATION_CHATRECORDS = "relations/chatRecords/{0}/{1}/{2}";

        //配置:存储在sqlite中
        public const string FILE_SAVE_FOLDER = "FileSaveFolder";
    }
}
