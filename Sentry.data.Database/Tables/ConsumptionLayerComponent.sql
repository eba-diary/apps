CREATE TABLE [dbo].[ConsumptionLayerComponent]
(
	[CLC_ID] INT NOT NULL PRIMARY KEY IDENTITY, 
    [DataAsset_ID] INT NOT NULL, 
    [CLType_ID] INT NOT NULL, 
    CONSTRAINT [FK_ConsumptionLayerComponent_DataAsset] FOREIGN KEY ([DataAsset_ID]) REFERENCES [DataAsset]([DataAsset_ID]), 
    CONSTRAINT [FK_ConsumptionLayerComponent_ConsumptionLayerType] FOREIGN KEY ([CLType_ID]) REFERENCES [ConsumptionLayerType]([CLType_ID])
)
