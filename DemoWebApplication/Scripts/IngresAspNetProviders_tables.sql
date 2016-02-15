/*------------------------------------------------------------------------------------------------*/
/*                                                                                                */
/* SQL Script      : Ingres ASP.NET Tables Creation Script                                        */
/*                                                                                                */
/* Script Name     : IngresAspNetProviders_tables.sql                                             */
/*                                                                                                */
/* System Name     : Ingres ASP.NET Providers                                                     */
/*                                                                                                */
/* Sub-System Name : SQL Scripts                                                                  */
/*                                                                                                */
/* Author          : Oliver P. Oyston    (Luminary Solutions)                                     */
/*                                                                                                */
/* Description     : Creates all of the tables required to use ASP .Net Provider functionality    */
/*                   with the Ingres RDBMS. The tables are currently used by the following Ingres */
/*                   providers:                                                                   */
/*                                                                                                */
/*                       (o) Membership;                                                          */
/*                       (o) Roles.                                                               */
/*                                                                                                */
/*                   All of the ASP .NET tables are created by this script even though the        */
/*                   provider to use them may not yet be implemented.                             */
/*                                                                                                */
/*                   Notes:                                                                       */
/*                   ------                                                                       */
/*                                                                                                */
/*                   The data type mappings from the SQL Server version of the tables are as      */
/*                   follows:                                                                     */
/*                                                                                                */
/*                       int              -> integer                                              */
/*                       datetime         -> date                                                 */
/*                       uniqueidentifier -> char(36)                                             */
/*                       nvarchar         -> varchar                                              */
/*                       bit              -> char(1)                                              */
/*                       ntext            -> long varchar                                         */
/*                       image            -> long byte                                            */
/*                       char             -> char                                                 */
/*                       decimal          -> decimal                                              */
/*                                                                                                */
/*                   Due to limitations on the column name length in Ingres, four columns in the  */
/*                   aspnet_Membership table have been renamed:                                   */
/*                                                                                                */
/*                   FailedPasswordAttemptCount             -> FailPwdAttemptCount                */
/*                   FailedPasswordAttemptWindowStart       -> FailPwdAttemptWindowStart          */
/*                   FailedPasswordAnswerAttemptCount       -> FailPwdAnswerAttemptCount          */
/*                   FailedPasswordAnswerAttemptWindowStart -> FailPwdAnswerAttemptWindowStart    */
/*                                                                                                */
/*                   Some tables had "DEFAULT (newid())" as the default value. For the Ingres     */
/*                   version no default will be used and instead a new guid passed through when   */
/*                   inserting into the table.                                                    */
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
/* 1.0     01/06/08 OPO   Original version.                                                       */
/* 1.1     10/12/08 OPO   Converted DATE columns to TIMESTAMP and the comment field of the        */
/*                        Membership table was changed from a LONG VARCHAR to a VARCHAR(500) for  */
/*                        easier porting of SQL Server based applications to Ingres.              */
/*                                                                                                */
/*------------------------------------------------------------------------------------------------*/

\sql
SET AUTOCOMMIT ON
\p\g
SET NOJOURNALING
\p\g
\sql
SET SESSION WITH PRIVILEGES=ALL
\p\g

                                                  /*----------------------------------------------*/
                                                  /* 1. Create and comment the Schema Versions    */
                                                  /* table.                                       */
                                                  /*----------------------------------------------*/                                               
\CONTINUE
DROP TABLE aspnet_SchemaVersions
\p\g

\NOCONTINUE                                                  
CREATE TABLE aspnet_SchemaVersions(
    Feature                 VARCHAR(128) NOT NULL NOT DEFAULT,
    CompatibleSchemaVersion VARCHAR(128) NOT NULL NOT DEFAULT,
    IsCurrentVersion        CHAR(1)      NOT NULL,

    PRIMARY KEY(Feature, CompatibleSchemaVersion)
);
\p\g

MODIFY aspnet_SchemaVersions TO BTREE UNIQUE ON 
    Feature,
    CompatibleSchemaVersion;
