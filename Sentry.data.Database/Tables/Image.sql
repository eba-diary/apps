CREATE TABLE [dbo].[Image]
(
	[ImageId] INT IDENTITY (1, 1) NOT NULL, 
    [ParentDataset] INT NOT NULL, 
    [ContentType] VARCHAR(50) NOT NULL, 
    [FileExtension] VARCHAR(10) NOT NULL, 
    [FileName] VARCHAR(250) NOT NULL, 
    [StorageBucketName] VARCHAR(100) NULL, 
    [StoragePrefix] VARCHAR(250) NOT NULL, 
    [StorageKey] VARCHAR(500) NOT NULL, 
    [UploadDate] DATETIME NOT NULL, 
    [Sort] INT NOT NULL, 
    CONSTRAINT [PK_Image] PRIMARY KEY CLUSTERED ([ImageId] ASC),
	CONSTRAINT [FK_Image_Dataset] FOREIGN KEY ([ParentDataset]) REFERENCES [dbo].[Dataset]([Dataset_ID])
)