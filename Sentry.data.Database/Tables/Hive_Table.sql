CREATE TABLE [dbo].[Hive_Table]
(
	[Hive_ID] INT IDENTITY (1, 1) PRIMARY KEY NOT NULL,
	[Schema_ID] INT NOT NULL,
	[Hive_NME] NVARCHAR(MAX) NOT NULL,
	[Hive_DSC] NVARCHAR(MAX) NULL,
    [HiveDatabase_NME] NVARCHAR(MAX) NOT NULL,
	[IsPrimary] BIT NOT NULL, 
	
	[Created_DTM] [datetime] NOT NULL,
	[Changed_DTM] [datetime] NOT NULL,
    CONSTRAINT [FK_HiveTables_Schema] FOREIGN KEY ([Schema_ID]) REFERENCES [dbo].[Hive_Schema] ([Schema_ID])
)
