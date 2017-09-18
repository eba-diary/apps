CREATE TABLE [dbo].[RT_APIParameter]
(
	[Id] INT NOT NULL PRIMARY KEY, 
    [SourceType_ID] INT NOT NULL, 
    [Name] VARCHAR(50) NOT NULL, 
    CONSTRAINT [[FK_RT_APIParameter_RT_Source_Type]OREIGN KEY ([SourceType_ID]) REFERENCES [RT_Source_Type]([ID])
)
