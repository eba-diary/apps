CREATE TABLE [dbo].[Hive_Schema]
(
	[Schema_ID] INT IDENTITY (1, 1) PRIMARY KEY NOT NULL,
    [Config_ID] INT NOT NULL,
	[Revision_ID] INT NOT NULL, 
	[Schema_NME] NVARCHAR(MAX) NULL,
	[Schema_DSC] NVARCHAR(MAX) NULL,
    [DataObject_ID] INT NOT NULL, 
    [IsForceMatch] BIT NOT NULL, 
	[Created_DTM] [datetime] NOT NULL,
	[Changed_DTM] [datetime] NOT NULL,
    CONSTRAINT [FK_Schema_DatasetFileConfigs] FOREIGN KEY ([Config_ID]) REFERENCES [dbo].[DatasetFileConfigs] ([Config_ID]),
)
