@ECHO off

REM  Author               : Oliver P. Oyston (Luminary Solutions)
REM 
REM  Batch Script         : Create the Default Named Ingres ASP.NET Database
REM
REM  Script Name          : create_aspnetdb.bat
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
REM  1.0      01/10/2008  opo     Original Version

CLS

ECHO ***************************************************
ECHO Starting Batch File For Creating aspnetdb Database
ECHO ***************************************************
ECHO.

REM Create DB
createdb aspnetdb
ECHO done
ECHO.


REM - add tables
ECHO Creating Tables...
sql aspnetdb < IngresAspNetProviders_tables.sql
ECHO done
ECHO.


ECHO ***************************************************
ECHO                Script Completed
ECHO ***************************************************
ECHO.

PAUSE

@ECHO on
