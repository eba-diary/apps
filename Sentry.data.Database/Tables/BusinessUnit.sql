CREATE TABLE [dbo].[BusinessUnit]
(
	[BusinessUnit_Id] INT IDENTITY(1,1) NOT NULL PRIMARY KEY, 
    [Name] VARCHAR(255) NOT NULL, 
    [AbbreviatedName] VARCHAR(10) NULL, 
    [Sequence] INT NOT NULL
)
