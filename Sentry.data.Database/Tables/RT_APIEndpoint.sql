CREATE TABLE [dbo].[RT_APIEndpoint]
(
	[Id] INT NOT NULL PRIMARY KEY, 
    [SourceType_ID] INT NOT NULL, 
    [Name] VARCHAR(100) NOT NULL, 
    CONSTRAINT [FK_RT_APIEndpoint_RT_Source_Type] FOREIGN KEY ([SourceType_ID]) REFERENCES [RT_Source_Type]([ID])
)
