CREATE TABLE [dbo].[Event]
(
	[Event_ID] INT IDENTITY(1,1) NOT NULL PRIMARY KEY,
	[Reason] [VARCHAR](4096) NULL,
	[StatusType] INT NOT NULL,
	[EventType] INT NOT NULL,
	[TimeCreated] DATETIME NOT NULL,
	[TimeNotified] DATETIME NOT NULL,
	[IsProcessed] BIT NOT NULL, 
    CONSTRAINT [FK_EventType] FOREIGN KEY ([EventType]) REFERENCES [EventType]([Type_ID]), 
    CONSTRAINT [FK_StatusType] FOREIGN KEY ([StatusType]) REFERENCES [StatusType]([Status_ID])
)
