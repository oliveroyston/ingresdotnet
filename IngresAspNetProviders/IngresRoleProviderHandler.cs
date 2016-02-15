#region Code Header

/*  
 * Author               : Oliver P. Oyston (Luminary Solutions)
 * 
 * File Name            : IngresRoleProviderHandler.cs
 *
 * Copyright            : (c) Oliver P. Oyston 2008
 * 
 * Licence              : GNU Lesser General Public Licence
 * 
 * This program is free software: you can redistribute it and/or modify it under the terms of the 
 * GNU Lesser General Public License as published by the Free Software Foundation, either version 3
 * of the License, or (at your option) any later version.
 *
 * This program is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; 
 * without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See 
 * the GNU Lesser General Public License for more details.
 *
 * You should have received a copy of the GNU Lesser General Public License along with this 
 * program. If not, see <http://www.gnu.org/licenses/>.
 * 
 * Version History
 *
 * Version  Date        Who     Description
 * -------  ----------  ---     --------------
 * 1.0      01/10/2008  opo     Original Version
*/

#endregion

namespace Ingres.Web.Security
{
    #region Namespaces Used

    using System;
    using System.Collections.Generic;
    using System.Configuration.Provider;
    using System.Data;
    using Ingres.Client;
    using Ingres.Web.Security.Utility;

    #endregion

    #region Ingres Role Provider Handler

    /// <summary>
    /// Implementation for the Ingres Membership provider.
    /// </summary>
    internal sealed class IngresRoleProviderHandler
    {
        #region Private Fields

        /// <summary>
        /// The Ingres transaction to use.
        /// </summary>
        private readonly IngresTransaction tran;

        /// <summary>
        /// The Ingres connection to use.
        /// </summary>
        private readonly IngresConnection conn;
        
        /// <summary>
        /// The Ingres Role Provider configuration.
        /// </summary>
        private IngresRoleProviderConfiguration config;

        /// <summary>
        /// The Ingres Role provider facade.
        /// </summary>
        private IngresRoleProvider provider;

        #endregion

        #region Constructors

        /// <summary>
        /// Initializes a new instance of the IngresRoleProviderHandler class.
        /// </summary>
        /// <param name="conn">The Ingres connection to use.</param>
        /// <param name="tran">The Ingres transaction to use.</param>
        /// <param name="config">The configuration settings to use.</param>
        /// <param name="provider">The Ingres Role Provider facade to use.</param>
        internal IngresRoleProviderHandler(IngresConnection conn, IngresTransaction tran, IngresRoleProviderConfiguration config, IngresRoleProvider provider)
        {
            if (conn == null || tran == null)
            {
                throw new Exception();
            }

            this.tran = tran;
            this.conn = conn;

            this.config = config;
            this.provider = provider;
        }

        #endregion

        #region Getters and Setters

        /// <summary>
        /// Gets the application id for the application that we are providing role functionality for.
        /// The application id is a Guid stored as a string and is lower case. We store the value
        /// as a string because Ingres does not natively support Guids but we still want to use 
        /// them for globally unique id's.
        /// </summary>
        private string ApplicationId
        {
            // Only expose the field with a getter. We don't want anybody to set the values of this
            // manually as we need to enforce the id integrity using Guids.
            get
            {
                // cache the id in a private field and only attempt the costly get from the
                // database if our value is out of date
                if (!this.config.IsApplicationIdCurrent)
                {
                    this.config.ApplicationId = this.GetApplicationId(new IngresConnection(this.config.ConnectionString), null);
                }

                return this.config.ApplicationId; 
            }
        }

        #endregion

        #region Implementation For the Role Methods

        #region AddUsersToRoles

