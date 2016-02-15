/*------------------------------------------------------------------------------------------------*/
/*                                                                                                */
/* SQL Script      : Ingres ASP.NET Role and Membership DemoWebApplication Setup Script           */
/*                                                                                                */
/* Script Name     : DemoWebApplication.sql                                                       */
/*                                                                                                */
/* System Name     : Ingres ASP.NET Providers                                                     */
/*                                                                                                */
/* Sub-System Name : SQL Scripts                                                                  */
/*                                                                                                */
/* Author          : Oliver P. Oyston    (Luminary Solutions)                                     */
/*                                                                                                */
/* Description     : Creates the test data required for the DemoWebApplication website.           */
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
/* 1.0     01/12/08 OPO   Original version.                                                       */
/*                                                                                                */
/*------------------------------------------------------------------------------------------------*/

\nocontinue

                                                  /*----------------------------------------------*/
                                                  /* Create the Demo Web Application.             */
                                                  /*----------------------------------------------*/
INSERT INTO aspnet_Applications
VALUES (
    'DemoWebApplication',
    'demowebapplication',
    '7235f9d1-9b01-4230-b989-b815936a603b',
    'A demo web application for the Ingres ASP.NET Role and Membership providers'
);

\p\g

                                                  /*----------------------------------------------*/
                                                  /* Create the Admin user and membership entry   */
                                                  /* for the Demo Web Application.                */
                                                  /*----------------------------------------------*/
                                                  
INSERT INTO aspnet_Users(
    ApplicationId,
    UserId,
    Username,
    LoweredUserName,
    LastActivityDate
)
VALUES(
    '7235f9d1-9b01-4230-b989-b815936a603b',
    '252c759e-9fde-4e1e-82b8-8db223177042',
    'Admin',
    'admin',
    '2008-11-26 16:08:52.083'
);

\p\g

INSERT INTO aspnet_Membership(
    ApplicationId,
    UserId,
    Password,
    PasswordFormat,
    PasswordSalt,
    MobilePIN,
    Email,
    LoweredEmail,
    PasswordQuestion,
    PasswordAnswer,
    IsApproved,
    IsLockedOut,
    CreateDate,
    LastLoginDate,
    LastPasswordChangedDate,
    LastLockoutDate,
    FailPwdAttemptCount,
    FailPwdAttemptWindowStart,
    FailPwdAnswerAttemptCount,
    FailPwdAnswerAttemptWindowStart,
    Comment
)
VALUES(
    '7235f9d1-9b01-4230-b989-b815936a603b', 
    '252c759e-9fde-4e1e-82b8-8db223177042',
    'Passw0rd!',
    0,
    '1234',
    'PIN',
    'admin@demowebapplication.com',
    'admin@demowebapplication.com',
    'What is 1 + 1?',
    '2',
    1,
    0,
    '2008-11-26 16:08:52.083',
    '2008-11-26 16:08:52.083',
    '2008-11-26 16:08:52.083',
    '2008-11-26 16:08:52.083',
    0,
    '2008-11-26 16:08:52.083',
    0,
    '2008-11-26 16:08:52.083',
    'The Admin User for the DemoWebApplication website for the Ingres ASP.NET Providers'
);

\p\g

                                                  /*----------------------------------------------*/
                                                  /* Create the standard user and membership      */
                                                  /* entry for the Demo Web Application.          */
                                                  /*----------------------------------------------*/

INSERT INTO aspnet_Users(
    ApplicationId,
    UserId,
    Username,
    LoweredUserName,
    LastActivityDate
)
VALUES(
    '7235f9d1-9b01-4230-b989-b815936a603b',
    '69ba6e33-22d8-4597-a284-d99e0e4c2441',
    'Oliver',
    'oliver',
    '2008-11-26 16:08:52.083'
);

\p\g

INSERT INTO aspnet_Membership(
    ApplicationId,
    UserId,
    Password,
    PasswordFormat,
    PasswordSalt,
    MobilePIN,
    Email,
    LoweredEmail,
    PasswordQuestion,
    PasswordAnswer,
    IsApproved,
    IsLockedOut,
    CreateDate,
    LastLoginDate,
    LastPasswordChangedDate,
    LastLockoutDate,
    FailPwdAttemptCount,
    FailPwdAttemptWindowStart,
    FailPwdAnswerAttemptCount,
    FailPwdAnswerAttemptWindowStart,
    Comment
)
VALUES(
    '7235f9d1-9b01-4230-b989-b815936a603b',
    '69ba6e33-22d8-4597-a284-d99e0e4c2441',
    'letmein',
    0,
    '1234',
    'PIN',
    'user@demowebapplication.com',
    'user@demowebapplication.com',
    'What is 2 + 2?',
    '4',
    1,
    0,
    '2008-11-26 16:08:52.083',
    '2008-11-26 16:08:52.083',
    '2008-11-26 16:08:52.083',
    '2008-11-26 16:08:52.083',
    0,
    '2008-11-26 16:08:52.083',
    0,
    '2008-11-26 16:08:52.083',
    'The User for the DemoWebApplication website for the Ingres ASP.NET Providers'
);

\p\g

                                                  /*----------------------------------------------*/
                                                  /* Create the Administrator and User roles.     */
                                                  /*----------------------------------------------*/

INSERT INTO aspnet_Roles(
    ApplicationId,
    RoleId,
    RoleName,
    LoweredRoleName,
    Description
)
VALUES(
    '7235f9d1-9b01-4230-b989-b815936a603b',
    'c152ea0d-60cc-4426-9022-4e91b388a3f3',
    'Administrator',
    'administrator',
    'The admin role for the website'
)

\p\g

INSERT INTO aspnet_Roles(
    ApplicationId,
    RoleId,
    RoleName,
    LoweredRoleName,
    Description
)
VALUES(
    '7235f9d1-9b01-4230-b989-b815936a603b',
    '807581a0-e313-4f2e-9adf-820888d6d412',
    'User',
    'user',
    'The user role for the website'
)

\p\g

                                                  /*----------------------------------------------*/
                                                  /* Put the Admin user into the Administrator    */
                                                  /* role and the User role and the User into the */
                                                  /* User Role.                                   */
                                                  /*----------------------------------------------*/
   
INSERT INTO aspnet_UsersInRoles(
    UserId,
    RoleId
)
VALUES(
    '252c759e-9fde-4e1e-82b8-8db223177042',
    'c152ea0d-60cc-4426-9022-4e91b388a3f3'
)

\p\g

INSERT INTO aspnet_UsersInRoles(
    UserId,
    RoleId
)
VALUES(
    '252c759e-9fde-4e1e-82b8-8db223177042',
    '807581a0-e313-4f2e-9adf-820888d6d412'
)

\p\g

INSERT INTO aspnet_UsersInRoles(
    UserId,
    RoleId
)
VALUES(
    '69ba6e33-22d8-4597-a284-d99e0e4c2441',
    '807581a0-e313-4f2e-9adf-820888d6d412'
)

\p\g

COMMIT;
\p\g\q

/*------------------------------------------------------------------------------------------------*/
/*---------------------------------------- End of SQL Script -------------------------------------*/
/*------------------------------------------------------------------------------------------------*/