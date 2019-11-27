CREATE TABLE [dbo].[Dataset](
	[Dataset_ID] [int] IDENTITY(1,1) NOT NULL,
	[Dataset_NME] [varchar](1024) NOT NULL,
	[Dataset_DSC] [varchar](4096) NULL,
	[FileCreator_NME] [varchar](128) NOT NULL,
	[PrimaryOwner_ID] [varchar](8) NOT NULL,
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
    PRIMARY KEY CLUSTERED 
(
	[Dataset_ID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY],
 CONSTRAINT [FK_Dataset_Security] FOREIGN KEY ([Security_ID]) REFERENCES [Security]([Security_ID])
) ON [PRIMARY]
