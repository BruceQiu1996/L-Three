USE master
GO
CREATE DATABASE ThreeL_ContextAPI ON PRIMARY
(
    NAME='threeL_contextapi',--涓绘枃浠堕€昏緫鏂囦欢鍚�
    FILENAME='D:\database\mssql_mdf\threeL_contextapi.mdf', --涓绘枃浠舵枃浠跺悕
    SIZE=5mb,--绯荤粺榛樿鍒涘缓鐨勬椂鍊欎細缁欎富鏂囦欢鍒嗛厤鍒濆澶у皬
    MAXSIZE=500MB,--涓绘枃浠剁殑鏈€澶у€�
    filegrowth=15%-- 涓绘枃浠剁殑澧為暱骞呭害
)
LOG ON
(
    name='book_log',--鏃ュ織鏂囦欢閫昏緫鏂囦欢鍚�
    filename='D:\database\mssql_log\threeL_contextapi.ldf',--鏃ュ織鏂囦欢灞嬮噷鏂囦欢鍚�
    SIZE=5MB,--鏃ュ織鏂囦欢鍒濆澶у皬
    filegrowth=0 --鍚姩鑷姩澧為暱
)
GO
USE ThreeL_ContextAPI
GO
CREATE TABLE [USER](
    id bigint PRIMARY key identity,
    userName varchar(16) not null,
    [password] varchar(255) not null,
    avatar bigint，
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
    Withdrawed bit NOT NULL,
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
    avatar bigint，
    createBy bigint,
    createTime datetime not null,
    Members varchar(5000) not null,
);
INSERT INTO FRIEND(Activer,Passiver,ActiverRemark,PassiverRemark,createTime) VALUES(1,2,NULL,'big b bro',GETDATE())

INSERT INTO EMOJI (groupName,groupIcon,folderLocation,requestPath,isDeleted,createTime) 
VALUES('GG Bond',	'icon.jpg',	'wwwroot/emojis/zhuzhuxia','files/emojis/zhuzhuxia',	0	,'2023-08-08 00:00:00.000')