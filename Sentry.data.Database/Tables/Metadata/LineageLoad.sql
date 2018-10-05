CREATE TABLE [dbo].[LineageLoad]
(
	LineageLoad_ID [int] IDENTITY(1,1) NOT NULL,
	[Asset] [varchar](max) NULL,
	[Element] [varchar](max) NULL,
	[ElementType] [varchar](max) NULL,
	[Object] [varchar](max) NULL,
	[ObjectType] [varchar](max) NULL,
	[ObjectField] [varchar](max) NULL,
	[SourceElement] [varchar](max) NULL,
	[SourceObject] [varchar](max) NULL,
	[SourceField] [varchar](max) NULL,
	[SourceFieldName] [varchar](max) NULL,
	[TransformationText] [varchar](max) NULL,
	[DisplayIndicator] [varchar](max) NULL,
	[ElementDSC] [varchar](max) NULL,
	[Modified] [datetime] NULL
)
