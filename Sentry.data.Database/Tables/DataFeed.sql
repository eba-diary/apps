﻿CREATE TABLE [dbo].[DataFeed]
(
	[Feed_ID] INT NOT NULL PRIMARY KEY IDENTITY, 
    [Feed_URL] VARCHAR(MAX) NOT NULL, 
    [FeedType_CDE] VARCHAR(50) NOT NULL, 
    [Feed_NME] NVARCHAR(50) NOT NULL
)