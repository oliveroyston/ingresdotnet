REM  Author               : Oliver P. Oyston (Luminary Solutions)
REM 
REM  Batch Script         : Sets up the Demonstration Web Application for the Ingres ASP.NET Role
REM                         and Membership Providers
REM
REM  Script Name          : SetupDemoWebApplication.bat
REM 
REM  Copyright            : (c) Oliver P. Oyston 2008
REM  
REM  Licence              : GNU Lesser General Public Licence
REM  
REM  This program is free software: you can redistribute it and/or modify it under the terms of the 
REM  GNU Lesser General Public License as published by the Free Software Foundation, either version 3
REM  of the License, or (at your option) any later version.
REM  
REM  This program is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; 
REM  without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See 
REM  the GNU Lesser General Public License for more details.
REM  
REM  You should have received a copy of the GNU Lesser General Public License along with this 
REM  program. If not, see <http://www.gnu.org/licenses/>.
REM   
REM  Version History
REM  
REM  Version  Date        Who     Description
REM  -------  ----------  ---     --------------
REM  1.0      10/11/2008  opo     Original Version

CLS

@ECHO off

ECHO ************************************************************
ECHO Starting Batch File For Creating DemoWebApplication Database            
ECHO ************************************************************
ECHO.

REM Destroy the database if it exists...
destroydb demowebapplication

REM Create the database...
createdb demowebapplication

REM Add the aspnet tables...
sql demowebapplication < IngresAspNetProviders_tables.sql

REM Add the test data...
sql demowebapplication < DemoWebApplication.sql

ECHO.
ECHO.
ECHO Done.
ECHO.
ECHO.

pause

@ECHO on