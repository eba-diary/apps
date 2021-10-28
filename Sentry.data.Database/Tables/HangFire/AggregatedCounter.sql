﻿CREATE TABLE [HangFire7].[AggregatedCounter] (
    [Key]      NVARCHAR (100) NOT NULL,
    [Value]    BIGINT         NOT NULL,
    [ExpireAt] DATETIME       NULL,
    CONSTRAINT [PK_HangFire_CounterAggregated] PRIMARY KEY CLUSTERED ([Key] ASC)
);


GO
CREATE NONCLUSTERED INDEX [IX_HangFire_AggregatedCounter_ExpireAt]
    ON [HangFire7].[AggregatedCounter]([ExpireAt] ASC) WHERE ([ExpireAt] IS NOT NULL);

