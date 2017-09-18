CREATE TABLE [dbo].[RT_Source_Type]
(
	[Id] INT NOT NULL PRIMARY KEY IDENTITY, 
    [Name] VARCHAR(50) NOT NULL, 
    [Base URL] VARCHAR(250) NULL, 
    [Description] VARCHAR(250) NULL
)
