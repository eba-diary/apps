Update DataSource Set Source_NME = 'DFS Drop Location', Source_DSC = 'DFS drop location monitored by data processing platform'
from (
select DSrc.DataSource_Id as 'Id' from DataSource DSrc
join DataSourceType DSrcType on
	DSrc.SourceType_IND = DSrcType.DiscrimatorValue
where DSrcType.Name = 'Basic DataFlow DFS'
) x
where DataSource_ID = x.Id