\p\g

COMMENT ON TABLE aspnet_SchemaVersions IS
    'This table is used by the Ingres ASP.NET Providers.';
\p\g

                                                  /*----------------------------------------------*/
                                                  /* 2. Create and comment the WebEvent Events    */
                                                  /* table.                                       */
                                                  /*----------------------------------------------*/
\CONTINUE
DROP TABLE aspnet_WebEvent_Events;
\p\g

\NOCONTINUE

CREATE TABLE aspnet_WebEvent_Events( 
    EventId                CHAR(32)      NOT NULL NOT DEFAULT PRIMARY KEY,
    EventTimeUtc           TIMESTAMP     NOT NULL NOT DEFAULT,
    EventTime              TIMESTAMP     NOT NULL NOT DEFAULT,
    EventType              VARCHAR(256)  NOT NULL NOT DEFAULT,
    EventSequence          DECIMAL(19,0) NOT NULL NOT DEFAULT,
    EventOccurrence        DECIMAL(19,0) NOT NULL NOT DEFAULT,
    EventCode              INTEGER       NOT NULL NOT DEFAULT,
    EventDetailCode        INTEGER       NOT NULL NOT DEFAULT,
    Message                VARCHAR(1024) NULL     NOT DEFAULT,
    ApplicationPath        VARCHAR(256)  NULL     NOT DEFAULT,
    ApplicationVirtualPath VARCHAR(256)  NULL     NOT DEFAULT,
    MachineName            VARCHAR(256)  NOT NULL NOT DEFAULT,
    RequestUrl             VARCHAR(1024) NULL     NOT DEFAULT,
    ExceptionType          VARCHAR(256)  NULL     NOT DEFAULT,
    Details                LONG VARCHAR  NULL     NOT DEFAULT
);
\p\g

MODIFY aspnet_WebEvent_Events TO BTREE UNIQUE ON EventId;
\p\g

COMMENT ON TABLE aspnet_WebEvent_Events IS
    'This table is used by the Ingres ASP.NET Providers.';
\p\g

                                                  /*----------------------------------------------*/
                                                  /* 3. Create and comment the Applications table.*/
                                                  /*----------------------------------------------*/
\CONTINUE
DROP TABLE aspnet_Applications;
\p\g

\NOCONTINUE

CREATE TABLE aspnet_Applications(
    ApplicationName        VARCHAR(256) UNIQUE NOT NULL NOT DEFAULT,
    LoweredApplicationName VARCHAR(256) UNIQUE NOT NULL NOT DEFAULT,
    ApplicationId          CHAR(36)     UNIQUE NOT NULL NOT DEFAULT,
    Description            VARCHAR(256)                 DEFAULT NULL,

    PRIMARY KEY(ApplicationId, ApplicationName, LoweredApplicationName)
);
\p\g 

MODIFY aspnet_Applications TO BTREE UNIQUE ON 
    ApplicationId,
    ApplicationName,
    LoweredApplicationName;
\p\g

COMMENT ON TABLE aspnet_Applications IS
    'This table is used by the Ingres ASP.NET Providers.';
\p\g

                                                  /*----------------------------------------------*/
                                                  /* 4. Create and comment the Personalization    */
                                                  /* Per User table.                              */
                                                  /*----------------------------------------------*/
\CONTINUE
DROP TABLE aspnet_PersonalizationPerUser;
\p\g

\NOCONTINUE

CREATE TABLE aspnet_PersonalizationPerUser(
    Id              CHAR(36)  NOT NULL NOT DEFAULT PRIMARY KEY,
    PathId          CHAR(36)  NULL     NOT DEFAULT,
    UserId          CHAR(36)  NULL     NOT DEFAULT,
    PageSettings    LONG BYTE NOT NULL NOT DEFAULT,
    LastUpdatedDate TIMESTAMP NOT NULL NOT DEFAULT
);
\p\g

MODIFY aspnet_PersonalizationPerUser TO BTREE UNIQUE ON Id;
\p\g

