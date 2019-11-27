CREATE TABLE [dbo].[RetrieverJobArchive]
(
	[Archive_Id] INT NOT NULL, 
    [Job_Id] INT NULL, 
    [Job_Guid] UNIQUEIDENTIFIER NULL, 
    [RelativeUri_DSC]  NVARCHAR (255) NULL,
    [Schedule]         NVARCHAR (255) NULL,
    [DataSource_ID]    INT            NOT NULL,
    [Config_ID]        INT            NOT NULL,
    [IsCompressed_IND] BIT            NULL,
    [Compression_TYP]  CHAR (10)      NULL,
    [Created_DTM]      DATETIME       NOT NULL,
    [Modified_DTM]     DATETIME       NOT NULL,
    [IsGeneric_IND]    BIT            NULL,
    [JobOptions]       NVARCHAR (MAX) NULL,
	[IsEnabled] BIT NULL,
	PRIMARY KEY CLUSTERED ([Archive_Id] ASC),
	CONSTRAINT [FK_RetrieverJobArchive_RetrieverJob] FOREIGN KEY ([Job_Id]) REFERENCES [RetrieverJob]([Job_ID]),
	CONSTRAINT [FK_RetrieverJobArchive_DatasetFileConfigs] FOREIGN KEY ([Config_ID]) REFERENCES [dbo].[DatasetFileConfigs] ([Config_ID]),
    CONSTRAINT [FK_RetrieverJobArchive_DataSource] FOREIGN KEY ([DataSource_ID]) REFERENCES [dbo].[DataSource] ([DataSource_Id])
)
