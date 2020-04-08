CREATE TABLE [dbo].[DatasetFile_Conversion_Archive]
(
	[DatasetFile_Id] INT NOT NULL, 
    [File_NME] VARCHAR(250) NOT NULL, 
    [Dataset_ID] INT NOT NULL, 
    [UploadUser_NME] VARCHAR(100) NOT NULL, 
    [Create_DTM] DATETIME NULL, 
    [Modified_DTM] DATETIME NULL, 
    [FileLocation] VARCHAR(250) NOT NULL, 
    [DatasetFileConfig_ID] INT NULL, 
    [ParentDatasetFile_ID] INT NULL, 
    [Version_ID] NVARCHAR(250) NULL, 
    [isBundled_IND] BIT NOT NULL, 
    [Information_DSC] NVARCHAR(MAX) NULL, 
    [Size_AMT] BIGINT NULL, 
    [Schema_ID] INT NULL, 
    [SchemaRevision_ID] INT NULL, 
    [FlowExecutionGuid] VARCHAR(100) NULL, 
    [RunInstanceGuid] VARCHAR(100) NULL
)
