﻿CREATE TABLE [dbo].[DatasetFileConfigs]
(
	[Config_ID] INT NOT NULL IDENTITY (1,1), 
    [DataFileConfig_ID] INT NOT NULL, 
    [Dataset_ID] INT NOT NULL, 
    [SearchCriteria] VARCHAR(250) NOT NULL, 
    [DropLocationType] VARCHAR(250) NOT NULL, 
    [DropPath] VARCHAR(250) NOT NULL, 
    [RegexSearch_IND] BIT NOT NULL, 
    [OverwriteDatafile_IND] BIT NOT NULL, 
    [VersionsToKeep_NBR] INT NOT NULL, 
    [FileType_ID] INT NOT NULL, 
    [Config_NME] VARCHAR(250) NOT NULL, 
    [Config_DSC] VARCHAR(250) NULL, 
    [IsGeneric] BIT NOT NULL, 
    [TargetFile_NME] VARCHAR(250) NULL, 
    CONSTRAINT [FK_DatasetFileConfigs_Dataset] FOREIGN KEY ([Dataset_ID]) REFERENCES [Dataset]([Dataset_ID]), 
    CONSTRAINT [PK_DatasetFileConfigs] PRIMARY KEY ([Config_ID]) 
)