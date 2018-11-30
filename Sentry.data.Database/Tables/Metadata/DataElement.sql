CREATE TABLE [dbo].[DataElement]
([DataElement_ID] [int] IDENTITY(1,1) NOT NULL,
	[DataTag_ID] [int] NULL,
	[DataElement_NME] [varchar](256) NULL,
	[DataElement_DSC] [varchar](max) NULL,
	[DataElement_CDE] [char](1) NULL,
	[DataElementCode_DSC] [varchar](50) NULL,
	[DataElementCreate_DTM] [datetime] NULL,
	[DataElementChange_DTM] [datetime] NULL,
	[LastUpdt_DTM] [datetime] NULL,
	[DataAsset_ID] [int] NULL,
	[BusElementKey] [varchar](1000) NULL,
[Config_ID] INT NULL, 
    PRIMARY KEY CLUSTERED 
(
	[DataElement_ID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
