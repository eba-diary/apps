CREATE TABLE [dbo].[DataElementDetail]
(
	[DataElementDetail_ID] [int] IDENTITY(1,1) NOT NULL,
	[DataElement_ID] [int] NULL,
	[DataElementDetailCreate_DTM] [datetime] NULL,
	[DataElementDetailChange_DTM] [datetime] NULL,
	[DataElementDetailType_CDE] [varchar](100) NULL,
	[DataElementDetailType_VAL] [varchar](max) NULL,
	[LastUpdt_DTM] [datetime] NULL,
	[BusElementKey] [varchar](1000) NULL,
PRIMARY KEY CLUSTERED 
(
	[DataElementDetail_ID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
