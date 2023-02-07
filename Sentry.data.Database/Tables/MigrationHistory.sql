CREATE TABLE [dbo].[MigrationHistory](
	[MigrationHistoryId] [int] IDENTITY(1,1) NOT NULL,
	[CreateDateTime] [datetime] NOT NULL
    PRIMARY KEY CLUSTERED 
(
	[MigrationHistoryId] ASC
))
GO
