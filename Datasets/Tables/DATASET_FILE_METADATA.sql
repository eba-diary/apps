CREATE TABLE [dbo].[DATASET_FILE_METADATA] (
    [DatasetFileMetadata_ID] INT           IDENTITY (1, 1) NOT NULL,
    [File_NME]               VARCHAR (100) NULL,
    [File_DSC]               VARCHAR (256) NULL,
    [FileCreator_NME]        VARCHAR (50)  NULL,
    [SentryOwner_NME]        VARCHAR (50)  NULL,
    [FileType_CDE]           CHAR (1)      NULL,
    [FileType_DSC]           VARCHAR (10)  NULL,
    [FileFormat_CDE]         CHAR (1)      NULL,
    [FileFormat_DSC]         VARCHAR (10)  NULL,
    [File_DTM]               DATE          NULL,
    [FileChange_DTM]         DATE          NULL,
    [FileUpload_DTM]         DATE          NULL,
    [FileCreationFreq_CDE]   CHAR (1)      NULL,
    [FileCreationFreq_DSC]   VARCHAR (10)  NULL,
    [Record_CNT]             INT           NULL,
    [ETag_TXT]               VARCHAR (256) NULL
);


GO
CREATE NONCLUSTERED INDEX [IDX_DATASET_FILE_METADATA]
    ON [dbo].[DATASET_FILE_METADATA]([ETag_TXT] ASC);

