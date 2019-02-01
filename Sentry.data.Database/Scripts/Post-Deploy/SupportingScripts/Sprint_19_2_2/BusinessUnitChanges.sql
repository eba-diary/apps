--This script required a pre-deploy script. Update some TagGroup names and insert Business Units in the BusinessUnit table.

--This is post deploy because we needed to wait for the new BusinessUnit table to be created.

UPDATE TagGroup SET Name = 'Attributes' WHERE Name = 'Attribute Group';
UPDATE TagGroup SET Name = 'Measures' WHERE Name = 'Measure Group';
UPDATE TagGroup SET Name = 'Business Unit' WHERE Name = 'Business Group';

INSERT INTO BusinessUnit VALUES ('Direct Writer', 'DW', 1);
INSERT INTO BusinessUnit VALUES ('National Accounts', 'NA', 2);
INSERT INTO BusinessUnit VALUES ('Regional', 'RG', 3);
INSERT INTO BusinessUnit VALUES ('Transportation', 'TR', 4);
INSERT INTO BusinessUnit VALUES ('Hortica', 'HRT', 5);
INSERT INTO BusinessUnit VALUES ('Life and Health', 'LH', 6);