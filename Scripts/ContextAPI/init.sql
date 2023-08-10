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
    avatar varchar(500),
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
    groupName varchar(50) not null,
    groupIcon varchar(500) not null,
    folderLocation varchar(500) not null,
    requestPath varchar(500) not null,
    isDeleted bit NOT NULL,
    createTime datetime not null
);
INSERT INTO FRIEND(Activer,Passiver,ActiverRemark,PassiverRemark,createTime) VALUES(1,2,NULL,'��b��',GETDATE())
INSERT INTO EMOJI (groupName,groupIcon,folderLocation,requestPath,isDeleted,createTime) 
VALUES('������',	'icon.jpg',	'wwwroot/emojis/zhuzhuxia','files/emojis/zhuzhuxia',	0	,'2023-08-08 00:00:00.000')