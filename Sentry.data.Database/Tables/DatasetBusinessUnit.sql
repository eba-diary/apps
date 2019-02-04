CREATE TABLE [dbo].[DatasetBusinessUnit]
(
	[Dataset_Id] INT NOT NULL , 
    [BusinessUnit_Id] INT NOT NULL,
	CONSTRAINT [FK_DatasetBusinessUnit_Dataset] FOREIGN KEY ([Dataset_Id]) REFERENCES [Dataset]([Dataset_Id]),
	CONSTRAINT [FK_DatasetBusinessUnit_BusinessUnit] FOREIGN KEY ([BusinessUnit_Id]) REFERENCES [BusinessUnit]([BusinessUnit_Id])
)
