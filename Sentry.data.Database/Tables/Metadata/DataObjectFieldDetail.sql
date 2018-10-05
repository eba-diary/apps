CREATE TABLE [dbo].[DataObjectFieldDetail]
(
	[DataObjectFieldDetail_ID] [int] IDENTITY(1,1) NOT NULL,
	[DataObjectField_ID] [int] NULL,
	[DataTag_ID] [int] NULL,
	[DataObjectFieldDetailCreate_DTM] [datetime] NULL,
	[DataObjectFieldDetailChange_DTM] [datetime] NULL,
	[DataObjectFieldDetailType_CDE] [varchar](100) NULL,
	[DataObjectFieldDetailType_VAL] [varchar](max) NULL,
	[LastUpdt_DTM] [datetime] NULL,
	[BusFieldKey] [varchar](1000) NULL,
PRIMARY KEY CLUSTERED 
(
	[DataObjectFieldDetail_ID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
