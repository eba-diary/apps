CREATE TABLE [dbo].[Dataset_Subscription]
(
	[Subscription_ID] INT IDENTITY(1,1) NOT NULL PRIMARY KEY,
	[Dataset_ID] [INT] NOT NULL,
	[EventType_ID] INT NOT NULL,
	[Interval_ID] [int] NOT NULL,
	[SentryOwner_NME] varchar(128) NOT NULL, 
    CONSTRAINT [FK_Dataset_ID] FOREIGN KEY ([Dataset_ID]) REFERENCES [dbo].[Dataset](Dataset_ID),
	CONSTRAINT [FK_Dataset_EventType] FOREIGN KEY (EventType_ID) REFERENCES dbo.EventType([Type_ID]), 
    CONSTRAINT [FK_Dataset_IntervalType] FOREIGN KEY ([Interval_ID]) REFERENCES dbo.[IntervalType](Interval_ID) 
)
