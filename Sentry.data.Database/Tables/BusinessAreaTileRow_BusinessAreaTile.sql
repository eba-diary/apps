CREATE TABLE [dbo].[BusinessAreaTileRow_BusinessAreaTile]
(
	[BusinessAreaTileRow_ID] INT NOT NULL, 
    [BusinessAreaTile_ID] INT NOT NULL,
	CONSTRAINT [FK_BusinessAreaTileRow_BusinessAreaTile_BusinessAreaTileRow] FOREIGN KEY ([BusinessAreaTileRow_ID]) REFERENCES [BusinessAreaTileRow]([BusinessAreaTileRow_ID]),
	CONSTRAINT [FK_BusinessAreaTileRow_BusinessAreaTile_BusinessAreaTile] FOREIGN KEY ([BusinessAreaTile_ID]) REFERENCES [BusinessAreaTile]([BusinessAreaTile_ID])
)
