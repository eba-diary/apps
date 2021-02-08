CREATE TABLE [dbo].[DatasetFileConverted]
(
	[DatasetFileConvertedID] [int] IDENTITY(1,1) NOT NULL,
	[FileNME] [varchar](250) NULL,
	[S3FileBucket] [varchar](250) NULL,
	[S3FileKey] [varchar](1000) NULL,
	[S3VersionID] [nvarchar](250) NULL,
	[S3FileSizeAMT] [bigint] NULL,
	[S3FileMetadata] [nvarchar](max) NULL,
	[DatasetID] [int] NULL,
	[SchemaID] [int] NULL,
	[SchemaRevisionID] [int] NULL,
	[FlowExecutionGUID] [varchar](100) NULL,
	[RunInstanceGUID] [varchar](100) NULL,
	[CreateDTM] [datetime] NULL
)
