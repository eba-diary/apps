CREATE TABLE [dbo].[RT_API_Parameters]
(
	[APIParameter_Id] INT NOT NULL PRIMARY KEY IDENTITY, 
    [SourceType_ID] INT NOT NULL,
	[APIEndpoint_ID] INT NOT NULL, 
    [Name] VARCHAR(250) NOT NULL, 
    CONSTRAINT [FK_RT_API_Parameters_RT_Source_Types] FOREIGN KEY ([SourceType_ID]) REFERENCES [RT_Source_Types]([SourceType_Id]), 
    CONSTRAINT [FK_RT_API_Parameters_RT_API_Endpoints] FOREIGN KEY ([APIEndpoint_ID]) REFERENCES [RT_API_Endpoints]([Endpoint_Id])
)
