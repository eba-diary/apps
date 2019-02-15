--This script required a pre-deploy script. Update some TagGroup names and insert Business Units in the BusinessUnit table.

UPDATE TagGroup SET Name = 'Attributes' WHERE Name = 'Attribute Group';
UPDATE TagGroup SET Name = 'Measures' WHERE Name = 'Measure Group';
UPDATE TagGroup SET Name = 'Business Unit' WHERE Name = 'Business Group';
