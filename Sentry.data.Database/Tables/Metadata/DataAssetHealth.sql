CREATE TABLE [dbo].[DataAssetHealth]
(
	[DataAsset_NME] [varchar](50) NULL,
	[AssetUpdt_DTM] [datetime] NULL,
	[Server_NME] [varchar](50) NULL,
	[Cube_NME] [varchar](50) NULL,
	[SourceSystem_VAL] [varchar](50) NULL,
	[LastUpdt_DTM] [datetime] NULL CONSTRAINT DF_LastUpdt_DTM DEFAULT GETDATE(),
	[DataAssetHealth_ID] [int] IDENTITY(1,1) NOT NULL
)
