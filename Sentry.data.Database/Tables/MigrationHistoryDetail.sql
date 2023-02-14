CREATE TABLE [dbo].[MigrationHistoryDetail](
	[MigrationHistoryDetailId] [int] IDENTITY(1,1) NOT NULL,
	[MigrationHistoryId] [int] NOT NULL,
	
	[SourceDatasetId] [int] NULL,
	[IsDatasetMigrated] [bit] NULL,
	[DatasetId] [int] NULL,
	[DatasetName] [varchar](1024) NULL,
	[DatasetMigrationMessage] [varchar](250) NULL,

	[SourceSchemaId] [int] NULL,
	[IsSchemaMigrated] [bit] NULL,
	[SchemaId] [int] NULL,
	[SchemaName] [varchar](250) NULL,
	[SchemaMigrationMessage] [varchar](250) NULL,

	[IsDataFlowMigrated] [bit] NULL,
	[DataFlowId] [int] NULL,
	[DataFlowName] [varchar](250) NULL,
	[DataFlowMigrationMessage] [varchar](250) NULL,

	[IsSchemaRevisionMigrated] [bit] NULL,
	[SchemaRevisionId] [int] NULL,
	[SchemaRevisionName] [varchar](250) NULL,
	[SchemaRevisionMigrationMessage] [varchar](250) NULL
    PRIMARY KEY CLUSTERED 
(
	[MigrationHistoryDetailId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY],
 CONSTRAINT [FK_MigrationHistoryDetail_MigrationHistory_MigrationHistoryId] FOREIGN KEY ([MigrationHistoryId]) REFERENCES [MigrationHistory]([MigrationHistoryId]),
 CONSTRAINT [FK_MigrationHistoryDetail_Dataset_SourceDatasetId] FOREIGN KEY ([SourceDatasetId]) REFERENCES [Dataset]([Dataset_ID]),
 CONSTRAINT [FK_MigrationHistoryDetail_Schema_SourceSchemaId] FOREIGN KEY ([SourceSchemaId]) REFERENCES [Schema]([Schema_Id]),
 CONSTRAINT [FK_MigrationHistoryDetail_Dataset_DatasetId] FOREIGN KEY ([DatasetId]) REFERENCES [Dataset]([Dataset_ID]),
 CONSTRAINT [FK_MigrationHistoryDetail_DataFlow_DataFlowId] FOREIGN KEY ([DataFlowId]) REFERENCES [DataFlow]([Id]),
 CONSTRAINT [FK_MigrationHistoryDetail_Schema_SchemaId] FOREIGN KEY ([SchemaId]) REFERENCES [Schema]([Schema_Id]),
 CONSTRAINT [FK_MigrationHistoryDetail_SchemaRevision_SchemaId] FOREIGN KEY ([SchemaRevisionId]) REFERENCES [SchemaRevision]([SchemaRevision_Id])
) ON [PRIMARY]
GO
