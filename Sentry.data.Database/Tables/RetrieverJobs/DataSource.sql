﻿CREATE TABLE [dbo].[DataSource] (
    [DataSource_Id]     INT            IDENTITY (1, 1) NOT NULL,
    [Source_NME]        NVARCHAR (255) NOT NULL,
    [Source_DSC]        NVARCHAR (255) NULL,
    [BaseUri]           NVARCHAR (255) NOT NULL,
    [IsUriEditable_IND] BIT            NOT NULL,
    [SourceType_IND]    NVARCHAR (50)  NULL,
    [SourceAuth_ID]     INT            NOT NULL,
    [KeyCode_CDE]       NVARCHAR (20)  NULL,
    [Created_DTM]       DATETIME       NOT NULL,
    [Modified_DTM]      DATETIME       NOT NULL,
    [Bucket_NME] NVARCHAR(250) NULL, 
    [PortNumber] INT NULL, 
    [HostFingerPrintKey] NVARCHAR(MAX) NULL, 
    [IsUserPassRequired] BIT NULL, 
    [AuthHeaderName] NVARCHAR(MAX) NULL, 
    [AuthHeaderValue] NVARCHAR(MAX) NULL, 
    [IVKey] NVARCHAR(MAX) NULL, 
    [RequestHeaders] NVARCHAR(MAX) NULL, 
    PRIMARY KEY CLUSTERED ([DataSource_Id] ASC),
    CONSTRAINT [FK_DataSource_AuthenticationType] FOREIGN KEY ([SourceAuth_ID]) REFERENCES [dbo].[AuthenticationType] ([Auth_Id])
);


