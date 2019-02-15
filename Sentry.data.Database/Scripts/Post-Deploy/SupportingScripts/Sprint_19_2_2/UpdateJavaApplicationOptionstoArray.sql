Update RetrieverJob
Set JobOptions = JSON_MODIFY(JobOptions,'$.JavaAppOptions.Arguments',JSON_QUERY(N'["' + x.args + '"]'))
from (
select Job_ID as 'ID', JSON_VALUE(JobOptions, '$.JavaAppOptions.Arguments') as 'args'
from Retrieverjob job
where ISJSON(JobOptions)>0
and JSON_VALUE(JobOptions, '$.JavaAppOptions.Arguments') IS NOT NULL
) x
where Job_ID = x.ID