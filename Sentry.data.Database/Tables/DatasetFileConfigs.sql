CREATE TABLE [dbo].[DatasetFileConfigs] (
    [Config_ID]             INT            IDENTITY (1, 1) NOT NULL,
    [Dataset_ID]            INT            NOT NULL,
    [DropLocationType]      VARCHAR (250)  NULL,
    [FileType_ID]           INT            NOT NULL,
    [Config_NME]            VARCHAR (250)  NOT NULL,
    [Config_DSC]            VARCHAR(MAX)  NULL,
    [DatasetScopeType_ID]   INT            NOT NULL,
    [FileExtension_CDE] INT NOT NULL, 
    [DeleteInd] BIT NOT NULL, 
    [DeleteIssuer] VARCHAR(8) NULL, 
    [DeleteIssueDTM] VARCHAR(100) NULL, 
    [IsSchemaTracked] BIT NOT NULL, 
    [Schema_Id] INT NULL, 
    [ObjectStatus] INT NOT NULL DEFAULT 1, 
    CONSTRAINT [PK_DatasetFileConfigs] PRIMARY KEY CLUSTERED ([Config_ID] ASC),
    CONSTRAINT [FK_DatasetFileConfigs_Dataset] FOREIGN KEY ([Dataset_ID]) REFERENCES [dbo].[Dataset] ([Dataset_ID]),
    CONSTRAINT [FK_DatasetFileConfigs_DatasetScopeTypes] FOREIGN KEY ([DatasetScopeType_ID]) REFERENCES [dbo].[DatasetScopeTypes] ([ScopeType_ID]),
    CONSTRAINT [FK_DatasetFileConfigs_FileExtension] FOREIGN KEY ([FileExtension_CDE]) REFERENCES [dbo].[FileExtension] ([Extension_Id]), 
    CONSTRAINT [FK_DatasetFileConfigs_Schema] FOREIGN KEY ([Schema_Id]) REFERENCES [Schema]([Schema_Id])
);




