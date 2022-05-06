CREATE TABLE [dbo].[UserFavorite]
(
	[UserFavoriteId] INT NOT NULL PRIMARY KEY IDENTITY, 
    [AssociateId] CHAR(6) NOT NULL, 
    [FavoriteType] VARCHAR(50) NOT NULL, 
    [FavoriteEntityId] INT NOT NULL, 
    [Sequence] INT NOT NULL DEFAULT 0, 
    [CreateDate] DATETIME NOT NULL DEFAULT GETDATE()
)
