CREATE TABLE [dbo].[RT_Request_Parameters]
(
	[Id] INT NOT NULL PRIMARY KEY, 
    [Request_ID] INT NOT NULL, 
    [APIParameter_ID] INT NOT NULL, 
    CONSTRAINT [FK_RT_Request_Parameters_RT_Request] FOREIGN KEY ([Request_ID]) REFERENCES [RT_Request]([ID]), 
    CONSTRAINT [FK_RT_Request_Parameters_RT_APIParameters] FOREIGN KEY ([APIParameter_ID]) REFERENCES [RT_APIParameters]([ID])
)
