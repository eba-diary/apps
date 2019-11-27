CREATE TABLE [dbo].[DatasetCategory]
(
    [Dataset_Id] INT NOT NULL, 
    [Category_Id] INT NOT NULL, 
    CONSTRAINT [FK_DatasetCategory_Dataset] FOREIGN KEY ([Dataset_Id]) REFERENCES [Dataset]([Dataset_ID]), 
    CONSTRAINT [FK_DatasetCategory_Category] FOREIGN KEY ([Category_Id]) REFERENCES [Category]([Id])
)