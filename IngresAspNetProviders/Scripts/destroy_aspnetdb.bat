@ECHO off

REM  Author               : Oliver P. Oyston (Luminary Solutions)
REM 
REM  Batch Script         : Destroy the Default Named Ingres ASP.NET Database
REM
REM  Script Name          : destroy_aspnetdb.bat
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


:start

ECHO Do you want to attempt to delete the aspnet database?
ECHO Any data inside will be lost.
ECHO.
ECHO Continue? 
SET /p userinp=[Y/N]:
SET userinp=%userinp:~0,1%

IF "%userinp%"=="Y"   GOTO yes
IF "%userinp%"=="y"   GOTO yes
IF "%userinp%"=="N"   GOTO no
IF "%userinp%"=="n"   GOTO no


ECHO.
ECHO Invalid Choice
ECHO.
PAUSE
CLS
goto :start

:yes

CLS

ECHO ***************************************************
ECHO           Dropping the aspnetdb Database
ECHO ***************************************************
ECHO.
ECHO.
destroydb aspnetdb
ECHO.
ECHO.
ECHO ***************************************************
ECHO                Script Completed
ECHO ***************************************************
ECHO.

PAUSE

@ECHO on

:no

@ECHO on