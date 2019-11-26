CREATE TABLE [dbo].[DatasetFileParquet](
                [DatasetFileParquet_Id] [int] IDENTITY(1,1) NOT NULL,
                [DatasetFile_Id] [int] NOT NULL,
                [Schema_ID] [int] NULL,
                [FileLocation] [varchar](250) NOT NULL,
[Dataset_ID] INT NULL, 
    PRIMARY KEY CLUSTERED 
(
                [DatasetFileParquet_Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]