COMMENT ON TABLE aspnet_PersonalizationPerUser IS
    'This table is used by the Ingres ASP.NET Providers.';
\p\g

                                                  /*----------------------------------------------*/
                                                  /* 5. Create and comment the Membership table.  */
                                                  /*----------------------------------------------*/
\CONTINUE
DROP TABLE aspnet_Membership;
\p\g

\NOCONTINUE

CREATE TABLE aspnet_Membership(
    ApplicationId                   CHAR(36)     NOT NULL NOT DEFAULT,
    UserId                          CHAR(36)     NOT NULL NOT DEFAULT PRIMARY KEY,
    Password                        VARCHAR(128) NOT NULL NOT DEFAULT,
    PasswordFormat                  INTEGER      NOT NULL DEFAULT 0,
    PasswordSalt                    VARCHAR(128) NOT NULL NOT DEFAULT,
    MobilePIN                       VARCHAR(16)  NULL     NOT DEFAULT,
    Email                           VARCHAR(256) NULL     NOT DEFAULT,
    LoweredEmail                    VARCHAR(256) NULL     NOT DEFAULT,
    PasswordQuestion                VARCHAR(256) NULL     NOT DEFAULT,
    PasswordAnswer                  VARCHAR(128) NULL     NOT DEFAULT,
    IsApproved                      CHAR(1)      NOT NULL NOT DEFAULT,
    IsLockedOut                     CHAR(1)      NOT NULL NOT DEFAULT,
    CreateDate                      TIMESTAMP    NOT NULL NOT DEFAULT,
    LastLoginDate                   TIMESTAMP    NOT NULL NOT DEFAULT,
    LastPasswordChangedDate         TIMESTAMP    NOT NULL NOT DEFAULT,
    LastLockoutDate                 TIMESTAMP    NOT NULL NOT DEFAULT,
    FailPwdAttemptCount             INTEGER      NOT NULL NOT DEFAULT,
    FailPwdAttemptWindowStart       TIMESTAMP    NOT NULL NOT DEFAULT,
    FailPwdAnswerAttemptCount       INTEGER      NOT NULL NOT DEFAULT,
    FailPwdAnswerAttemptWindowStart TIMESTAMP    NOT NULL NOT DEFAULT,
    Comment                         VARCHAR(500) NULL         DEFAULT NULL
);
\p\g

MODIFY aspnet_Membership TO BTREE UNIQUE ON UserId;
\p\g

COMMENT ON TABLE aspnet_Membership IS
    'This table is used by the Ingres ASP.NET Providers.';
\p\g

                                                  /*----------------------------------------------*/
                                                  /* 6. Create and comment the Profile table.     */
                                                  /*----------------------------------------------*/
\CONTINUE
DROP TABLE aspnet_Profile;
\p\g

\NOCONTINUE

CREATE TABLE aspnet_Profile(
    UserId               CHAR(36)     NOT NULL NOT DEFAULT PRIMARY KEY,
    PropertyNames        LONG VARCHAR NOT NULL NOT DEFAULT,
    PropertyValuesString LONG VARCHAR NOT NULL NOT DEFAULT,
    PropertyValuesBinary LONG BYTE    NOT NULL NOT DEFAULT,
    LastUpdatedDate      TIMESTAMP    NOT NULL NOT DEFAULT
);
\p\g

MODIFY aspnet_Profile TO BTREE UNIQUE ON UserId;
\p\g

COMMENT ON TABLE aspnet_Profile IS
    'This table is used by the Ingres ASP.NET Providers.';
\p\g

                                                  /*----------------------------------------------*/
                                                  /* 7. Create and comment the Users In Roles     */
                                                  /* table.                                       */
                                                  /*----------------------------------------------*/
\CONTINUE
DROP TABLE aspnet_UsersInRoles;
\p\g

\NOCONTINUE

CREATE TABLE aspnet_UsersInRoles(
    UserId CHAR(36) NOT NULL NOT DEFAULT,
    RoleId CHAR(36) NOT NULL NOT DEFAULT,

    PRIMARY KEY(UserId, RoleId)
);
\p\g

