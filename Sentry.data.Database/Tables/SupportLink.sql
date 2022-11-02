CREATE TABLE [dbo].[SupportLink]
(
	[SupportLinkId] INT NOT NULL PRIMARY KEY IDENTITY, 
    [Name] VARCHAR(250) NOT NULL, 
    [Description] VARCHAR(250) NULL, 
    [Url] VARCHAR(2048) NOT NULL
)
