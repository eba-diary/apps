CREATE TABLE [dbo].[Dataset_DatasetFunction]
(
	[Dataset_Id] INT NOT NULL , 
    [Function_Id] INT NOT NULL,
	CONSTRAINT [FK_Dataset_Function_Dataset] FOREIGN KEY ([Dataset_Id]) REFERENCES [Dataset]([Dataset_ID]),
	CONSTRAINT [FK_Dataset_Function_DatasetFunction] FOREIGN KEY ([Function_Id]) REFERENCES [DatasetFunction]([DatasetFunction_Id])
)
