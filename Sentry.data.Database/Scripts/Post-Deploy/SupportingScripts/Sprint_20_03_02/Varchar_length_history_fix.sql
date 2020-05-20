/* When existing value is 0 or null
	 Set these records to 8000 */
Update SchemaField
set FieldLength = 8000
from 
(
	select Field_ID as 'Fid' 
	from SchemaField 
	where 
		type = 'VARCHAR' 
		and (FieldLength <= 0 or FieldLength is null)
) x
where Field_Id = x.Fid


/* When existing value is greater than 65535
	Set these values to max 65535 */
Update SchemaField
set FieldLength = 65535
from 
(
	select Field_ID as 'Fid' 
	from schemaField 
	where 
		type = 'VARCHAR' 
		and (FieldLength > 65535)
) x
where Field_Id = x.Fid