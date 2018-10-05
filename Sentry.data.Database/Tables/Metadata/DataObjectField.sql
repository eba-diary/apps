CREATE TABLE [dbo].[DataObjectField]
(
	[DataObjectField_ID] [int] IDENTITY(1,1) NOT NULL,
	[DataObject_ID] [int] NULL,
	[DataTag_ID] [int] NULL,
	[DataObjectField_NME] [varchar](256) NULL,
	[DataObjectField_DSC] [varchar](max) NULL,
	[DataObjectFieldCreate_DTM] [datetime] NULL,
	[DataObjectFieldChange_DTM] [datetime] NULL,
	[LastUpdt_DTM] [datetime] NULL,
	[BusObjectKey] [varchar](1000) NULL,
	[BusFieldKey] [varchar](1000) NULL,
PRIMARY KEY CLUSTERED 
(
	[DataObjectField_ID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
