USE master
GO
CREATE DATABASE ThreeL_ContextAPI ON PRIMARY
(
    NAME='threeL_contextapi',--主文件��辑文件各1�7
    FILENAME='D:\database\mssql_mdf\threeL_contextapi.mdf', --主文件文件名
    SIZE=5mb,--系统默认创建的时候会给主文件分配初始大小
    MAXSIZE=500MB,--主文件的朢�大��1�7
    filegrowth=15%-- 主文件的增长幅度
)
LOG ON
(
    name='book_log',--日志文件逻辑文件各1�7
    filename='D:\database\mssql_log\threeL_contextapi.ldf',--日志文件屋里文件各1�7
    SIZE=5MB,--日志文件初始大小
    filegrowth=0 --启动自动增长
)
GO
USE ThreeL_ContextAPI
GO
CREATE TABLE [USER](
    id bigint PRIMARY key identity,
    userName varchar(16) not null,
    [password] varchar(255) not null,
    avatar bigint��
    sign varchar(20),
    isDeleted bit NOT NULL,
    [role] int NOT NULL,
    createBy bigint,
    createTime datetime not null,
    lastLoginTime datetime
);
GO
CREATE TABLE FRIEND(
    id bigint PRIMARY key identity,
    Activer bigint not NULL,
		Passiver bigint not NULL,
		ActiverRemark VARCHAR(50),
		PassiverRemark VARCHAR(50),
    createTime datetime not null
);
GO
CREATE TABLE [EMOJI](
    id bigint PRIMARY key identity,
    groupName nvarchar(50) not null,
    groupIcon varchar(500) not null,
    folderLocation varchar(500) not null,
    requestPath varchar(500) not null,
    isDeleted bit NOT NULL,
    createTime datetime not null
);
GO
CREATE TABLE [File](
    id bigint PRIMARY key identity,
    CreateBy bigint not NULL,
	FileName VARCHAR(500) NOT NULL,
	[Size] bigint not NULL,
	Code VARCHAR(500) NOT NULL,
	Location VARCHAR(500) NOT NULL,
    createTime datetime not null,
    Receiver bigint not null
);
GO
CREATE TABLE ChatRecord(
    Id bigint PRIMARY key identity,
    [From] bigint not NULL,
    [To] bigint not NULL,
    MessageId VARCHAR(50) not NULL,
    Message VARCHAR(500),
    MessageRecordType int,
    ImageType int,
    SendTime datetime,
    FileId bigint,
    InnerId bigint,
    Withdrawed bit NOT NULL, --TODO默认值0
);
GO
CREATE TABLE [dbo].[VoiceChatRecord] (
  [Id] bigint PRIMARY key identity,
  [ChatKey] varchar(50) NOT NULL,
  [SendTime] datetime  NOT NULL,
  [From] bigint  NOT NULL,
  [To] bigint  NOT NULL,
  [StartTime] datetime  NULL,
  [EndTime] datetime  NULL,
  [FromPlatform] varchar(255) NOT NULL,
  [ToPlatform] varchar(255),
  [Status] int NOT NULL
);
GO
CREATE TABLE GroupChatRecord(
    Id bigint PRIMARY key identity,
    [From] bigint not NULL,
    FromName NVARCHAR(50) not NULL,
    [To] bigint not NULL,
    MessageId VARCHAR(50) not NULL,
    Message VARCHAR(500),
    MessageRecordType int,
    ImageType int,
    SendTime datetime,
    FileId bigint,
    Withdrawed bit NOT NULL, --TODO默认值0
);
CREATE TABLE [FriendApply] (
  [Id] bigint  PRIMARY key identity,
  [Activer] bigint  NOT NULL,
  [Passiver] bigint  NOT NULL,
  [CreateTime] datetime  NOT NULL,
  [Status] tinyint  NOT NULL,
  [ProcessTime] datetime  NULL
)
GO

CREATE TABLE [UserAvatar](
    id bigint PRIMARY key identity,
    CreateBy bigint not NULL,
	FileName VARCHAR(500) NOT NULL,
	Code VARCHAR(500) NOT NULL,
	Location VARCHAR(500) NOT NULL,
    createTime datetime not null
);
GO

CREATE TABLE [Group](
    id bigint PRIMARY key identity,
    Name varchar(16) not null,
    avatar bigint��
    createBy bigint,
    createTime datetime not null,
    Members varchar(5000) not null,
);
INSERT INTO FRIEND(Activer,Passiver,ActiverRemark,PassiverRemark,createTime) VALUES(1,2,NULL,'big b bro',GETDATE())

INSERT INTO EMOJI (groupName,groupIcon,folderLocation,requestPath,isDeleted,createTime) 
VALUES('GG Bond',	'icon.jpg',	'wwwroot/emojis/zhuzhuxia','files/emojis/zhuzhuxia',	0	,'2023-08-08 00:00:00.000')