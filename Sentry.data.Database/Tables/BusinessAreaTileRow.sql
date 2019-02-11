CREATE TABLE [dbo].[BusinessAreaTileRow]
(
	[BusinessAreaTileRow_ID] INT IDENTITY(1,1) NOT NULL PRIMARY KEY, 
    [NbrOfColumns_CNT] INT NOT NULL, 
    [BusinessArea_ID] INT NOT NULL, 
    [Order_SEQ] INT NOT NULL,
	CONSTRAINT [FK_BusinessAreaTileRow_BusinessArea] FOREIGN KEY ([BusinessArea_ID]) REFERENCES [BusinessArea]([BusinessArea_ID])
)