        /// <summary>
        /// Takes, as input, a list of user names and a list of role names and adds the specified 
        /// users to the specified roles.
        /// </summary>
        /// <remarks>
        /// This is the main implementation for the <c>AddUsersToRoles</c> method of the provider. Please
        /// see the corresponding method in the Facade, which calls this method, for full documentaion. 
        /// </remarks>
        /// <param name="usernames">A list of user names.</param>
        /// <param name="roleNames">A list of roles.</param>
        internal void AddUsersToRoles(string[] usernames, string[] roleNames)
        {
            // Note: Most of this validation is also done by the .NET framework - however I will
            // explicitly do it here also.
            ValidationUtil.CheckArrayParameterIsOk(ref roleNames, true, true, true, 256, "roleNames");
            ValidationUtil.CheckArrayParameterIsOk(ref usernames, true, true, true, 256, "usernames");

            // Ensure that all of the roleNames are valid.
            foreach (string rolename in roleNames)
            {
                if (!this.RoleExists(rolename))
                {
                    throw new ProviderException(string.Format(Messages.RoleNotFound, rolename));
                }
            }

            // Ensure that all of the usernames are valid.
            foreach (string username in usernames)
            {
                if (!this.UserExists(username))
                {
                    throw new ProviderException(string.Format(Messages.UserWasNotFound, username));
                }
            }

            // Ensure that all of the users actually are in the roles.
            foreach (string username in usernames)
            {
                foreach (string rolename in roleNames)
                {
                    if (this.IsUserInRole(username, rolename))
                    {
                        throw new ProviderException(string.Format(Messages.UserAlreadyInRole, username, rolename));
                    }
                }
            }

            // Instantiate a string to hold the required SQL
            string sql = @"
                            INSERT INTO aspnet_UsersInRoles
                               (UserId,
                                RoleId)
                            VALUES
                               (?, 
                                ?)
                           ";
            
            // Create a new command and enrol in the current transaction
            IngresCommand cmd = new IngresCommand(sql, this.conn);
            cmd.Transaction = this.tran;
            cmd.CommandTimeout = this.config.CommandTimeout;
            
            // Add the required parameters
            IngresParameter userParam = cmd.Parameters.Add("UserId", DbType.String);
            IngresParameter roleParam = cmd.Parameters.Add("RoleId", DbType.String);
                        
            // Note: this if a bit "row at a time" style rather than "chunk at a time"
            // processing - however it makes for easier to read and understand code and
            // allows for a static parameterized SQL string. If performance is poor then
            // this could be changed.

            // for each user
            foreach (string username in usernames)
            {
                // obtain the user id
                string userId = this.GetUserIdByName(username);

                // for each rolename
                foreach (string rolename in roleNames)
                {
                    // obtain role id
                    string roleId = this.GetRoleIdByName(rolename);

                    userParam.Value = userId;
                    roleParam.Value = roleId;

                    // try to add user to role
                    // The number of rows affected
                    int rows = cmd.ExecuteNonQuery();

                    // One row should be affected
                    if (rows != 1)
                    {
                        throw new ProviderException(string.Format(Messages.UnknownError));
                    }
                }
            }            
        }

        #endregion

        #region CreateRole

        /// <summary>
        /// Takes, as input, a role name and creates the specified role. <c>CreateRole</c> throws a 
        /// <c>ProviderException</c> if the role already exists, the role name contains a comma, or the 
        /// role name exceeds the maximum length allowed by the data source.
        /// </summary>
        /// <remarks>
        /// This is the main implementation for the <c>CreateRole</c> method of the provider. Please
        /// see the corresponding method in the Facade, which calls this method, for full documentaion. 
        /// </remarks>
        /// <param name="roleName">The role name to be created.</param>
        internal void CreateRole(string roleName)
        {
            // instantiate the out parameter and pass into main CreateRole.
            // Note: the main CreateRole method will take care of input parameter validation.
            string roleid;

            this.CreateRole(roleName, out roleid, this.conn, this.tran);
        }

        #endregion

        #region DeleteRole

        /// <summary>
        /// Takes, as input, a role name and a Boolean value that indicates whether to throw an 
        /// exception if there are users currently associated with the role, and then deletes the 
        /// specified role.
        /// </summary>
        /// <remarks>
        /// This is the main implementation for the <c>DeleteRole</c> method of the provider. Please
        /// see the corresponding method in the Facade, which calls this method, for full documentaion. 
        /// </remarks>
        /// <param name="roleName">The user name that we wish to delete.</param>
        /// <param name="throwOnPopulatedRole">Whether we should throw an exception if the role
        /// we wish to delete has any users in the role or not.</param>
        /// <returns>Whether the role was successfully deleted.</returns>
        internal bool DeleteRole(string roleName, bool throwOnPopulatedRole)
        {
            // Validate the rolename
            ValidationUtil.CheckParameterIsOK(ref roleName, true, true, true, 256, "roleName");

            // The roleName should exist
            if (!this.RoleExists(roleName))
            {
                throw new ProviderException(string.Format(Messages.RoleNotFound, roleName));
            }

            // If we have to throw errors for a poupulated roll then we must check if there are
            // any users in the role.
            if (throwOnPopulatedRole && (this.GetUsersInRole(roleName).Length > 0))
            {
                throw new ProviderException(string.Format(Messages.RoleNotEmpty));
            }

            string sqlDeleteUsersInRoles = @"
                                              DELETE FROM aspnet_UsersInRoles 
                                              WHERE
                                                  RoleId = ? 
                                            ";

            // Create a new command and enrol in the current transactions
            IngresCommand cmdDeleteUsersInRoles = new IngresCommand(sqlDeleteUsersInRoles, this.conn);

            cmdDeleteUsersInRoles.Transaction = this.tran;
            cmdDeleteUsersInRoles.CommandTimeout = this.config.CommandTimeout;

            // Add the required parameter
            cmdDeleteUsersInRoles.Parameters.Add("RoleId", DbType.String).Value = this.GetRoleIdByName(roleName);

            // Delete the users
            cmdDeleteUsersInRoles.ExecuteNonQuery();

            string sqlDeleteRoles = @"
                                    DELETE FROM aspnet_Roles
                                    WHERE 
                                        LoweredRolename = ? 
                                    AND ApplicationId   = ?
                                   ";

            // Create a new command and enrol in the current transaction
            IngresCommand cmdDeleteRoles = new IngresCommand(sqlDeleteRoles, this.conn);
            cmdDeleteRoles.Transaction = this.tran;
            cmdDeleteRoles.CommandTimeout = this.config.CommandTimeout;

            // Add the required paramaters
            cmdDeleteRoles.Parameters.Add("LoweredRolename", DbType.String).Value = roleName.ToLower();
            cmdDeleteRoles.Parameters.Add("ApplicationId",   DbType.String).Value = this.ApplicationId;

            // Finally delete the role
            int rows = cmdDeleteRoles.ExecuteNonQuery();

            // If more than one row was effected then throw an error
            if (rows != 1)
            {
                throw new ProviderException(string.Format(Messages.UnknownError));
            }

            return true;
        }

