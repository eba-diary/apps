CREATE TABLE [dbo].[DatasetScopeTypes]
(
	[ScopeType_ID] INT NOT NULL PRIMARY KEY IDENTITY, 
    [Name] VARCHAR(250) NOT NULL, 
    [Type_DSC] VARCHAR(250) NULL, 
    [IsEnabled_IND] BIT NOT NULL
)
