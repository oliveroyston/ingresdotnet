@echo off

REM  Author               : Oliver P. Oyston (Luminary Solutions)
REM 
REM  Batch Script         : Export Role and Membership data from a SQL Server Database (DEMO).
REM
REM  Script Name          : SqlServerExport.bat
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

cls

set DBSERVER=%1
set DBNAME=%2

if "%DBSERVER%"== "" goto :usage
if "%DBNAME%"== "" goto :usage

BCP "SELECT ApplicationName, LoweredApplicationName, Lower(ApplicationId), Description FROM %DBNAME%.dbo.aspnet_Applications" queryout c:\Temp\dbo.aspnet_Applications.csv  -c -t"|" -T -S %DBSERVER%
BCP "SELECT Lower(ApplicationId), Lower(UserId), Password, PasswordFormat, PasswordSalt, MobilePIN, Email, LoweredEmail, PasswordQuestion, PasswordAnswer, IsApproved, IsLockedOut, CreateDate, LastLoginDate, LastPasswordChangedDate, LastLockoutDate, FailedPasswordAttemptCount, FailedPasswordAttemptWindowStart, FailedPasswordAnswerAttemptCount, FailedPasswordAnswerAttemptWindowStart, Comment FROM %DBNAME%.dbo.aspnet_Membership" queryout c:\Temp\dbo.aspnet_Membership.csv -c -t"|" -T -S %DBSERVER%
BCP "SELECT Lower(ApplicationId), Lower(RoleId), RoleName, LoweredRoleName, Description FROM %DBNAME%.dbo.aspnet_Roles" queryout c:\Temp\dbo.aspnet_Roles.csv -c -t"|" -T -S %DBSERVER%
BCP "SELECT Lower(ApplicationId), Lower(UserId), UserName, LoweredUserName, MobileAlias, IsAnonymous, LastActivityDate FROM %DBNAME%.dbo.aspnet_Users" queryout c:\Temp\dbo.aspnet_Users.csv -c -t"|" -T -S %DBSERVER%
BCP "SELECT LOWER(UserId), LOWER(RoleId) FROM %DBNAME%.dbo.aspnet_UsersInRoles" queryout c:\Temp\dbo.aspnet_UsersInRoles.csv  -c -t"|" -T -S %DBSERVER%

ECHO.
ECHO Script completed.
ECHO.

goto xit

:usage
ECHO.
ECHO Usage:
ECHO SqlServerExport SERVER DATABASE
ECHO.
ECHO Example:
ECHO SqlServerExport MyServer aspnetdb
ECHO.
ECHO This is a simple demonstration script that may need 
ECHO altering depending upon the environment in which it 
ECHO is executed.
ECHO.

goto xit

:xit
pause

@echo on
