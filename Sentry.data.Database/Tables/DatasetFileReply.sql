CREATE TABLE [dbo].[DatasetFileReply]
(
	[DatasetFileReply_Id] [int] IDENTITY(1,1) NOT NULL,
                [DatasetFile_Id] [int] NOT NULL,
                [Schema_ID] [int] NULL,
                [FileLocation] [varchar](250) NOT NULL,
                [ReplyStatus] [varchar](250) NOT NULL

PRIMARY KEY CLUSTERED 
(
                [DatasetFileReply_Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY], 
    [Dataset_ID] INT NULL
) ON [PRIMARY]
