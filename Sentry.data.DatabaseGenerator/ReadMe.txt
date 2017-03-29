Sentry._MyApp_.DatabaseGenerator
-----------------------------------------------------------------------------------------------
This project is a console app for generating a database schema off of your NHibernate mappings.
You'll need to create an empty database first using SSMS.  Then, just run the project, and the
schema in your local database will be generated.  This is especially useful when just starting 
a project, for rapid development of schema.  Once you are ready to move schema onto a database 
server, you'll need to transition to using the Sentry._MyApp_.Database project, which uses SQL 
Server Data Tools.