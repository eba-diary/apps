CREATE TABLE [dbo].[User] (
    [Id]                   INT           IDENTITY (1, 1) NOT NULL,
    --###  BEGIN Sentry.Data  A### - Code below is Sentry.Data-specific
    [Version]              INT           NOT NULL,
    [Created]              DATETIME      NOT NULL,
    [Ranking]              INT           NOT NULL,
    --###  END Sentry.Data  ### - Code above is Sentry.Data-specific
    [AssociateId] VARCHAR(6) NOT NULL, 
    CONSTRAINT [PK_User] PRIMARY KEY CLUSTERED ([Id] ASC)
);
