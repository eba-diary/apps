CREATE TABLE [dbo].[BusinessArea_Subscription]
(
	[Subscription_ID] INT IDENTITY(1,1) NOT NULL PRIMARY KEY,
	[BusinessArea_ID] [INT] NOT NULL,
	[EventType_ID] INT NOT NULL,
	[Interval_ID] [int] NOT NULL,
	[SentryOwner_NME] varchar(128) NOT NULL, 
    CONSTRAINT [FK_BusinessArea_ID] FOREIGN KEY ([BusinessArea_ID]) REFERENCES [dbo].[BusinessArea](BusinessArea_ID),
	CONSTRAINT [FK_BusinessArea_EventType] FOREIGN KEY (EventType_ID) REFERENCES dbo.EventType([Type_ID]), 
    CONSTRAINT [FK_BusinessArea_IntervalType] FOREIGN KEY ([Interval_ID]) REFERENCES dbo.[IntervalType](Interval_ID) 
)
