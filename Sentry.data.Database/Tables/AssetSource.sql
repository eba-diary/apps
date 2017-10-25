CREATE TABLE [dbo].[AssetSource]
(
	[Source_ID] INT NOT NULL PRIMARY KEY IDENTITY, 
    [SourceDisplay_NME] VARCHAR(50) NULL, 
    [Source_DSC] VARCHAR(100) NULL, 
    [MetadataRepositorySrcSys_VAL] VARCHAR(50) NOT NULL, 
    [DataAsset_ID] INT NOT NULL, 
    [IsVisible_IND] BIT NOT NULL, 
    CONSTRAINT [FK_AssetSource_DataAsset] FOREIGN KEY ([DataAsset_ID]) REFERENCES [DataAsset]([DataAsset_ID]) 
)
