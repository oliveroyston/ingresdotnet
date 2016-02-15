(c) Oliver P. Oyston 2008

This application is designed to test a connection to the Ingres database using the Ingres.NET data
provider and help determine the required connection string to use in the Ingres ASP.NET Role and
Membership providers.

The application is essentially a GUI wrapper around an IngresConnectionStringBuilder object.

The connection string defaults to the simplest case - namely the default names aspnet database
running on a local Ingres installation and with the current user having required credentials to
access the database.

The connection string may be changed by changing the properties in the property grid. The generated
connection string is shown in the read-only connection string text box. 

Pressing the "Test Connection String" button causes the application to attempt to connect to an 
Ingres database using the connection string in the "Connection String" text box.

Any messages - such as exception messages and connection status are shown in the "Messages" text box.

At any time the "Copy Connection String To Clipboard" button may be pressed in order to copy the 
connection string in the "Connection String" text box to the clipboard.

If the Ingres service is not started on the local machine then the "Start Ingres Service on local
machine" button may be pressed to attempt to start the service.


The application uses icons from the Silk icon set by Mark James (http://www.famfamfam.com/lab/icons/silk/).