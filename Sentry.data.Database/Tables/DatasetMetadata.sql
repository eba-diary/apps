CREATE TABLE [dbo].[DatasetMetadata](
	[DatasetMetadata_ID] [int] IDENTITY(1,1) NOT NULL,
	[Dataset_ID] [int] NOT NULL,
	[IsColumn_IND] [bit] NOT NULL,
	[Metadata_NME] [varchar](128) NOT NULL,
	[Metadata_VAL] [varchar](4096) NULL,
PRIMARY KEY CLUSTERED 
(
	[DatasetMetadata_ID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]
GO

ALTER TABLE [dbo].[DatasetMetadata]  WITH CHECK ADD  CONSTRAINT [FK_DatasetMetadata_Dataset] FOREIGN KEY([Dataset_ID])
REFERENCES [dbo].[Dataset] ([Dataset_ID])
GO
