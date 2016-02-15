/*------------------------------------------------------------------------------------------------*/
/*                                                                                                */
/* SQL Script      : Ingres ASP.NET Roles and Membership CSV Import Script (DEMO)                 */
/*                                                                                                */
/* Script Name     : Import.sql                                                                   */
/*                                                                                                */
/* System Name     : Ingres ASP.NET Providers                                                     */
/*                                                                                                */
/* Sub-System Name : SQL Scripts                                                                  */
/*                                                                                                */
/* Author          : Oliver P. Oyston    (Luminary Solutions)                                     */
/*                                                                                                */
/* Description     : Imports Role and Membership data from CSV files to Ingres.                   */
/*                                                                                                */
/* Licence         : GNU Lesser General Public Licence                                            */
/*                                                                                                */
/* This program is free software: you can redistribute it and/or modify it under the terms of the */
/* GNU Lesser General Public License as published by the Free Software Foundation, either version */
/* 3 of the License, or (at your option) any later version.                                       */
/*                                                                                                */
/* This program is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY;      */
/* without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See  */
/* the GNU Lesser General Public License for more details.                                        */
/*                                                                                                */
/* You should have received a copy of the GNU Lesser General Public License along with this       */
/* program. If not, see <http://www.gnu.org/licenses/>.                                           */
/*                                                                                                */
/* Version History                                                                                */
/*                                                                                                */
/* Version Date     Who   Description                                                             */
/* ------- -------- ----- --------------------------------------------                            */
/* 1.0     10/12/08 OPO   Original version.                                                       */
/*                                                                                                */
/*------------------------------------------------------------------------------------------------*/

COPY TABLE aspnet_Applications
   (ApplicationName        = CHAR(0) '|',
    LoweredApplicationName = CHAR(0) '|',
    ApplicationId          = CHAR(0) '|',
    Description            = CHAR(0))
FROM 'c:\temp\dbo.aspnet_Applications.csv'
WITH row_estimate = 1;
\p\g

COPY TABLE aspnet_Roles
   (ApplicationId   = CHAR(0) '|',
    RoleId          = CHAR(0) '|',
    RoleName        = CHAR(0) '|',
    LoweredRoleName = CHAR(0) '|',
    Description     = CHAR(0))
FROM 'c:\temp\dbo.aspnet_Roles.csv'
WITH row_estimate = 10;
\p\g

COPY TABLE aspnet_Users
   (ApplicationId    = CHAR(0) '|',
    UserId           = CHAR(0) '|',
    UserName         = CHAR(0) '|',
    LoweredUserName  = CHAR(0) '|',
    MobileAlias      = CHAR(0) '|',
    IsAnonymous      = CHAR(0) '|',
    LastActivityDate = CHAR(0))
FROM 'c:\temp\dbo.aspnet_Users.csv'
WITH row_estimate = 50;
\p\g

COPY TABLE aspnet_UsersInRoles
   (UserId = CHAR(0) '|',
    RoleId = CHAR(0))
FROM 'c:\temp\dbo.aspnet_UsersInRoles.csv'
WITH row_estimate = 100;
\p\g

COPY TABLE aspnet_Membership
   (ApplicationId                          = CHAR(0) '|',
    UserId                                 = CHAR(0) '|',
    Password                               = CHAR(0) '|',
    PasswordFormat                         = CHAR(0) '|',
    PasswordSalt                           = CHAR(0) '|',
    MobilePIN                              = CHAR(0) '|',
    Email                                  = CHAR(0) '|',
    LoweredEmail                           = CHAR(0) '|',
    PasswordQuestion                       = CHAR(0) '|',
    PasswordAnswer                         = CHAR(0) '|',
    IsApproved                             = CHAR(0) '|',
    IsLockedOut                            = CHAR(0) '|',
    CreateDate                             = CHAR(0) '|',
    LastLoginDate                          = CHAR(0) '|',
    LastPasswordChangedDate                = CHAR(0) '|',
    LastLockoutDate                        = CHAR(0) '|',
    FailPwdAttemptCount                    = CHAR(0) '|',
    FailPwdAttemptWindowStart              = CHAR(0) '|',
    FailPwdAnswerAttemptCount              = CHAR(0) '|',
    FailPwdAnswerAttemptWindowStart        = CHAR(0) '|',
    Comment                                = CHAR(0))
FROM 'c:\temp\dbo.aspnet_Membership.csv'
WITH row_estimate = 100;
\p\g

COMMIT;

\p\gq

/*------------------------------------------------------------------------------------------------*/
/*----------------------------------------- End of SQL Script ------------------------------------*/
/*------------------------------------------------------------------------------------------------*/