        #endregion

        #region GetAllRoles

        /// <summary>
        /// Returns the names of all existing roles. If no roles exist, <c>GetAllRoles</c> returns an 
        /// empty string array (a string array with no elements).
        /// </summary>
        /// <remarks>
        /// This is the main implementation for the <c>GetAllRoles</c> method of the provider. Please
        /// see the corresponding method in the Facade, which calls this method, for full documentaion. 
        /// </remarks>
        /// <returns>A string array with all of the roles for the application.</returns>
        internal string[] GetAllRoles()
        {
            // Ensure that we have an application Id
            if (this.ApplicationId == null)
            {
                throw new Exception(Messages.ApplicationIdNotFound);
            }
            
            // build the SQL command
            string sql = @"
                            SELECT 
                                Rolename 
                            FROM 
                                aspnet_Roles
                            WHERE 
                                ApplicationId = ?
                           ";
            
            // Create a new command and enrol in the current transaction
            IngresCommand cmd = new IngresCommand(sql, this.conn);
            cmd.Transaction = this.tran;
            cmd.CommandTimeout = this.config.CommandTimeout;

            // Add the requires parameteters
            cmd.Parameters.Add("ApplicationId", DbType.String).Value = this.ApplicationId;

            // Instantiate a reader and execute the query
            IngresDataReader reader = cmd.ExecuteReader();

            if (reader != null)
            {
                // Get all roles from the reader
                List<string> roleNamesList = new List<string>();

                while (reader.Read())
                {
                    roleNamesList.Add(DBUtil.ColValAsString(reader, "Rolename"));
                }

                reader.Close();

                // If we have some role names then return them as a string array
                if (roleNamesList.Count > 0)
                {
                    return roleNamesList.ToArray();
                } 
            }

            // Otherwise just return a new empty string array
            return new string[0];
        }
        #endregion

        #region GetRolesForUser

        /// <summary>
        /// Takes, as input, a user name and returns the names of the roles to which the user 
        /// belongs. If the user is not assigned to any roles, <c>GetRolesForUser</c> returns an empty 
        /// string array (a string array with no elements). If the user name does not exist, 
        /// <c>GetRolesForUser</c> throws a <c>ProviderException</c>.
        /// </summary>
        /// <remarks>
        /// This is the main implementation for the <c>GetRolesForUser</c> method of the provider. Please
        /// see the corresponding method in the Facade, which calls this method, for full documentaion. 
        /// </remarks>
        /// <param name="username">The username we want to get the roles for.</param>
        /// <returns>Any array of all of the roles for the given user.</returns>
        internal string[] GetRolesForUser(string username)
        {
            // Validate the username
            ValidationUtil.CheckParameterIsOK(ref username, true, false, true, 256, "username");

            // Validate that the user exists and throw exception if he doesn't
            if (!this.UserExists(username))
            {
                throw new ProviderException(string.Format(Messages.UserNotFound));
            }

            // Build up the required SQL
            string sql = @"
                            SELECT 
	                            aspnet_Roles.RoleName
                            FROM   
	                            aspnet_Roles, 
	                            aspnet_UsersInRoles
                            WHERE  
	                            aspnet_Roles.RoleId = aspnet_UsersInRoles.RoleId 
                            AND aspnet_Roles.ApplicationId = ?
                            AND aspnet_UsersInRoles.UserId = ?
                            ORDER BY 
	                            aspnet_Roles.RoleName
                          ";

            // Create a new command and enrol in the current transaction
            IngresCommand cmd = new IngresCommand(sql, this.conn);
            cmd.Transaction = this.tran;
            cmd.CommandTimeout = this.config.CommandTimeout;

            // Add required parameters
            cmd.Parameters.Add("ApplicationId", DbType.String).Value = this.ApplicationId;
            cmd.Parameters.Add("UserId", DbType.String).Value = this.GetUserIdByName(username);
                        
            // Instantiate a new reader and read in the query
            IngresDataReader reader = cmd.ExecuteReader();

            // Get the roles out of the reader
            List<string> roleNamesList = new List<string>();
            
            while (reader.Read())
            {
                roleNamesList.Add(DBUtil.ColValAsString(reader, "RoleName"));
            }

            reader.Close();

            // Return an appropriate string array
            if (roleNamesList.Count > 0)
            {
                return roleNamesList.ToArray();
            }

            // we had no roles and so just return an empty string array.
            return new string[0];
        }
        #endregion

