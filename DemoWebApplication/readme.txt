(c) Oliver P. Oyston 2008

This is a simple ASP.NET web application that is designed to show how to use and set up the Ingres 
ASP.NET Role and Membership providers. An example web service implementation is also included.

Before the application is used please use the setup scripts to create and populate the required 
database. Additionally, the connection strings in the web.config will probably need to be altered.

This application may be hosted on IIS or opened in Visual Studio and viewed using Cassini.

NOTE: To use the Ingres ASP.NET assembly in the GAC (rather than a reference to a local assembly) then
you must change the web.config file! Substitute the supplied fully qualified type name instead of the
unqualified type name for both the Role and the Membership provider. See comments in the web.config file
for more information.