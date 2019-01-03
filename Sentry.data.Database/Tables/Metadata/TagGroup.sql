CREATE TABLE [dbo].[TagGroup]
(
	[TagGroupId] INT NOT NULL PRIMARY KEY IDENTITY, 
    [Name] VARCHAR(50) NOT NULL, 
    [Description] VARCHAR(250) NULL, 
    [Created] DATETIME NOT NULL, 
    [Modified] DATETIME NOT NULL
)
