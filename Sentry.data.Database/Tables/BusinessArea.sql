CREATE TABLE [dbo].[BusinessArea]
(
	[BusinessArea_ID] INT IDENTITY(1,1) NOT NULL PRIMARY KEY, 
    [Name_DSC] VARCHAR(255) NOT NULL, 
    [AbbreviatedName_DSC] VARCHAR(10) NULL
)
