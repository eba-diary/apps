CREATE TABLE [dbo].[RT_Request]
(
	[Request_Id] INT NOT NULL PRIMARY KEY IDENTITY, 
    [SourceType_ID] INT NOT NULL, 
    [APIEndpoint_ID] INT NOT NULL, 
    [Requestor_ID] INT NULL, 
    [Create_DTM] NCHAR(10) NULL, 
    [Enable_IND] BIT NOT NULL, 
    [SystemFolder_NME] VARCHAR(250) NULL, 
    [Request_NME] VARCHAR(250) NOT NULL, 
    [Options_DSC] NVARCHAR(4000) NULL, 
    CONSTRAINT [FK_RT_Request_RT_Source_Types] FOREIGN KEY ([SourceType_ID]) REFERENCES [RT_Source_Types]([SourceType_Id]), 
    CONSTRAINT [FK_RT_Request_RT_API_Endpoints] FOREIGN KEY ([APIEndpoint_ID]) REFERENCES [RT_API_Endpoints]([Endpoint_Id])
)
