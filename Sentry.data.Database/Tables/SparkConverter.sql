CREATE TABLE [dbo].[SparkConverter](
                [SparkConverter_ID] [int] IDENTITY(1,1) NOT NULL,
                [StartCheckTime] [datetime] NULL,
                [EndCheckTime] [datetime] NULL,
                [TotalCount] [int] NULL,
                [ProcessedCount] [int] NULL,
                [CreatedTime] [datetime] NULL,
                [UpdatedTime] [datetime] NULL,
PRIMARY KEY CLUSTERED 
(
                [SparkConverter_ID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]