        #region GetUsersInRole

        /// <summary>
        /// Takes, as input, a role name and returns the names of all users assigned to that role.
        /// </summary>
        /// <remarks>
        /// This is the main implementation for the <c>GetUsersInRole</c> method of the provider. Please
        /// see the corresponding method in the Facade, which calls this method, for full documentaion. 
        /// </remarks>
        /// <param name="roleName">The rolename that we wish to get all the users for.</param>
        /// <returns>An array of all of the users for the given role.</returns>
        internal string[] GetUsersInRole(string roleName)
        {
            // If we don't have an application id then we aren't going to have any roles.
            if (this.ApplicationId == null)
            {
                return new string[0];
            }

            // Validate the roleName
            ValidationUtil.CheckParameterIsOK(ref roleName, true, true, true, 256, "roleName");

            // Check if the role exists and throw an exception if it doesn't
            if (!this.RoleExists(roleName))
            {
                throw new ProviderException(string.Format(Messages.RoleNotFound, roleName));
            }
                       
            string sql = @"
                            SELECT 
                                aspnet_Users.UserName
                            FROM   
                                aspnet_Users,
                                aspnet_UsersInRoles,
                                aspnet_Roles
                            WHERE  
                                aspnet_UsersInRoles.RoleId   = aspnet_Roles.RoleId
                            AND aspnet_UsersInRoles.UserId   = aspnet_Users.UserId
                            AND aspnet_Roles.ApplicationId   = aspnet_Users.ApplicationId
                            AND aspnet_Users.ApplicationId   = ?
                            AND aspnet_Roles.LoweredRoleName = ?
                            ORDER BY 
                                aspnet_Users.UserName
                           ";

            // Create a new command and enrol in the current transaction
            IngresCommand cmd = new IngresCommand(sql, this.conn);
            cmd.Transaction = this.tran;
            cmd.CommandTimeout = this.config.CommandTimeout;
            
            // Add the required parameters
            cmd.Parameters.Add("ApplicationId", DbType.String).Value = this.ApplicationId.ToLower();
            cmd.Parameters.Add("LoweredRoleName", DbType.String).Value = roleName.ToLower();

            // Instantiate a new reader and execute the query
            IngresDataReader reader = cmd.ExecuteReader();

            if (reader != null)
            {
                // Get the users from the reader
                List<string> userList = new List<string>();

                while (reader.Read())
                {
                    userList.Add(DBUtil.ColValAsString(reader, "UserName"));
                }

                // Close the reader
                reader.Close();

                // retun an appropriate string array dependent on whether we managed to return any users or not
                if (userList.Count > 0)
                {
                    return userList.ToArray();
                } 
            }

            // no users so return an empty array
            return new string[0];
        }
        #endregion

        #region IsUserInRole

        /// <summary>
        /// Takes, as input, a user name and a role name and determines whether the specified user 
        /// is associated with the specified role.
        /// </summary>
        /// <remarks>
        /// This is the main implementation for the <c>IsUserInRole</c> method of the provider. Please
        /// see the corresponding method in the Facade, which calls this method, for full documentaion. 
        /// </remarks>
        /// <param name="username">The username that we wish to check.</param>
        /// <param name="roleName">The role which we wish to check.</param>
        /// <returns>Whether the given user is in the given role.</returns>
        internal bool IsUserInRole(string username, string roleName)
        {
            ValidationUtil.CheckParameterIsOK(ref roleName, true, true, true, 256, "roleName");
            ValidationUtil.CheckParameterIsOK(ref username, true, false, true, 256, "username");
            
            if (username.Length < 1)
            {
                return false;
            }

            if (!this.UserExists(username))
            {
                throw new ProviderException(string.Format(Messages.UserNotFound));
            }

            if (!this.RoleExists(roleName))
            {
                throw new ProviderException(string.Format(Messages.RoleNotFound, roleName));
            }

            // ensure that we have an appication id
            if (this.ApplicationId == null)
            {
                throw new ArgumentNullException("ApplicationId");
            }
            
            // Instantiate a bool for whether the user is in the role - assume not.
            bool userIsInRole = false;

            // Build up the SQL string
            string sql = @"
                            SELECT 
                                COUNT(*) 
                            FROM 
                                aspnet_UsersInRoles
                            WHERE 
                                UserId = ? 
                            AND RoleId = ? 
                         ";

            // Create a new command and enrol in the current transaction.
            IngresCommand cmd = new IngresCommand(sql, this.conn);
            cmd.Transaction = this.tran;
            cmd.CommandTimeout = this.config.CommandTimeout;

            // Get the user and role id's for the specified user and role names.
            string userid = this.GetUserIdByName(username);
            string roleid = this.GetRoleIdByName(roleName);

            cmd.Parameters.Add("UserId", DbType.String).Value = userid;
            cmd.Parameters.Add("RoleId", DbType.String).Value = roleid;

            // Execute the query and determine is the user is in the role.
            int rows = Convert.ToInt32(cmd.ExecuteScalar());

            if (rows > 0)
            {
                userIsInRole = true;
            }
  
            return userIsInRole;
        }
        #endregion

