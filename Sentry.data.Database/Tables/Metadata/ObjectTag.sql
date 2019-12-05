CREATE TABLE [dbo].[ObjectTag]
(
    [TagId] INT NOT NULL, 
    [DatasetId] INT NOT NULL, 
    CONSTRAINT [FK_ObjectTag_Dataset] FOREIGN KEY ([DatasetId]) REFERENCES [Dataset]([Dataset_ID]), 
    CONSTRAINT [FK_ObjectTag_Tag] FOREIGN KEY ([TagId]) REFERENCES [Tag]([TagId])
)
