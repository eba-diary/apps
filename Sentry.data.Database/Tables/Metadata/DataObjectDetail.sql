CREATE TABLE [dbo].[DataObjectDetail]
(
	[DataObjectDetail_ID] [int] IDENTITY(1,1) NOT NULL,
	[DataObject_ID] [int] NULL,
	[DataObjectDetailCreate_DTM] [datetime] NULL,
	[DataObjectDetailChange_DTM] [datetime] NULL,
	[DataObjectDetailType_CDE] [varchar](100) NULL,
	[DataObjectDetailType_VAL] [varchar](max) NULL,
	[LastUpdt_DTM] [datetime] NULL,
	[BusObjectKey] [varchar](1000) NULL,
PRIMARY KEY CLUSTERED 
(
	[DataObjectDetail_ID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]

GO

CREATE NONCLUSTERED INDEX [IDX1_DataObjectDetail] ON [dbo].[DataObjectDetail]
(
       [DataObject_ID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