        #region RemoveUsersFromRoles

        /// <summary>
        /// Takes, as input, a list of user names and a list of role names and removes the 
        /// specified users from the specified roles.
        /// </summary>
        /// <remarks>
        /// This is the main implementation for the <c>RemoveUsersFromRoles</c> method of the provider. Please
        /// see the corresponding method in the Facade, which calls this method, for full documentaion. 
        /// </remarks>
        /// <param name="usernames">The array of usernames.</param>
        /// <param name="roleNames">The array of roles.</param>
        internal void RemoveUsersFromRoles(string[] usernames, string[] roleNames)
        {
            // Validate the input parameters
            ValidationUtil.CheckArrayParameterIsOk(ref roleNames, true, true, true, 256, "roleNames");
            ValidationUtil.CheckArrayParameterIsOk(ref usernames, true, true, true, 256, "usernames");

            // Instantiate lists to hold the calculated user and role Ids.
            List<string> userIdsList = new List<string>();
            List<string> roleIdsList = new List<string>();

            // Ensure that all of the roleNames are valid
            foreach (string username in usernames)
            {
                string userId = this.GetUserIdByName(username);

                if (userId == null)
                {
                    throw new ProviderException(string.Format(Messages.UserWasNotFound, username));
                }

                userIdsList.Add(userId);
            }

            // Ensure that all of the roleNames are valid
            foreach (string rolename in roleNames)
            {
                string roleId = this.GetRoleIdByName(rolename);

                if (roleId == null)
                {
                    throw new ProviderException(string.Format(Messages.RoleNotFound, rolename));
                }

                roleIdsList.Add(roleId);
            }

            // Ensure that the users are actually the the roles to begin with!
            foreach (string username in usernames)
            {
                foreach (string rolename in roleNames)
                {
                    if (!this.IsUserInRole(username, rolename))
                    {
                        throw new ProviderException(string.Format(Messages.UserAlreadyNotInRole, username, rolename));
                    }
                }
            }

            // Build up the SQL string 
            string sql = @"
                            DELETE FROM aspnet_UsersInRoles 
                            WHERE 
                                UserId = ? 
                            AND RoleId = ? 
                           ";

            // Create a new command and enrol in the current transaction.
            IngresCommand cmd = new IngresCommand(sql, this.conn);
            cmd.Transaction = this.tran;
            cmd.CommandTimeout = this.config.CommandTimeout;

            // Add the required parameters
            IngresParameter userParm = cmd.Parameters.Add("UserId", DbType.String);
            IngresParameter roleParm = cmd.Parameters.Add("RoleId", DbType.String);
            
            // For each user
            foreach (string userId in userIdsList)
            {
                // For each role
                foreach (string roleId in roleIdsList)
                {
                    userParm.Value = userId;
                    roleParm.Value = roleId;

                    // Remove the user from the role
                    cmd.ExecuteNonQuery();
                }
            }
        }
        #endregion

        #region RoleExists

        /// <summary>
        /// Takes, as input, a role name and determines whether the role exists.
        /// </summary>
        /// <remarks>
        /// This is the main implementation for the <c>RoleExists</c> method of the provider. Please
        /// see the corresponding method in the Facade, which calls this method, for full documentaion. 
        /// </remarks>
        /// <param name="roleName">role name to check the existence of</param>
        /// <returns>Whether the given role exists.</returns>
        internal bool RoleExists(string roleName)
        {
            // Validate the Parameter
            ValidationUtil.CheckParameterIsOK(ref roleName, true, true, true, 256, "roleName");

            // Assume that the role does not exist
            bool exists = false;

            string sql = @"
                            SELECT 
                                RoleName 
                            FROM 
                                aspnet_Roles 
                            WHERE 
                                LoweredRoleName = ? 
                            AND ApplicationId   = ?
                          ";

            // Instantiate a new command and enrol in the current transaction
            IngresCommand cmd = new IngresCommand(sql, this.conn);
            cmd.Transaction = this.tran;
            cmd.CommandTimeout = this.config.CommandTimeout;
            
            // Add the required parameters.
            cmd.Parameters.Add("LoweredRoleName", DbType.String).Value = roleName.ToLower();
            cmd.Parameters.Add("ApplicationId",   DbType.String).Value = this.ApplicationId.ToLower();

            // Execute the query.
            IngresDataReader reader = cmd.ExecuteReader();

            // Determine whether the role exists.
            if (reader.HasRows)
            {
                exists = true;
            }

            reader.Close();

            return exists;
        }
        #endregion

