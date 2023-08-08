USE master
GO
CREATE DATABASE ThreeL_ContextAPI ON PRIMARY
(
    NAME='threeL_contextapi',--主文件逻辑文件名
    FILENAME='D:\database\mssql_mdf\threeL_contextapi.mdf', --主文件文件名
    SIZE=5mb,--系统默认创建的时候会给主文件分配初始大小
    MAXSIZE=500MB,--主文件的最大值
    filegrowth=15%-- 主文件的增长幅度
)
LOG ON
(
    name='book_log',--日志文件逻辑文件名
    filename='D:\database\mssql_log\threeL_contextapi.ldf',--日志文件屋里文件名
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
CREATE TABLE [EMOJI](
    id bigint PRIMARY key identity,
    groupName varchar(50) not null,
    groupIcon varchar(500) not null,
    folderLocation varchar(500) not null,
    isDeleted bit NOT NULL,
    createTime datetime not null
);