CREATE TABLE [dbo].[DataAsset_Subscription]
(
	[Subscription_ID] INT IDENTITY(1,1) NOT NULL PRIMARY KEY,
	[DataAsset_ID] [INT] NOT NULL,
	[EventType_ID] INT NOT NULL,
	[Interval_ID] [int] NOT NULL,
	[SentryOwner_NME] varchar(128) NOT NULL, 
    CONSTRAINT [FK_DataAsset_ID] FOREIGN KEY (DataAsset_ID) REFERENCES dbo.DataAsset(DataAsset_ID), 
    CONSTRAINT [FK_DataAsset_EventType] FOREIGN KEY (EventType_ID) REFERENCES dbo.EventType([Type_ID]), 
    CONSTRAINT [FK_DataAsset_IntervalType] FOREIGN KEY ([Interval_ID]) REFERENCES dbo.[IntervalType](Interval_ID) 
)
