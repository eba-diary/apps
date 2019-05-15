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
insert into DataSource
select 
	'Default HSZ Drop Location',
	'Default HSZ Drop Location',
	'file:///c:/tmp/DatasetLoader/hsz/',
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
	null
END
