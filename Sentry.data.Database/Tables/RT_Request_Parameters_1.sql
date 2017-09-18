CREATE TABLE [dbo].[RT_Request_Parameters]
(
	[RequestParameter_Id] INT NOT NULL PRIMARY KEY IDENTITY, 
    [Request_ID] INT NOT NULL, 
    [APIParameter_ID] INT NOT NULL, 
    [Value] VARCHAR(250) NULL, 
    CONSTRAINT [FK_RT_Request_Parameters_RT_Request] FOREIGN KEY ([Request_ID]) REFERENCES [RT_Request]([Request_Id]), 
    CONSTRAINT [FK_RT_Request_Parameters_RT_APIParameters] FOREIGN KEY ([APIParameter_ID]) REFERENCES [RT_API_Parameters]([APIParameter_Id])
)
