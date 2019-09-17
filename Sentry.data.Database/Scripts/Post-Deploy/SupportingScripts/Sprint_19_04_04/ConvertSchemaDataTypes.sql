/*
	This script will convert existing schema datatypes to a
	new "trimmed" down list of datatypes.
*/

/* Convert to DECIMAL */
UPDATE DataObjectFieldDetail
SET DataObjectFieldDetailType_VAL = 'DECIMAL'
FROM (
SELECT DOFD_Datatype.DataObjectField_ID AS 'fieldId', DOFD_Datatype.DataObjectFieldDetail_ID AS 'fielddetail_id', DOFD_Datatype.DataObjectFieldDetailType_VAL AS 'val'
FROM DataObjectField DOF
LEFT JOIN DataObjectFieldDetail DOFD_Datatype ON
	DOF.DataObjectField_ID = DOFD_Datatype.DataObjectField_ID AND
	DOFD_Datatype.DataObjectFieldDetailType_CDE = 'Datatype_TYP'
LEFT JOIN DataObject DO ON
	DOF.DataObject_ID = DO.DataObject_ID
LEFT JOIN DataElement DE ON
	DO.DataElement_ID = DE.DataElement_ID
WHERE DE.Config_ID IS NOT NULL AND DOFD_Datatype.DataObjectFieldDetailType_VAL IN ('FLOAT', 'DECIMAL')
)x
WHERE DataObjectFieldDetail_ID = x.fielddetail_id AND DataObjectField_ID = x.fieldId

/* Convert to TIMESTAMP */
update DataObjectFieldDetail
set DataObjectFieldDetailType_VAL = 'TIMESTAMP'
from (
select DOFD_Datatype.DataObjectField_ID as 'fieldId', DOFD_Datatype.DataObjectFieldDetail_ID as 'fielddetail_id', DOFD_Datatype.DataObjectFieldDetailType_VAL as 'val'
from DataObjectField DOF
left join DataObjectFieldDetail DOFD_Datatype on
	DOF.DataObjectField_ID = DOFD_Datatype.DataObjectField_ID and
	DOFD_Datatype.DataObjectFieldDetailType_CDE = 'Datatype_TYP'
left join DataObject DO on
	DOF.DataObject_ID = DO.DataObject_ID
left join DataElement DE on
	DO.DataElement_ID = DE.DataElement_ID
where DE.Config_ID is not null and DOFD_Datatype.DataObjectFieldDetailType_VAL in ('DATETIME')
)x
where DataObjectFieldDetail_ID = x.fielddetail_id and DataObjectField_ID = x.fieldId

/* Convert to VARCHAR */
UPDATE DataObjectFieldDetail
set DataObjectFieldDetailType_VAL = 'VARCHAR'
from (
select DOFD_Datatype.DataObjectField_ID as 'fieldId', DOFD_Datatype.DataObjectFieldDetail_ID as 'fielddetail_id', DOFD_Datatype.DataObjectFieldDetailType_VAL as 'val'
from DataObjectField DOF
left join DataObjectFieldDetail DOFD_Datatype on
	DOF.DataObjectField_ID = DOFD_Datatype.DataObjectField_ID and
	DOFD_Datatype.DataObjectFieldDetailType_CDE = 'Datatype_TYP'
left join DataObject DO on
	DOF.DataObject_ID = DO.DataObject_ID
left join DataElement DE on
	DO.DataElement_ID = DE.DataElement_ID
where DE.Config_ID is not null and DOFD_Datatype.DataObjectFieldDetailType_VAL in ('CHAR', 'STRING', 'VARCHAR')
)x
where DataObjectFieldDetail_ID = x.fielddetail_id and DataObjectField_ID = x.fieldId