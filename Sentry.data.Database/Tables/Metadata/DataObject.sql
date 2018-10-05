CREATE TABLE [dbo].[DataObject]
(
	[DataObject_ID] [int] IDENTITY(1,1) NOT NULL,
	[DataElement_ID] [int] NULL,
	[DataTag_ID] [int] NULL,
	[Reviewer_ID] [int] NULL,
	[DataObject_NME] [varchar](256) NULL,
	[DataObject_DSC] [varchar](max) NULL,
	[DataObjectParent_ID] [int] NULL,
	[DataObject_CDE] [char](1) NULL,
	[DataObjectCode_DSC] [varchar](100) NULL,
	[DataObjectCreate_DTM] [datetime] NULL,
	[DataObjectChange_DTM] [datetime] NULL,
	[LastUpdt_DTM] [datetime] NULL,
	[BusElementKey] [varchar](1000) NULL,
	[BusObjectKey] [varchar](1000) NULL,
PRIMARY KEY CLUSTERED 
(
	[DataObject_ID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
