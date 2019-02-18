CREATE TABLE [dbo].[BusinessAreaTile]
(
	[BusinessAreaTile_ID] INT NOT NULL PRIMARY KEY, 
    [Title_DSC] VARCHAR(255) NOT NULL, 
    [TileColor_DSC] VARCHAR(25) NOT NULL, 
    [Image_NME] VARCHAR(100) NOT NULL, 
    [Hyperlink_URL] VARCHAR(255) NOT NULL, 
    [Order_SEQ] INT NOT NULL, 
    [Hyperlink_DSC] VARCHAR(255) NOT NULL
)
