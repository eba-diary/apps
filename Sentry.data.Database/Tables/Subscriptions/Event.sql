CREATE TABLE [dbo].[Event]
(
	[Event_ID] INT IDENTITY(1,1) NOT NULL PRIMARY KEY,
	[Reason] [VARCHAR](4096) NULL,
	[StatusType] INT NOT NULL,
	[EventType] INT NOT NULL,
	[TimeCreated] DATETIME NOT NULL,
	[TimeNotified] DATETIME NOT NULL,
	[IsProcessed] BIT NOT NULL, 
    [Parent_Event_ID] NVARCHAR(512) NULL, 
    [DataAsset_ID] INT NULL, 
    [Dataset_ID] INT NULL, 
    [DataFile_ID] INT NULL, 
    [DataConfig_ID] INT NULL, 
    [CreatedUser] NCHAR(256) NULL, 
    [Line_CDE] NVARCHAR(50) NULL, 
    [Search] NVARCHAR(MAX) NULL, 
	[Business_Term] AS JSON_VALUE(Search, '$.Business_Term'),
	[Consumption_Layer] AS JSON_VALUE(Search, '$.Consumption_Layer'),
	[Lineage_Table] AS JSON_VALUE(Search, '$.Lineage_Table'),
	[Notification_ID] INT NULL, 
	[Schema_Id] INT NULL, 
	[DeleteDetail] NVARCHAR(MAX) NULL, 
    CONSTRAINT [FK_EventType] FOREIGN KEY ([EventType]) REFERENCES [EventType]([Type_ID]), 
    CONSTRAINT [FK_StatusType] FOREIGN KEY ([StatusType]) REFERENCES [StatusType]([Status_ID]),
	CONSTRAINT [FK_Notifications] FOREIGN KEY ([Notification_ID]) REFERENCES [Notifications]([Notification_ID])
)

GO

CREATE INDEX [Search_BusinessTerms] ON [dbo].[Event] ([Business_Term])

GO
CREATE INDEX [Search_Consumption_Layer] ON [dbo].[Event] ([Consumption_Layer])

GO
CREATE INDEX [Search_Lineage_Table] ON [dbo].[Event] ([Lineage_Table])

GO

CREATE INDEX [IDX_Event__EventType_Dataset_ID] ON [dbo].[Event] ([EventType], [Dataset_ID])
GO

CREATE INDEX IDX_EventNeedsValidation ON Event(IsProcessed,TimeCreated,EventType) with(online =on)