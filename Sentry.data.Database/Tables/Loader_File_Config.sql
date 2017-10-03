CREATE TABLE [dbo].[Loader_File_Config]
(
	[Id] INT NOT NULL PRIMARY KEY, 
    [SystemConfig_ID] INT NOT NULL, 
    [DatasetMetadata_ID] INT NOT NULL, 
    CONSTRAINT [FK_Loader_File_Config_To_LoaderSystemConfig] FOREIGN KEY ([SystemConfig_ID]) REFERENCES [Loader_System_Config]([Id]), 
    CONSTRAINT [FK_Loader_File_Config_To_LoaderDatasetMetadata] FOREIGN KEY ([DatasetMetadata_ID]) REFERENCES [Loader_Dataset_Metadata]([Id])
)
