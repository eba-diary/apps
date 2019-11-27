CREATE TABLE [dbo].[Favorites]
(
	[FavoriteId] INT NOT NULL PRIMARY KEY IDENTITY, 
    [DatasetId] INT NOT NULL, 
    [UserId] CHAR(6) NOT NULL, 
    [Created] DATETIME NOT NULL, 
    [Sequence] INT NOT NULL DEFAULT 0, 
    CONSTRAINT [FK_Favorites_Dataset] FOREIGN KEY ([DatasetId]) REFERENCES [Dataset]([Dataset_ID])
)

GO

CREATE INDEX [IX_Favorites_Column] ON [dbo].[Favorites] ([DatasetId])
