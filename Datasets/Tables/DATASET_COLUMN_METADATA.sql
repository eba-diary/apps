CREATE TABLE [dbo].[DATASET_COLUMN_METADATA] (
    [DatasetColumnMetadata_ID] INT           IDENTITY (1, 1) NOT NULL,
    [DatasetFileMetadata_ID]   INT           NOT NULL,
    [File_NME]                 VARCHAR (100) NULL,
    [Column_NME]               VARCHAR (50)  NULL,
    [Column_DSC]               VARCHAR (256) NULL
);

