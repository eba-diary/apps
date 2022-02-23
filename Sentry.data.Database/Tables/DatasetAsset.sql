CREATE TABLE [dbo].[DatasetAsset]
(
	[DatasetAsset_ID] INT IDENTITY(1,1) NOT NULL, 
    [SaidKey_CDE] CHAR(4) NOT NULL, 
    [Security_ID] UNIQUEIDENTIFIER NOT NULL, 
    CONSTRAINT [PK_DatasetAsset] PRIMARY KEY ([DatasetAsset_ID]),
    CONSTRAINT [AK1_DatasetAsset] UNIQUE ([SaidKey_CDE]),
    CONSTRAINT [FK_DatasetAsset_Security] FOREIGN KEY ([Security_ID]) REFERENCES [Security]([Security_ID])
)