        #region FindUsersInRole

        /// <summary>
        /// Takes, as input, a search pattern and a role name and returns a list of users belonging 
        /// to the specified role whose user names match the pattern. Wildcard syntax is 
        /// data-source-dependent and may vary from provider to provider. User names are returned 
        /// in alphabetical order.
        /// </summary>
        /// <remarks>
        /// This is the main implementation for the <c>FindUsersInRole</c> method of the provider. Please
        /// see the corresponding method in the Facade, which calls this method, for full documentaion. 
        /// </remarks>
        /// <param name="roleName">The rolename to find users for.</param>
        /// <param name="usernameToMatch">The username wildcard to match.</param>
        /// <returns>Returns a list of users belonging to the specified role whose user names match the pattern.</returns>
        internal string[] FindUsersInRole(string roleName, string usernameToMatch)
        {
            // Validate the input parameters
            ValidationUtil.CheckParameterIsOK(ref roleName, true, true, true, 256, "roleName");
            ValidationUtil.CheckParameterIsOK(ref usernameToMatch, true, true, false, 256, "usernameToMatch");

            // Check if the role exists and throw an exception if it doesn't
            if (!this.RoleExists(roleName))
            {
                throw new ProviderException(string.Format(Messages.RoleNotFound, roleName));
            }

            // Get the role id
            string roleId = this.GetRoleIdByName(roleName);

            // Adjust the username so that it is in the correct format for an Ingres "LIKE" 
            usernameToMatch = String.Format("%{0}%", usernameToMatch);

            string sql = @"
                            SELECT
                                aspnet_Users.UserName 
                            FROM 
                                aspnet_Users,
                                aspnet_UsersInRoles 
                            WHERE 
                                aspnet_Users.Username LIKE ? 
                            AND aspnet_Users.UserId        = aspnet_UsersInRoles.UserId 
                            AND aspnet_UsersInRoles.RoleId = ?
                            AND aspnet_Users.ApplicationId = ?
                            ORDER BY 
                                aspnet_Users.UserName
                           ";

            // Create a new command and enrol in the current transaction
            IngresCommand cmd = new IngresCommand(sql, this.conn);
            
            cmd.Transaction = this.tran;
            cmd.CommandTimeout = this.config.CommandTimeout;

            // Add the requires parameters
            cmd.Parameters.Add("Username",      DbType.String).Value = usernameToMatch;
            cmd.Parameters.Add("RoleId",        DbType.String).Value = roleId;
            cmd.Parameters.Add("ApplicationId", DbType.String).Value = this.ApplicationId.ToLower();

            // Instantiate a new reader and execute the query
            IngresDataReader reader = cmd.ExecuteReader();

            if (reader != null)
            {
                // Get the results out of the reader
                List<string> userNamesList = new List<string>();

                while (reader.Read())
                {
                    userNamesList.Add(DBUtil.ColValAsString(reader, "UserName"));
                }

                reader.Close();

                // return an appropriate string array
                if (userNamesList.Count > 0)
                {
                    return userNamesList.ToArray();
                } 
            }

            return new string[0];
        }
        #endregion

        #endregion

        #region Helper Methods

        #region GetUserIdByName

        /// <summary>
        /// A helper method to return the User Id for a given username.
        /// </summary>
        /// <param name="username">The username.</param>
        /// <returns>The Id of the username.</returns>
        private string GetUserIdByName(string username)
        {
            string id = null;

            string sql = @"
                          SELECT  
                              UserId 
                          FROM 
                              aspnet_Users 
                          WHERE 
                              LoweredUserName = ?
                          AND ApplicationId   = ?
                         ";

            IngresCommand cmd = new IngresCommand(sql, this.conn);
            cmd.Transaction = this.tran;

            cmd.Parameters.Add("LoweredUserName", DbType.String).Value = username.ToLower();
            cmd.Parameters.Add("ApplicationId",   DbType.String).Value = this.ApplicationId.ToLower();

            IngresDataReader datareader = cmd.ExecuteReader();

            if (datareader != null)
            {
                if (datareader.HasRows)
                {
                    datareader.Read();
                    id = datareader.GetString(0);
                }

                datareader.Close();
            } 

            return id;
        }

        #endregion

        #region CreateUser

