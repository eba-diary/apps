CREATE TABLE [dbo].[MigrationHistory](
	[MigrationHistoryId] [int] IDENTITY(1,1) NOT NULL,
	[SourceNamedEnvironment] [varchar](25) NULL,
	[TargetNamedEnvironment] [varchar](25) NULL,
	[SourceDatasetId] [int] NULL,
	[TargetDatasetId] [int] NULL,
	[CreateDateTime] [datetime] NOT NULL
    PRIMARY KEY CLUSTERED 
(
	[MigrationHistoryId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY],
 CONSTRAINT [FK_MigrationHistory_Dataset_SourceDatasetId] FOREIGN KEY ([SourceDatasetId]) REFERENCES [Dataset]([Dataset_ID]),
 CONSTRAINT [FK_MigrationHistory_Dataset_TargetDatasetId] FOREIGN KEY ([TargetDatasetId]) REFERENCES [Dataset]([Dataset_ID])
) ON [PRIMARY]
GO
