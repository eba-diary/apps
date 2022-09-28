CREATE TABLE [dbo].[RetrieverJob] (
    [Job_ID]           INT            IDENTITY (1, 1) NOT NULL,
    [RelativeUri_DSC]  NVARCHAR (255) NULL,
    [Schedule]         NVARCHAR (255) NULL,
    [DataSource_ID]    INT            NOT NULL,
    [Config_ID]        INT            NULL,
    [IsCompressed_IND] BIT            NULL,
    [Compression_TYP]  CHAR (10)      NULL,
    [Created_DTM]      DATETIME       NOT NULL,
    [Modified_DTM]     DATETIME       NOT NULL,
    [IsGeneric_IND]    BIT            NULL,
    [JobOptions]       NVARCHAR (MAX) NULL,
    [IsEnabled] BIT NULL, 
    [Job_Guid] UNIQUEIDENTIFIER NULL, 
    [Schema_ID] INT NULL, 
    [DataFlow_ID] INT NULL, 
    [ObjectStatus] INT NULL , 
    [DeleteIssuer] VARCHAR(10) NULL, 
    [DeleteIssueDTM] DATETIME NULL, 
    [ExecutionParameters] NVARCHAR(MAX) NULL, 
    PRIMARY KEY CLUSTERED ([Job_ID] ASC),
    CONSTRAINT [FK_RetrieverJob_DatasetFileConfigs] FOREIGN KEY ([Config_ID]) REFERENCES [dbo].[DatasetFileConfigs] ([Config_ID]),
    CONSTRAINT [FK_RetrieverJob_DataSource] FOREIGN KEY ([DataSource_ID]) REFERENCES [dbo].[DataSource] ([DataSource_Id])
);