        /// <summary>
        /// Helper method to create a user.
        /// </summary>
        /// <param name="username">The username that we wish to create.</param>
        /// <returns>The Id for the newly created user.</returns>
        private string CreateUser(string username)
        {
            // Validate the username
            ValidationUtil.CheckParameterIsOK(ref username, true, true, true, 256, "username");

            string id = Guid.NewGuid().ToString().ToLower();

            string sql = @"
                          INSERT INTO aspnet_Users
                             (ApplicationId,
                              UserId,
                              UserName,
                              LoweredUserName,
                              MobileAlias,
                              IsAnonymous,
                              LastActivityDate)
                          VALUES
                             (?,
                              ?,
                              ?, 
                              ?,
                              NULL,
                              '0',
                              ?)
                         ";

            IngresCommand cmd     = new IngresCommand(sql, this.conn);

            cmd.Parameters.Add("ApplicationId",   DbType.String).Value = this.ApplicationId.ToLower();
            cmd.Parameters.Add("UserId",          DbType.String).Value = id;
            cmd.Parameters.Add("UserName",        DbType.String).Value = username;
            cmd.Parameters.Add("LoweredUserName", DbType.String).Value = username.ToLower();
            cmd.Parameters.Add("LastActivity",    DbType.Date).Value   = DateTime.Now;

            int rows  = cmd.ExecuteNonQuery();

            if (rows != 1)
            {
                throw new ProviderException(string.Format(Messages.UnknownError));
            }

            // the user has been successfully created and so we return the id.
            return id;
        }

        #endregion

        #region GetRoleIdByName

        /// <summary>
        /// Finds the Id for a given rolename.
        /// </summary>
        /// <param name="rolename">The name of the role to find the Id for.</param>
        /// <returns>The Id for the given role.</returns>
        private string GetRoleIdByName(string rolename)
        {
            string id = null;

            string sql = @"
                          SELECT  
                              RoleId 
                          FROM 
                              aspnet_Roles 
                          WHERE 
                              LoweredRoleName = ?
                          AND ApplicationId   = ?
                         ";

            IngresCommand cmd = new IngresCommand(sql, this.conn);
            cmd.Transaction = this.tran;
            cmd.CommandTimeout = this.config.CommandTimeout;

            cmd.Parameters.Add("LoweredRoleName", DbType.String).Value = rolename.ToLower();
            cmd.Parameters.Add("ApplicationId",   DbType.String).Value = this.ApplicationId.ToLower();
            
            IngresDataReader datareader = cmd.ExecuteReader();

            if (datareader != null)
            {
                if (datareader.HasRows)
                {
                    datareader.Read();
                    id = datareader.GetString(0);
                }

                datareader.Close();
            }

            return id;
        }
        #endregion

        #region GetApplicationId

        /// <summary>
        /// Gets the Id for the current application.
        /// </summary>
        /// <param name="conn">The Ingres connection to use.</param>
        /// <param name="tran">The Ingres transaction to use.</param> 
        /// <returns>The Id for the current application.</returns>
        private string GetApplicationId(IngresConnection conn, IngresTransaction tran)
        {
            string id = null;

            string sql = @"
                          SELECT  
                              ApplicationId 
                          FROM 
                              aspnet_Applications 
                          WHERE LoweredApplicationName = ?
                         ";

            // Create the new command and enrol in the current transaction
            IngresCommand cmd = new IngresCommand(sql, this.conn);

            cmd.Transaction = this.tran;

            cmd.Parameters.Add("LoweredApplicationName", DbType.String).Value = this.config.ApplicationName.ToLower();

            conn.Open();

            IngresDataReader reader = cmd.ExecuteReader();

            if (reader != null)
            {
                if (reader.HasRows)
                {
                    reader.Read();

                    // Retrieve the Id
                    id = DBUtil.ColValAsString(reader, "ApplicationId");

                    reader.Close();
                }
                else
                {
                    // Close the reader.
                    reader.Close();

                    // We don't have an application so create one.
                    this.CreateApplication(this.config.ApplicationName, out id);
                }
            }

            // Mark the application Id as current so that we don't have to fetch it from the database
            // again unless it changes.
            this.config.IsApplicationIdCurrent = true;

            // Close the connection
            conn.Close();

            return id;
        }
        #endregion

        #region CreateApplication

