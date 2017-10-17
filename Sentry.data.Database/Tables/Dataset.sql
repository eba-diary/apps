CREATE TABLE [dbo].[Dataset](
	[Dataset_ID] [int] IDENTITY(1,1) NOT NULL,
	[Category_CDE] [varchar](64) NOT NULL,
	[Dataset_NME] [varchar](1024) NOT NULL,
	[Dataset_DSC] [varchar](4096) NULL,
	[FileCreator_NME] [varchar](128) NOT NULL,
	[SentryOwner_NME] [varchar](128) NULL,
	[UploadedBy_NME] [varchar](128) NOT NULL,
	[Origination_CDE] [varchar](16) NULL,
	[Dataset_DTM] [datetime] NOT NULL,
	[FileChanged_DTM] [datetime] NULL,
	[FileUploaded_DTM] [datetime] NOT NULL,
	[CreationFreq_DSC] [varchar](11) NULL,
	[S3_KEY] [varchar](1024) NOT NULL,
    [IsSensitive_IND] BIT NOT NULL, 
    [Category_ID] INT NOT NULL, 
    [Display_IND] BIT NOT NULL, 
    [DatasetScopeType_ID] INT NOT NULL, 
    [DatafilesToKeep_NBR] INT NOT NULL, 
    [DropLocation] VARCHAR(250) NOT NULL, 
    PRIMARY KEY CLUSTERED 
(
	[Dataset_ID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY], 
    CONSTRAINT [FK_Dataset_Category] FOREIGN KEY ([Category_ID]) REFERENCES [Category]([Id]), 
    CONSTRAINT [FK_Dataset_DatasetScopeTypes] FOREIGN KEY ([DatasetScopeType_ID]) REFERENCES [DatasetScopeTypes]([ScopeType_ID])
) ON [PRIMARY]
