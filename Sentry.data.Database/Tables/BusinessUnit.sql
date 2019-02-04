CREATE TABLE [dbo].[BusinessUnit]
(
	[BusinessUnit_Id] INT NOT NULL PRIMARY KEY, 
    [Name] VARCHAR(255) NOT NULL, 
    [AbbreviatedName] VARCHAR(10) NULL, 
    [Sequence] INT NOT NULL
)
