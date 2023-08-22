﻿namespace ThreeL.Client.Shared.Configurations
{
    public class Const
    {
        public const string LOGIN = "user/login";
        public const string UPLOAD_AVATAR = "user/upload/{0}";
        public const string REFRESH_TOKEN = "user/refresh/token";
        public const string UPLOAD_FILE = "files/{0}/{1}";
        public const string DOWNLOAD_FILE = "files/download/{0}";
        public const string FETCH_FRIENDS = "relations/friends";
        public const string FETCH_EMOJIS = "emojis";
        public const string FILE_EXIST = "files/{0}";
        public const string FETCH_FRIEND_CHATRECORDS = "relations/{0}/{1}/chatRecords";

        //配置:存储在sqlite中
        public const string FILE_SAVE_FOLDER = "FileSaveFolder";
    }
}
