CREATE TABLE [dbo].[DatasetDetailTabMetrics]
(
	[UserId] VARCHAR(8) NOT NULL PRIMARY KEY, 
    [DatasetId] INT NOT NULL, 
    [SchemaAboutClicks] INT NOT NULL DEFAULT 0, 
    [SchemaColumnsClicks] INT NOT NULL DEFAULT 0, 
    [DataFilesClicks] INT NOT NULL DEFAULT 0, 
    [DataPreviewClicks] INT NOT NULL DEFAULT 0
)
