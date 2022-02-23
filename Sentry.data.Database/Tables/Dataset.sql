﻿CREATE TABLE [dbo].[Dataset](
	[Dataset_ID] [int] IDENTITY(1,1) NOT NULL,
	[Dataset_NME] [varchar](1024) NOT NULL,
	[Dataset_DSC] [varchar](4096) NULL,
	[FileCreator_NME] [varchar](128) NOT NULL,
	[UploadedBy_NME] [varchar](128) NOT NULL,
	[Origination_CDE] [varchar](16) NULL,
	[Dataset_DTM] [datetime] NOT NULL,
	[FileChanged_DTM] [datetime] NOT NULL,
	[FileUploaded_DTM] [datetime] NULL,
	[S3_KEY] [varchar](1024) NULL,
    [Display_IND] BIT NOT NULL, 
    [Information_DSC] NVARCHAR(MAX) NULL, 
    [Metadata] VARCHAR(MAX) NULL, 
    [Dataset_TYP] VARCHAR(3) NULL, 
    [DataClassification_CDE] INT NOT NULL DEFAULT 0 , 
    [PrimaryContact_ID] VARCHAR(8) NOT NULL DEFAULT '000000', 
    [IsSecured_IND] BIT NOT NULL DEFAULT 0 , 
    [Security_ID] UNIQUEIDENTIFIER NULL, 
    [DeleteInd] BIT NOT NULL DEFAULT 0, 
    [DeleteIssuer] VARCHAR(10) NULL, 
    [DeleteIssueDTM] DATETIME NOT NULL, 
    [ObjectStatus] INT NOT NULL DEFAULT 1, 
    [SaidKeyCode] VARCHAR(10) NULL, 
    [NamedEnvironment] VARCHAR(25) NULL, 
    [NamedEnvironmentType] VARCHAR(25) NULL, 
    [DatasetAsset_ID] INT NULL, 
    PRIMARY KEY CLUSTERED 
(
	[Dataset_ID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY],
 CONSTRAINT [FK_Dataset_Security] FOREIGN KEY ([Security_ID]) REFERENCES [Security]([Security_ID]),
 CONSTRAINT [FK_Dataset_DatasetAsset] FOREIGN KEY ([DatasetAsset_ID]) REFERENCES [DatasetAsset]([DatasetAsset_ID])
) ON [PRIMARY]
GO
EXEC sp_addextendedproperty @name = N'MS_Description',
    @value = N'The Quartermaster Named Environment that this Dataset is for',
    @level0type = N'SCHEMA',
    @level0name = N'dbo',
    @level1type = N'TABLE',
    @level1name = N'Dataset',
    @level2type = N'COLUMN',
    @level2name = N'NamedEnvironment'
GO
EXEC sp_addextendedproperty @name = N'MS_Description',
    @value = N'The Quartermaster Named Environment Type (prod or nonprod) that this Dataset is for',
    @level0type = N'SCHEMA',
    @level0name = N'dbo',
    @level1type = N'TABLE',
    @level1name = N'Dataset',
    @level2type = N'COLUMN',
    @level2name = N'NamedEnvironmentType'