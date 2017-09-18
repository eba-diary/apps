CREATE TABLE [dbo].[RT_API_Endpoints]
(
	[Endpoint_Id] INT NOT NULL PRIMARY KEY IDENTITY, 
    [SourceType_ID] INT NOT NULL, 
    [Name] VARCHAR(250) NOT NULL, 
    [Value] VARCHAR(250) NOT NULL, 
    CONSTRAINT [FK_RT_API_Endpoints_RT_Source_Types] FOREIGN KEY ([SourceType_ID]) REFERENCES [RT_Source_Types]([SourceType_Id])
)
