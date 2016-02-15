(c) Oliver P. Oyston 2008

This is the NUnit test suite for the Ingres ASP.NET Role and Membership providers.

The tests utilise DbUnit so that the database can be put into a known condition before the start
of every test fixture. The data that is placed into the database is specified in the corresponding
XML Data Set files (which are embedded resources and hence not visible in the project output). 

Configuration of the Ingres ASP.NET Role and Membership Providers is done in the App.config file.

For more information about NUnit please see:  http://www.nunit.org/index.php
For more information about DbUnit please see: http://www.dbunit.org/