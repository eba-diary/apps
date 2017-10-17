CREATE TABLE [dbo].[DatasetFile]
(
	[DatasetFile_Id] INT NOT NULL PRIMARY KEY IDENTITY, 
    [File_NME] VARCHAR(250) NOT NULL, 
    [Dataset_ID] INT NOT NULL, 
    [UploadUser_NME] VARCHAR(100) NOT NULL, 
    [Create_DTM] DATETIME NULL, 
    [Modified_DTM] DATETIME NULL, 
    [FileLocation] VARCHAR(250) NOT NULL, 
    [DatasetFileConfig_ID] INT NULL, 
    [ParentDatasetFile_ID] INT NULL, 
    [Version_ID] NVARCHAR(250) NULL, 
    CONSTRAINT [FK_DatasetFile_Dataset] FOREIGN KEY (Dataset_ID) REFERENCES Dataset(Dataset_ID), 
    CONSTRAINT [FK_DatasetFile_DatasetFileConfigs] FOREIGN KEY ([DatasetFileConfig_ID]) REFERENCES [DatasetFileConfigs]([Config_ID]) 
)