MODIFY aspnet_UsersInRoles TO BTREE UNIQUE ON 
    UserId,
    RoleId;
\p\g

COMMENT ON TABLE aspnet_UsersInRoles IS
    'This table is used by the Ingres ASP.NET Providers.';
\p\g

                                                  /*----------------------------------------------*/
                                                  /* 8. Create and comment the Personalization    */
                                                  /* for All Users table.                         */
                                                  /*----------------------------------------------*/
\CONTINUE
DROP TABLE aspnet_PersonalizationAllUsers;
\p\g

\NOCONTINUE

CREATE TABLE aspnet_PersonalizationAllUsers(
    PathId          CHAR(36)  NOT NULL NOT DEFAULT PRIMARY KEY,
    PageSettings    LONG BYTE NOT NULL NOT DEFAULT,
    LastUpdatedDate TIMESTAMP NOT NULL NOT DEFAULT
);
\p\g

MODIFY aspnet_PersonalizationAllUsers TO BTREE UNIQUE ON PathId;
\p\g

COMMENT ON TABLE aspnet_PersonalizationAllUsers IS
    'This table is used by the Ingres ASP.NET Providers.';
\p\g

                                                  /*----------------------------------------------*/
                                                  /* 9. Create and comment the Users table.       */
                                                  /*----------------------------------------------*/
\CONTINUE
DROP TABLE aspnet_Users;
\p\g

\NOCONTINUE

CREATE TABLE aspnet_Users(
    ApplicationId    CHAR(36)     NOT NULL NOT DEFAULT,
    UserId           CHAR(36)     NOT NULL NOT DEFAULT PRIMARY KEY,
    UserName         VARCHAR(256) NOT NULL NOT DEFAULT,
    LoweredUserName  VARCHAR(256) NOT NULL NOT DEFAULT,
    MobileAlias      VARCHAR(16)  NULL     DEFAULT NULL,
    IsAnonymous      CHAR(1)      NOT NULL DEFAULT 0,
    LastActivityDate TIMESTAMP    NOT NULL NOT DEFAULT
);
\p\g

MODIFY aspnet_Users TO BTREE UNIQUE ON UserId;
\p\g

COMMENT ON TABLE aspnet_Users IS
    'This table is used by the Ingres ASP.NET Providers.';
\p\g

                                                  /*----------------------------------------------*/
                                                  /* 10. Create and comment the Paths table.      */
                                                  /*----------------------------------------------*/
\CONTINUE
DROP TABLE aspnet_Paths;
\p\g

\NOCONTINUE

CREATE TABLE aspnet_Paths(
    ApplicationId CHAR(36)     NOT NULL NOT DEFAULT,
    PathId        CHAR(36)     NOT NULL NOT DEFAULT PRIMARY KEY,
    Path          VARCHAR(256) NOT NULL NOT DEFAULT,
    LoweredPath   VARCHAR(256) NOT NULL NOT DEFAULT
);
\p\g

MODIFY aspnet_Paths TO BTREE UNIQUE ON PathId;
\p\g

COMMENT ON TABLE aspnet_Paths IS
    'This table is used by the Ingres ASP.NET Providers.';
\p\g

                                                  /*----------------------------------------------*/
                                                  /* 11. Create and comment the Roles table.      */
                                                  /*----------------------------------------------*/
\CONTINUE
DROP TABLE aspnet_Roles;
\p\g

\NOCONTINUE

CREATE TABLE aspnet_Roles(
    ApplicationId   CHAR(36)     NOT NULL NOT DEFAULT,
    RoleId          CHAR(36)     NOT NULL NOT DEFAULT PRIMARY KEY,
    RoleName        VARCHAR(256) NOT NULL NOT DEFAULT,
    LoweredRoleName VARCHAR(256) NOT NULL NOT DEFAULT,
    Description     VARCHAR(256) NULL     NOT DEFAULT
);
\p\g

MODIFY aspnet_Roles TO BTREE UNIQUE ON RoleId;
\p\g

