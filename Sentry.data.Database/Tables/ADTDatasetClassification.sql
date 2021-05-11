CREATE TABLE [dbo].[ADTDatasetClassification]
(
	[Id] int NOT NULL IDENTITY PRIMARY KEY,
	[Category_NME] VARCHAR(255) NOT NULL, 
    [Dataset_NME] VARCHAR(1024) NOT NULL, 
    [Dataset_ID] INT NOT NULL, 
    [Schema_NME] VARCHAR(1024) NOT NULL, 
    [Schema_ID] INT NOT NULL, 
    [Storage_CDE] INT NOT NULL, 
    [Delivery_TYP] VARCHAR(25) NOT NULL, 
    [Create_DTM] DATETIME NOT NULL DEFAULT GETDATE()
)
