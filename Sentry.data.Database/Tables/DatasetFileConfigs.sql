CREATE TABLE [dbo].[DatasetFileConfigs] (
    [Config_ID]             INT            IDENTITY (1, 1) NOT NULL,
    [Dataset_ID]            INT            NOT NULL,
    [SearchCriteria]        VARCHAR (250)  NULL,
    [DropLocationType]      VARCHAR (250)  NULL,
    [DropPath]              VARCHAR (250)  NOT NULL,
    [RegexSearch_IND]       BIT            NULL,
    [OverwriteDatafile_IND] BIT            NULL,
    [FileType_ID]           INT            NOT NULL,
    [Config_NME]            VARCHAR (250)  NOT NULL,
    [Config_DSC]            VARCHAR (250)  NULL,
    [IsGeneric]             BIT            NOT NULL,
    [TargetFile_NME]        VARCHAR (250)  NULL,
    [CreationFreq_DSC]      NVARCHAR (100) NULL,
    [DatasetScopeType_ID]   INT            NOT NULL,
    [CurrentFile_IND]       BIT            NULL,
    CONSTRAINT [PK_DatasetFileConfigs] PRIMARY KEY CLUSTERED ([Config_ID] ASC),
    CONSTRAINT [FK_DatasetFileConfigs_Dataset] FOREIGN KEY ([Dataset_ID]) REFERENCES [dbo].[Dataset] ([Dataset_ID]),
    CONSTRAINT [FK_DatasetFileConfigs_DatasetScopeTypes] FOREIGN KEY ([DatasetScopeType_ID]) REFERENCES [dbo].[DatasetScopeTypes] ([ScopeType_ID])
);




