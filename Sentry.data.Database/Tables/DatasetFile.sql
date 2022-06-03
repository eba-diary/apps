CREATE TABLE [dbo].[DatasetFile]
(
	[DatasetFile_Id] INT NOT NULL PRIMARY KEY IDENTITY, 
    [File_NME] VARCHAR(250) NOT NULL, 
    [FileLocation] VARCHAR(250) NOT NULL, 
    [FileKey] VARCHAR(2000) NULL, 
    [FileBucket] VARCHAR(100) NULL, 
    [Version_ID] NVARCHAR(250) NULL,
    [ETag] VARCHAR(1000) NULL,
    [Dataset_ID] INT NOT NULL, 
    [DatasetFileConfig_ID] INT NULL, 
    [Schema_ID] INT NULL, 
    [SchemaRevision_ID] INT NULL, 
    [FlowExecutionGuid] VARCHAR(100) NULL, 
    [RunInstanceGuid] VARCHAR(100) NULL, 
    [ParentDatasetFile_ID] INT NULL,
    [UploadUser_NME] VARCHAR(100) NULL, 
    [isBundled_IND] BIT NOT NULL, 
    [Information_DSC] NVARCHAR(MAX) NULL, 
    [Size_AMT] BIGINT NULL, 
    [Modified_DTM] DATETIME NULL, 
    [Created_DTM] DATETIME NULL, 
    [ObjectStatus] INT NOT NULL DEFAULT 1,
    [OriginalFileName] VARCHAR(250) NULL,
    CONSTRAINT [FK_DatasetFile_Dataset] FOREIGN KEY (Dataset_ID) REFERENCES Dataset(Dataset_ID), 
    CONSTRAINT [FK_DatasetFile_DatasetFileConfigs] FOREIGN KEY ([DatasetFileConfig_ID]) REFERENCES [DatasetFileConfigs]([Config_ID]),
    CONSTRAINT [FK_DatasetFile_ObjectStatus] FOREIGN KEY (ObjectStatus) REFERENCES ObjectStatus(ObjectStatus_Id)
)

GO

CREATE INDEX [IX_DatasetFile_FlowExecutionGuid] ON [dbo].[DatasetFile] ([FlowExecutionGuid])
