CREATE TABLE [dbo].[RT_Source_Types]
(
	[SourceType_Id] INT NOT NULL PRIMARY KEY IDENTITY, 
    [Name] VARCHAR(250) NOT NULL, 
    [BaseURL] VARCHAR(250) NOT NULL, 
    [Description] VARCHAR(250) NULL, 
    [Type_NME] VARCHAR(20) NOT NULL
)
