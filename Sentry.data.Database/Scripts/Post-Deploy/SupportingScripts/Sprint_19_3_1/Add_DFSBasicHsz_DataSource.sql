IF NOT EXISTS (Select * from DataSourceType where DiscrimatorValue = 'DFSBasicHsz')
BEGIN
	insert into DataSourceType
	select
		'Basic DFS HSZ',
		'DFS drop location controlled by data.sentry.com within HSZ',
		'DFSBasicHsz'
END

IF NOT EXISTS (Select * from DataSource where Source_NME = 'Default HSZ Drop Location')
BEGIN
	IF EXISTS (select 1 where @@SERVERNAME like '%fit-p-shardb-10%' OR @@SERVERNAME like '%fit-p-shardb-20%')
	BEGIN
		insert into DataSource
		select 
			'Default HSZ Drop Location',
			'Default HSZ Drop Location',
			'file:////sentry.com/securefs/datasetloader/',
			0,
			'DFSBasicHsz',
			1,
			'2304E10B-AF6',
			GETDATE(),
			GETDATE(),
			null,
			0,
			null,
			0,
			null,
			null,
			null,
			null,
			null,
			null,
			null,
			null,
			null,
			null,
			null,
			null,
			'072984',
			'072984',
			0,
			null
	END
	ELSE IF EXISTS (select 1 where @@SERVERNAME like '%fit-n-shardb-10%' OR @@SERVERNAME like '%fit-n-shardb-20%')
	BEGIN
		insert into DataSource
		select 
			'Default HSZ Drop Location',
			'Default HSZ Drop Location',
			'file:////sentry.com/securefs_nonprod/datasetloader/',
			0,
			'DFSBasicHsz',
			1,
			'2304E10B-AF6',
			GETDATE(),
			GETDATE(),
			null,
			0,
			null,
			0,
			null,
			null,
			null,
			null,
			null,
			null,
			null,
			null,
			null,
			null,
			null,
			null,
			'072984',
			'072984',
			0,
			null
	END
	ELSE IF EXISTS (select 1 where @@SERVERNAME like '%fit-n-shardb-11%' and DB_NAME() = 'SentryDatasets_NR')
	BEGIN
		insert into DataSource
		select 
			'Default HSZ Drop Location',
			'Default HSZ Drop Location',
			'file:////sentry.com/securefs_nonprod/datasetloader/nrtest/',
			0,
			'DFSBasicHsz',
			1,
			'2304E10B-AF6',
			GETDATE(),
			GETDATE(),
			null,
			0,
			null,
			0,
			null,
			null,
			null,
			null,
			null,
			null,
			null,
			null,
			null,
			null,
			null,
			null,
			'072984',
			'072984',
			0,
			null
	END
	ELSE IF EXISTS (select 1 where @@SERVERNAME like '%fit-n-shardb-11%' and DB_NAME() = 'SentryDatasets')
	BEGIN
		insert into DataSource
		select 
			'Default HSZ Drop Location',
			'Default HSZ Drop Location',
			'file:////sentry.com/securefs_nonprod/datasetloader/test/',
			0,
			'DFSBasicHsz',
			1,
			'2304E10B-AF6',
			GETDATE(),
			GETDATE(),
			null,
			0,
			null,
			0,
			null,
			null,
			null,
			null,
			null,
			null,
			null,
			null,
			null,
			null,
			null,
			null,
			'072984',
			'072984',
			0,
			null
	END
	ELSE
	BEGIN
		insert into DataSource
		select 
			'Default HSZ Drop Location',
			'Default HSZ Drop Location',
			'file:///c:/tmp/datasetloader/hsz/',
			0,
			'DFSBasicHsz',
			1,
			'2304E10B-AF6',
			GETDATE(),
			GETDATE(),
			null,
			0,
			null,
			0,
			null,
			null,
			null,
			null,
			null,
			null,
			null,
			null,
			null,
			null,
			null,
			null,
			'072984',
			'072984',
			0,
			null
	END
END

select DB_NAME()

