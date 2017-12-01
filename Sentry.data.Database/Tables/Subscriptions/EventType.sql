CREATE TABLE [dbo].[EventType]
(
	[Type_ID] INT IDENTITY(1,1) NOT NULL PRIMARY KEY,
	[Description] [varchar](1024) NOT NULL,
	[Severity] INT NOT NULL
)
