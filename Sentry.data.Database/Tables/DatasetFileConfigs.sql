CREATE TABLE [dbo].[DatasetFileConfigs] (
    [Config_ID]             INT            IDENTITY (1, 1) NOT NULL,
    [Dataset_ID]            INT            NOT NULL,
    [DropLocationType]      VARCHAR (250)  NULL,
    [FileType_ID]           INT            NOT NULL,
    [Config_NME]            VARCHAR (250)  NOT NULL,
    [Config_DSC]            VARCHAR (250)  NULL,
    [IsGeneric]             BIT            NOT NULL,
    [DatasetScopeType_ID]   INT            NOT NULL,
    [FileExtension_CDE] INT NOT NULL, 
    [DataElement_ID] INT NULL, 
    CONSTRAINT [PK_DatasetFileConfigs] PRIMARY KEY CLUSTERED ([Config_ID] ASC),
    CONSTRAINT [FK_DatasetFileConfigs_Dataset] FOREIGN KEY ([Dataset_ID]) REFERENCES [dbo].[Dataset] ([Dataset_ID]),
    CONSTRAINT [FK_DatasetFileConfigs_DatasetScopeTypes] FOREIGN KEY ([DatasetScopeType_ID]) REFERENCES [dbo].[DatasetScopeTypes] ([ScopeType_ID]),
    CONSTRAINT [FK_DatasetFileConfigs_FileExtension] FOREIGN KEY ([FileExtension_CDE]) REFERENCES [dbo].[FileExtension] ([Extension_ID])
);