        /// <summary>
        /// Creates an application in the database.
        /// </summary>
        /// <param name="name">The name of the application to create.</param>
        /// <param name="id">The Id of the application to create.</param>
        private void CreateApplication(string name, out string id)
        {
            // Ensure that the proposed rolename is of a valid form and does not already exist
            if (name.IndexOf(',') > 0)
            {
                throw new ArgumentException(Messages.ApplicationNamesCannotContainCommas);
            }

            // Build up the command and connection details
            id = null;

            string sql = @"
                          SELECT
                              ApplicationId 
                          FROM
                              aspnet_Applications 
                          WHERE 
                              LoweredApplicationName = ?
                         ";

            IngresCommand cmd = new IngresCommand(sql, this.conn);
            cmd.Transaction = this.tran;
            cmd.CommandTimeout = this.config.CommandTimeout;

            cmd.Parameters.Add("LoweredApplicationName", DbType.String).Value = name.ToLower();

            IngresDataReader datareader = cmd.ExecuteReader();

            if (datareader != null)
            {
                if (datareader.HasRows)
                {
                    datareader.Read();
                    id = datareader.GetString(0);

                    datareader.Close();

                    return;
                }

                id = Guid.NewGuid().ToString();
            }

            if (datareader != null)
            {
                datareader.Close();
            }

            sql = @"
                    INSERT INTO aspnet_Applications 
                       (ApplicationId, 
                        ApplicationName, 
                        LoweredApplicationName)
                    VALUES  
                       (?, 
                        ?, 
                        ?)
                    ";

            cmd  = new IngresCommand(sql, this.conn);
            cmd.Transaction = this.tran;
            cmd.CommandTimeout = this.config.CommandTimeout;

            cmd.Parameters.Add("ApplicationId",          DbType.String).Value = id;
            cmd.Parameters.Add("ApplicationName",        DbType.String).Value = name;
            cmd.Parameters.Add("LoweredApplicationName", DbType.String).Value = name.ToLower();

            cmd.ExecuteNonQuery();

            return;
        }
        #endregion

        #region CreateRole

        /// <summary>
        /// Creates a role in the database.
        /// </summary>
        /// <param name="roleName">The rolename to create.</param>
        /// <param name="roleid">The role id.</param>
        /// <param name="conn">The Ingres connection to use.</param>
        /// <param name="tran">The Ingres transaction to use.</param>
        private void CreateRole(string roleName, out string roleid, IngresConnection conn, IngresTransaction tran)
        {
            // Validate the roleName
            ValidationUtil.CheckParameterIsOK(ref roleName, true, true, true, 256, "roleName");

            // Ensure that the proposed roleName does not already exist
            if (this.RoleExists(roleName))
            {
                throw new ProviderException(string.Format(Messages.RoleAlreadyExists, roleName));
            }

            string sql = @"
                            INSERT INTO aspnet_Roles
                               (ApplicationId,
                                RoleId,
                                RoleName,
                                LoweredRoleName,
                                Description)  
                            VALUES
                               (?,
                                ?,
                                ?,
                                ?,
                                NULL)
                         ";

            // Create the command with the current connection and enrol in the transaction
            IngresCommand cmd = new IngresCommand(sql, this.conn);
            cmd.Transaction = this.tran;
            cmd.CommandTimeout = this.config.CommandTimeout;

            // Generate a new role Id - this will be the sent out 
            roleid = Guid.NewGuid().ToString().ToLower();

            // Add the required parameters
            cmd.Parameters.Add("ApplicationId", DbType.String).Value = this.ApplicationId;
            cmd.Parameters.Add("RoleId",          DbType.String).Value = roleid;
            cmd.Parameters.Add("RoleName",        DbType.String).Value = roleName;
            cmd.Parameters.Add("LoweredRoleName", DbType.String).Value = roleName.ToLower();

            // Execute the query
            int rows = cmd.ExecuteNonQuery();

            // Validate that the query affected the correct numbber of rows.
            if (rows != 1)
            {
                throw new ProviderException(string.Format(Messages.UnknownError));
            }
        }

        /// <summary>
        /// Helper method to determine whether a user exists for a given
        /// </summary>
        /// <param name="username">The username that we want to check the existence of.</param>
        /// <returns>A boolean indicating whether the user exists.</returns>
        private bool UserExists(string username)
        {
            // Validate the input
            ValidationUtil.CheckParameterIsOK(ref username, true, true, true, 256, "username");
           
            // assume that the user doesn't exist
            bool exists = false;

            // build up the parameterised SQL string
            string sql = @"
                            SELECT
                                UserName 
                            FROM 
                                aspnet_Users 
                            WHERE 
                                LoweredUserName = ?
                            AND ApplicationId   = ? 
                           ";

            // Create a new command and enrol in the current transaction
            IngresCommand cmd = new IngresCommand(sql, this.conn);
            cmd.Transaction = this.tran;
            cmd.CommandTimeout = this.config.CommandTimeout;

            // Add the required parameters
            cmd.Parameters.Add("LoweredUserName", DbType.String).Value = username.ToLower();
            cmd.Parameters.Add("ApplicationId", DbType.String).Value = this.ApplicationId;

            // Instantiate a new reader and execute the query
            IngresDataReader datareader = cmd.ExecuteReader();

            // If we have returned anything then the user exists
            if (datareader.HasRows)
            {
                exists = true;
            }

            // close the datareader and return
            datareader.Close();

            return exists;
        }

        #endregion

        #endregion
    }

    #endregion
}