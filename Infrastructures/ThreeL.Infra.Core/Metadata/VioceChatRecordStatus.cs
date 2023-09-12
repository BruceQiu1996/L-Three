namespace ThreeL.Infra.Core.Metadata
{
    public enum VioceChatRecordStatus
    {
        /// <summary>
        ///        /// 未开始
        ///        /// </summary>
        NotStart = 0,
        ///                             /// <summary>
        ///                                    /// 进行中
        ///                                           /// </summary>
        InProgress = 1,
        ///                                                         /// <summary>
        ///                                                                /// 已结束
        ///                                                                       /// </summary>
        Finished = 2,
        ///                                                                                     /// <summary>
        ///                                                                                            /// 已取消
        ///                                                                                                   /// </summary>
        Canceled = 3,
        ///                                                                                            /// 已拒绝
        ///                                                                                                   /// </summary>
        Rejected = 4,
        /// <summary>
        /// 超时未接受
        /// </summary>
        NotAccept = 5;
    }
}
