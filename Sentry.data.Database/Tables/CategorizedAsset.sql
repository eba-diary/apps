CREATE TABLE [dbo].[CategorizedAsset] (
    [AssetId]     INT NOT NULL,
    [CategoryId] INT NOT NULL,
    CONSTRAINT [FK_CategorizedAsset_Category] FOREIGN KEY ([CategoryId]) REFERENCES [dbo].[Category] ([Id]),
    CONSTRAINT [FK_CategorizedAsset_Asset] FOREIGN KEY ([AssetId]) REFERENCES [dbo].[Asset] ([Id])
);

