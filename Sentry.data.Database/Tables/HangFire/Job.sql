CREATE TABLE [HangFire7].[Job] (
    [Id]             BIGINT         IDENTITY (1, 1) NOT NULL,
    [StateId]        BIGINT         NULL,
    [StateName]      NVARCHAR (20)  NULL,
    [InvocationData] NVARCHAR (MAX) NOT NULL,
    [Arguments]      NVARCHAR (MAX) NOT NULL,
    [CreatedAt]      DATETIME       NOT NULL,
    [ExpireAt]       DATETIME       NULL,
    CONSTRAINT [PK_HangFire_Job] PRIMARY KEY CLUSTERED ([Id] ASC)
);


GO
CREATE NONCLUSTERED INDEX [IX_HangFire_Job_ExpireAt]
    ON [HangFire7].[Job]([ExpireAt] ASC)
    INCLUDE([StateName]) WHERE ([ExpireAt] IS NOT NULL);


GO
CREATE NONCLUSTERED INDEX [IX_HangFire_Job_StateName]
    ON [HangFire7].[Job]([StateName] ASC) WHERE ([StateName] IS NOT NULL);

