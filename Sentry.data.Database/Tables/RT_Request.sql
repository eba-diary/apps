CREATE TABLE [dbo].[RT_Request]
(
	[Id] INT NOT NULL PRIMARY KEY IDENTITY,
	[SourceType_ID]  INT NOT NULL,
    [Reqeustor_ID] INT NOT NULL, 
    [Create_DTM] DATETIME NOT NULL, 
    CONSTRAINT [FK_RT_Request_RT_Source_Type] FOREIGN KEY ([SourceType_ID]) REFERENCES [RT_Source_Type]([ID]) 
)
