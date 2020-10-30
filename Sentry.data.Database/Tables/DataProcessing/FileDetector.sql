CREATE TABLE [dbo].[FileDetector](
	[FileDetectorId] [int] IDENTITY(1,1) NOT NULL,
	[FilePath] [varchar](1000) NULL,
	[FileDTM] [datetime] NULL,
	[CreatedDTM] [datetime] NULL
);