COMMENT ON TABLE aspnet_Roles IS
    'This table is used by the Ingres ASP.NET Providers.';
\p\g

                                                  /*----------------------------------------------*/
                                                  /* Indexes.                                     */
                                                  /*----------------------------------------------*/


                                                  /*----------------------------------------------*/
                                                  /* Create foreign key constraints for the       */
                                                  /* purpose of referential integrity.            */
                                                  /*----------------------------------------------*/

\NOCONTINUE

ALTER TABLE aspnet_Users
ADD CONSTRAINT aspnet_Users_c01
FOREIGN KEY(ApplicationId)
REFERENCES aspnet_Applications(ApplicationId);
\p\g

ALTER TABLE aspnet_UsersInRoles
ADD CONSTRAINT aspnet_UsersInRoles_c01
FOREIGN KEY(RoleId)
REFERENCES aspnet_Roles(RoleId);
\p\g

ALTER TABLE aspnet_UsersInRoles
ADD CONSTRAINT aspnet_UsersInRoles_c02
FOREIGN KEY(UserId)
REFERENCES aspnet_Users(UserId);
\p\g

ALTER TABLE aspnet_Roles
ADD CONSTRAINT aspnet_Roles_c01
FOREIGN KEY(ApplicationId)
REFERENCES aspnet_Applications(ApplicationId);
\p\g

ALTER TABLE aspnet_Profile
ADD CONSTRAINT aspnet_Profile_c01
FOREIGN KEY(UserId)
REFERENCES aspnet_Users(UserId);
\p\g

ALTER TABLE aspnet_PersonalizationPerUser
ADD CONSTRAINT aspnet_PersonalPerUser_c01
FOREIGN KEY(PathId)
REFERENCES aspnet_Paths(PathId);
\p\g

ALTER TABLE aspnet_PersonalizationPerUser
ADD CONSTRAINT aspnet_PersonalPerUser_c02
FOREIGN KEY(UserId)
REFERENCES aspnet_Users(UserId);
\p\g

ALTER TABLE aspnet_PersonalizationAllUsers
ADD CONSTRAINT aspnet_PersonalAllUsers_c01
FOREIGN KEY(PathId)
REFERENCES aspnet_Paths(PathId);
\p\g

ALTER TABLE aspnet_Paths
ADD CONSTRAINT aspnet_Paths_c01
FOREIGN KEY(ApplicationId)
REFERENCES aspnet_Applications(ApplicationId);
\p\g

ALTER TABLE aspnet_Membership
ADD CONSTRAINT aspnet_Membership_c01
FOREIGN KEY(ApplicationId)
REFERENCES aspnet_Applications(ApplicationId);
\p\g

ALTER TABLE aspnet_Membership
ADD CONSTRAINT aspnet_Membership_c02
FOREIGN KEY(UserId)
REFERENCES aspnet_Users(UserId);
\p\g
                                                  /*----------------------------------------------*/
                                                  /* Permissions.                                 */
                                                  /*----------------------------------------------*/
\NOCONTINUE

GRANT ALL ON aspnet_SchemaVersions          TO PUBLIC;
GRANT ALL ON aspnet_WebEvent_Events         TO PUBLIC;
GRANT ALL ON aspnet_Applications            TO PUBLIC;
GRANT ALL ON aspnet_PersonalizationPerUser  TO PUBLIC;
GRANT ALL ON aspnet_Membership              TO PUBLIC;
GRANT ALL ON aspnet_Profile                 TO PUBLIC;
GRANT ALL ON aspnet_UsersInRoles            TO PUBLIC;
GRANT ALL ON aspnet_PersonalizationAllUsers TO PUBLIC;
GRANT ALL ON aspnet_Users                   TO PUBLIC;
GRANT ALL ON aspnet_Paths                   TO PUBLIC;
GRANT ALL ON aspnet_Roles                   TO PUBLIC;

\p\g

/*------------------------------------------------------------------------------------------------*/
/*---------------------------------------- End of SQL Script -------------------------------------*/
/*------------------------------------------------------------------------------------------------*/