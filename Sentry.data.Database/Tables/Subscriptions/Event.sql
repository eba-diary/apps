CREATE TABLE [dbo].[Event]
(
	[Event_ID] INT IDENTITY(1,1) NOT NULL PRIMARY KEY,
	[Reason] [VARCHAR](4096) NULL,
	[StatusType] INT NOT NULL,
	[EventType] INT NOT NULL,
	[TimeCreated] DATETIME NOT NULL,
	[TimeNotified] DATETIME NOT NULL,
	[IsProcessed] BIT NOT NULL, 
    [Parent_Event_ID] INT NULL, 
    [DataAsset_ID] INT NULL, 
    [Dataset_ID] INT NULL, 
    [DataFile_ID] INT NULL, 
    [DataConfig_ID] INT NULL, 
    [CreatedUser] NCHAR(256) NULL, 
    CONSTRAINT [FK_EventType] FOREIGN KEY ([EventType]) REFERENCES [EventType]([Type_ID]), 
    CONSTRAINT [FK_StatusType] FOREIGN KEY ([StatusType]) REFERENCES [StatusType]([Status_ID]), 
    CONSTRAINT [FK_DataFile] FOREIGN KEY ([DataFile_ID]) REFERENCES [DatasetFile]([DatasetFile_Id]), 
    CONSTRAINT [FK_DataAsset] FOREIGN KEY (DataAsset_ID) REFERENCES [DataAsset]([DataAsset_ID]), 
    CONSTRAINT [FK_DataConfig] FOREIGN KEY ([DataConfig_ID]) REFERENCES [DatasetFileConfigs]([Config_ID]), 
    CONSTRAINT [FK_Dataset] FOREIGN KEY (Dataset_ID) REFERENCES [Dataset]([Dataset_ID]), 
    CONSTRAINT [FK_ParentEvent] FOREIGN KEY ([Parent_Event_ID]) REFERENCES [Event]([Event_ID])
)
