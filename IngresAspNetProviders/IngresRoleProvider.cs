#region Code Header

/*  
 * Author               : Oliver P. Oyston (Luminary Solutions)
 * 
 * File Name            : IngresRoleProvider.cs
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
    using System.Collections.Specialized;
    using System.Web.Security;
    using Ingres.Client;
    using Ingres.Web.Security.Utility;

    #endregion

    #region Ingres ASP.NET Role Provider [Facade]

    /// <summary>
    /// A fully implemented custom role provider to provide the interface between ASP.NET's role 
    /// management service (the "role manager") and the Ingres RDBMS. This role provider is closely
    /// based on the SQL Server implementation that shipped with .Net 2.0 and, by default, uses the
    /// same table names and provides all the same functionality of its SQL Server counterpart so 
    /// that it can be used as a true "drop-in" alternative.
    /// </summary>
    /// <remarks>
    /// <para>
    /// The fundamental job of the role provider is to interface with an Ingres database containing 
    /// containing role data mapping users to roles, and to provide methods for creating roles, 
    /// deleting roles, adding users to roles, etc. Given a user name, the role manager relies on 
    /// the role provider to determine whether what role or roles the user belongs to. The role 
    /// manager also implements administrative methods such as <c>Roles.CreateRole</c> and 
    /// <c>Roles.AddUserToRole</c> by calling the underlying methods in the provider.
    /// </para>
    /// <para>
    /// This provider makes use of the Ingres .NET data provider and assumes the existence of the
    /// standard "aspnet" tables in the Ingres database specified in the connection string details.
    /// If you have not already created these tables then please run the setup scripts provided 
    /// with the Ingres Role Provider distribution.
    /// </para>
    /// <para>
    /// The Ingres .NET data provider may be obtained as a free download from the 
    /// <see href="http://www.ingres.com/">Ingres website</see>.
    /// </para>
    /// <para>
    /// The documentation in this file was compiled using reference information from MSDN.
    /// </para>
    /// </remarks>
    public class IngresRoleProvider : RoleProvider
    {
        /// <summary>
        /// The Ingres Membership provider configuration settings.
        /// </summary>
        private IngresRoleProviderConfiguration config;

        #region Role Provider Property Overrides

        #region ApplicationName

        /// <summary>
        /// Gets or sets the name of the application using the role provider. <c>ApplicationName</c> is 
        /// used to scope role data. i.e. using an <c>ApplicationName</c> enables us to have many 
        /// distinct applications in a single database.
        /// </summary>
        /// <remarks>
        /// Overrides a property from the <c>System.Web.Security.RoleProvider</c> class to provide an 
        /// Ingres specific implementation.
        /// </remarks>
        public override string ApplicationName
        {
            get
            {
                return this.config.ApplicationName;
            }

            set
            {
                this.config.ApplicationName = value;
            }
        }

        #endregion

        #endregion

        #region System Configuration Initialization

        /// <summary>
        /// Overridden <c>System.Configuration.Provider.ProviderBase.Initialize</c> Method providing
        /// our custom implementation for the Ingres ASP.NET Role Provider.
        /// </summary>
        /// <param name="name">The name of the provider.</param>
        /// <param name="coll">A NameValueCollection of configuration settings.</param>
        public override void Initialize(string name, NameValueCollection coll)
        {
            // Initialise the configuration;
            this.config = new IngresRoleProviderConfiguration(name, coll);

            // Initialize the abstract base class.
            base.Initialize(name, coll);
        }

        #endregion

        #region Role Provider Method Overrides

        #region AddUsersToRoles

        /// <summary>
        /// This method takes an array of user names and an array of role names and adds the 
        /// specified users to the specified roles.
        /// </summary>
        /// <remarks>
        /// <para>
        /// <c>AddUsersToRoles</c> throws a <c>ProviderException</c> if any of the user names or 
        /// role names do not exist. If any user name or role name is null an <c>ArgumentNullException</c> 
        /// is thrown. If any user name or role name is an empty string then an <c>ArgumentException</c>
        /// is thrown.
        /// </para>
        /// <para>
        /// This method overrides a method from the <c>System.Web.Security.RoleProvider</c> class to provide an Ingres
        /// specific implementation.
        /// </para>
        /// </remarks>
        /// <param name="usernames">A list of user names.</param>
        /// <param name="roleNames">A list of roles.</param>
        public override void AddUsersToRoles(string[] usernames, string[] roleNames)
        {
            IngresTransaction tran = null;
            
            try
            {
                using (IngresConnection conn = new IngresConnection(this.config.ConnectionString))
                {
                    // Open the connection and start a new transaction
                    conn.Open();

                    tran = conn.BeginTransaction();

                    // Call the implementation of the method
                    this.GetHandler(conn, tran).AddUsersToRoles(usernames, roleNames);

                    // Commit the transaction
                    tran.Commit();
                }
            }
            catch (Exception ex)
            {
                // Attempt to rollback
                try
                {
                    if (tran != null && tran.Connection != null)
                    {
                        tran.Rollback();
                    }
                }
                catch
                {
                    // Add the rollback error.
                    ExceptionHandler.LogRollbackWarning(RoleProviderMethod.AddUsersToRoles);
                }

                // Handle the exception appropriately
                ExceptionHandler.HandleException(ex, RoleProviderMethod.AddUsersToRoles);
            }
        }

        #endregion

        #region CreateRole

        /// <summary>
        /// This method takes a role name and creates the specified role. <c>CreateRole</c> throws a 
        /// <c>ProviderException</c> if the role already exists, the role name contains a comma, or the 
        /// role name exceeds the maximum length allowed by the data source.
        /// </summary>
        /// <remarks>
        /// This method overrides a method from the <c>System.Web.Security.RoleProvider</c> class to provide an Ingres
        /// specific implementation.
        /// </remarks>
        /// <param name="roleName">The role name to be created.</param>
        public override void CreateRole(string roleName)
        {
            IngresTransaction tran = null;

            try
            {
                using (IngresConnection conn = new IngresConnection(this.config.ConnectionString))
                {
                    // Open the connection and start a new transaction
                    conn.Open();

                    tran = conn.BeginTransaction();

                    // Call the implementation of the method
                    this.GetHandler(conn, tran).CreateRole(roleName);

                    // Commit the transaction
                    tran.Commit();
                }
            }
            catch (Exception ex)
            {
                // Attempt to rollback
                try
                {
                    if (tran != null && tran.Connection != null)
                    {
                        tran.Rollback();
                    }
                }
                catch
                {
                    // Add the rollback error.
                    ExceptionHandler.LogRollbackWarning(RoleProviderMethod.CreateRole);
                }

                // Handle the exception appropriately
                ExceptionHandler.HandleException(ex, RoleProviderMethod.CreateRole);
            }
        }

        #endregion

        #region DeleteRole

        /// <summary>
        /// This method takes a role name and a Boolean value that indicates whether to throw an 
        /// exception if there are users currently associated with the role, and then deletes the 
        /// specified role.
        /// </summary>
        /// <remarks>
        /// <para>
        /// If the <c>throwOnPopulatedRole</c> input parameter is true and the specified role has one or 
        /// more members then a <c>ProviderException</c> is thrown and the role is not deleted. If 
        /// <c>throwOnPopulatedRole</c> is false then the role is deleted whether it is empty or not.
        /// </para>op 
        /// <para>
        /// When <c>DeleteRole</c> deletes a role and there are users assigned to that role, it also 
        /// removes users from the role.
        /// </para>
        /// <para>
        /// This method overrides a method from the <c>System.Web.Security.RoleProvider</c> class to 
        /// provide an Ingres specific implementation.
        /// </para>
        /// </remarks>
        /// <param name="roleName">The user name that we wish to delete.</param>
        /// <param name="throwOnPopulatedRole">Whether we should throw an exception if the role
        /// we wish to delete has any users in the role or not.</param>
        /// <returns>Whether the role was successfully deleted.</returns>
        public override bool DeleteRole(string roleName, bool throwOnPopulatedRole)
        {
            bool result = false;

            IngresTransaction tran = null;

            try
            {
                using (IngresConnection conn = new IngresConnection(this.config.ConnectionString))
                {
                    // Open the connection and start a new transaction
                    conn.Open();

                    tran = conn.BeginTransaction();

                    // Call the implementation of the method
                    result = this.GetHandler(conn, tran).DeleteRole(roleName, throwOnPopulatedRole);

                    // Commit the transaction
                    tran.Commit();
                }
            }
            catch (Exception ex)
            {
                // Attempt to rollback
                try
                {
                    if (tran != null && tran.Connection != null)
                    {
                        tran.Rollback();
                    }
                }
                catch
                {
                    // Add the rollback error.
                    ExceptionHandler.LogRollbackWarning(RoleProviderMethod.DeleteRole);
                }

                // Handle the exception appropriately
                ExceptionHandler.HandleException(ex, RoleProviderMethod.DeleteRole);
            }

            return result;
        }

        #endregion

        #region FindUsersInRole

        /// <summary>
        /// Takes a search pattern and a role name and returns a list of users belonging to the 
        /// specified role whose user names match the pattern. Any search pattern supported by the 
        /// Ingres 'LIKE %usernameToMatch%' SQL clause may be used. User names are returned in 
        /// alphabetical order.
        /// </summary>
        /// <remarks>
        /// <para>
        /// If the search finds no matches an empty string array is returned. If the role does not
        /// exist then a <c>ProviderException</c> is thrown.
        /// </para>
        /// <para>This method overrides a method from the <c>System.Web.Security.RoleProvider</c>
        /// class to provide an Ingres specific implementation.
        /// </para>
        /// </remarks>
        /// <param name="roleName">The rolename to find users for.</param>
        /// <param name="usernameToMatch">The username wildcard to match.</param>
        /// <returns>Returns a list of users belonging to the specified role whose user names match the pattern.</returns>
        public override string[] FindUsersInRole(string roleName, string usernameToMatch)
        {
            string[] result = new string[0];

            IngresTransaction tran = null;

            try
            {
                using (IngresConnection conn = new IngresConnection(this.config.ConnectionString))
                {
                    // Open the connection and start a new transaction
                    conn.Open();

                    tran = conn.BeginTransaction();

                    // Call the implementation of the method
                    result = this.GetHandler(conn, tran).FindUsersInRole(roleName, usernameToMatch);

                    // Commit the transaction
                    tran.Commit();
                }
            }
            catch (Exception ex)
            {
                // Attempt to rollback
                try
                {
                    if (tran != null && tran.Connection != null)
                    {
                        tran.Rollback();
                    }
                }
                catch
                {
                    // Add the rollback error.
                    ExceptionHandler.LogRollbackWarning(RoleProviderMethod.FindUsersInRole);
                }

                // Handle the exception appropriately
                ExceptionHandler.HandleException(ex, RoleProviderMethod.FindUsersInRole);
            }

            return result;
        }

        #endregion

        #region GetAllRoles

        /// <summary>
        /// Returns the names of all existing roles. If no roles exist, <c>GetAllRoles</c> returns an 
        /// empty string array.
        /// </summary>
        /// <remarks>
        /// This method overrides a method from the <c>System.Web.Security.RoleProvider</c> class to
        /// provide an Ingres specific implementation.
        /// </remarks>
        /// <returns>A string array with all of the roles for the application.</returns>
        public override string[] GetAllRoles()
        {
            string[] result = new string[0];

            IngresTransaction tran = null;

            try
            {
                using (IngresConnection conn = new IngresConnection(this.config.ConnectionString))
                {
                    // Open the connection and start a new transaction
                    conn.Open();

                    tran = conn.BeginTransaction();

                    // Call the implementation of the method
                    result = this.GetHandler(conn, tran).GetAllRoles();

                    // Commit the transaction
                    tran.Commit();
                }
            }
            catch (Exception ex)
            {
                // Attempt to rollback
                try
                {
                    if (tran != null && tran.Connection != null)
                    {
                        tran.Rollback();
                    }
                }
                catch
                {
                    // Add the rollback error.
                    ExceptionHandler.LogRollbackWarning(RoleProviderMethod.GetAllRoles);
                }

                // Handle the exception appropriately
                ExceptionHandler.HandleException(ex, RoleProviderMethod.GetAllRoles);
            }

            return result;
        }

        #endregion

        #region GetRolesForUser

        /// <summary>
        /// Takes a user name and returns the names of the roles which the user is in. 
        /// </summary>
        /// <remarks>
        /// <para>
        /// If the user is not assigned to any roles, <c>GetRolesForUser</c> returns an empty 
        /// string array. If the user name does not exist a <c>ProviderException</c> is thrown.
        /// </para>
        /// <para>
        /// This method overrides a method from the <c>System.Web.Security.RoleProvider</c> class
        /// to provide an Ingres specific implementation.
        /// </para>
        /// </remarks>
        /// <param name="username">The username we want to get the roles for.</param>
        /// <returns>An array of all of the roles for the given user.</returns>
        public override string[] GetRolesForUser(string username)
        {
            string[] result = new string[0];

            IngresTransaction tran = null;

            try
            {
                using (IngresConnection conn = new IngresConnection(this.config.ConnectionString))
                {
                    // Open the connection and start a new transaction
                    conn.Open();

                    tran = conn.BeginTransaction();

                    // Call the implementation of the method
                    result = this.GetHandler(conn, tran).GetRolesForUser(username);

                    // Commit the transaction
                    tran.Commit();
                }
            }
            catch (Exception ex)
            {
                // Attempt to rollback
                try
                {
                    if (tran != null && tran.Connection != null)
                    {
                        tran.Rollback();
                    }
                }
                catch
                {
                    // Add the rollback error.
                    ExceptionHandler.LogRollbackWarning(RoleProviderMethod.GetRolesForUser);
                }

                // Handle the exception appropriately
                ExceptionHandler.HandleException(ex, RoleProviderMethod.GetRolesForUser);
            }

            return result;
        }

        #endregion

        #region GetUsersInRole

        /// <summary>
        /// This method takes a role name and returns the names of all users assigned to that role.
        /// </summary>
        /// <remarks>
        /// <para>
        /// If no users are associated with the specified role, <c>GetUserInRole</c> returns an empty 
        /// string array. If the role does not exist a <c>ProviderException</c> is thrown.
        /// </para>
        /// <para>
        /// This method overrides a method from the <c>System.Web.Security.RoleProvider</c> class 
        /// to provide an Ingres specific implementation.
        /// </para>
        /// </remarks>
        /// <param name="roleName">The rolename that we wish to get all the users for.</param>
        /// <returns>An array of all of the users for the given role.</returns>
        public override string[] GetUsersInRole(string roleName)
        {
            string[] result = new string[0];

            IngresTransaction tran = null;

            try
            {
                using (IngresConnection conn = new IngresConnection(this.config.ConnectionString))
                {
                    // Open the connection and start a new transaction
                    conn.Open();

                    tran = conn.BeginTransaction();

                    // Call the implementation of the method
                    result = this.GetHandler(conn, tran).GetUsersInRole(roleName);

                    // Commit the transaction
                    tran.Commit();
                }
            }
            catch (Exception ex)
            {
                // Attempt to rollback
                try
                {
                    if (tran != null && tran.Connection != null)
                    {
                        tran.Rollback();
                    }
                }
                catch
                {
                    // Add the rollback error.
                    ExceptionHandler.LogRollbackWarning(RoleProviderMethod.GetUsersInRole);
                }

                // Handle the exception appropriately
                ExceptionHandler.HandleException(ex, RoleProviderMethod.GetUsersInRole);
            }

            return result;
        }

        #endregion

        #region IsUserInRole

        /// <summary>
        /// Takes a user name and a role name and determines whether the specified user is 
        /// associated with the specified role.
        /// </summary>
        /// <remarks>
        /// <para>
        /// If the user or role does not exist a <c>ProviderException</c> is thrown.
        /// </para>
        /// <para>
        /// This method overrides a method from the <c>System.Web.Security.RoleProvider</c> class
        /// to provide an Ingres specific implementation.
        /// </para>
        /// </remarks>
        /// <param name="username">The username that we wish to check.</param>
        /// <param name="roleName">The role which we wish to check.</param>
        /// <returns>Whether the given user is in the given role.</returns>
        public override bool IsUserInRole(string username, string roleName)
        {
            bool result = false;

            IngresTransaction tran = null;

            try
            {
                using (IngresConnection conn = new IngresConnection(this.config.ConnectionString))
                {
                    // Open the connection and start a new transaction
                    conn.Open();

                    tran = conn.BeginTransaction();

                    // Call the implementation of the method
                    result = this.GetHandler(conn, tran).IsUserInRole(username, roleName);

                    // Commit the transaction
                    tran.Commit();
                }
            }
            catch (Exception ex)
            {
                // Attempt to rollback
                try
                {
                    if (tran != null && tran.Connection != null)
                    {
                        tran.Rollback();
                    }
                }
                catch
                {
                    // Add the rollback error.
                    ExceptionHandler.LogRollbackWarning(RoleProviderMethod.IsUserInRole);
                }

                // Handle the exception appropriately
                ExceptionHandler.HandleException(ex, RoleProviderMethod.IsUserInRole);
            }

            return result;
        }

        #endregion

        #region RemoveUsersFromRoles

        /// <summary>
        /// Takes an array of user names and an array of role names and removes the specified users
        /// from the specified roles.
        /// </summary>
        /// <remarks>
        /// <para>
        /// <c>RemoveUsersFromRoles</c> throws a <c>ProviderException</c> if any of the users or roles do not 
        /// exist, or if any user specified in the call does not belong to the role from which he 
        /// or she is being removed.
        /// </para>
        /// <para>
        /// This method overrides a method from the <c>System.Web.Security.RoleProvider</c> class to provide an Ingres
        /// specific implementation.
        /// </para>
        /// </remarks>
        /// <param name="usernames">The array of usernames.</param>
        /// <param name="roleNames">The array of roles.</param>
        public override void RemoveUsersFromRoles(string[] usernames, string[] roleNames)
        {
            IngresTransaction tran = null;

            try
            {
                using (IngresConnection conn = new IngresConnection(this.config.ConnectionString))
                {
                    // Open the connection and start a new transaction
                    conn.Open();

                    tran = conn.BeginTransaction();

                    // Call the implementation of the method
                    this.GetHandler(conn, tran).RemoveUsersFromRoles(usernames, roleNames);

                    // Commit the transaction
                    tran.Commit();
                }
            }
            catch (Exception ex)
            {
                // Attempt to rollback
                try
                {
                    if (tran != null && tran.Connection != null)
                    {
                        tran.Rollback();
                    }
                }
                catch
                {
                    // Add the rollback error.
                    ExceptionHandler.LogRollbackWarning(RoleProviderMethod.RemoveUsersFromRoles);
                }

                // Handle the exception appropriately
                ExceptionHandler.HandleException(ex, RoleProviderMethod.RemoveUsersFromRoles);
            }
        }

        #endregion

        #region RoleExists

        /// <summary>
        /// This method takes a role name and determines whether the role exists.
        /// </summary>
        /// <remarks>
        /// This method overrides a method from the <c>System.Web.Security.RoleProvider</c> class to provide an Ingres
        /// specific implementation.
        /// </remarks>
        /// <param name="roleName">Role name to check the existence of.</param>
        /// <returns>Whether the given role exists.</returns>
        public override bool RoleExists(string roleName)
        {
            bool result = false;

            IngresTransaction tran = null;

            try
            {
                using (IngresConnection conn = new IngresConnection(this.config.ConnectionString))
                {
                    // Open the connection and start a new transaction
                    conn.Open();

                    tran = conn.BeginTransaction();

                    // Call the implementation of the method
                    result = this.GetHandler(conn, tran).RoleExists(roleName);

                    // Commit the transaction
                    tran.Commit();
                }
            }
            catch (Exception ex)
            {
                // Attempt to rollback
                try
                {
                    if (tran != null && tran.Connection != null)
                    {
                        tran.Rollback();
                    }
                }
                catch
                {
                    // Add the rollback error.
                    ExceptionHandler.LogRollbackWarning(RoleProviderMethod.RoleExists);
                }

                // Handle the exception appropriately
                ExceptionHandler.HandleException(ex, RoleProviderMethod.RoleExists);
            }

            return result;
        }

        #endregion

        #endregion

        #region Private Methods

        /// <summary>
        /// Gets a handler for the Ingres membership provider.
        /// </summary>
        /// <param name="conn">The Ingres connection.</param>
        /// <param name="tran">The Ingres transaction to use.</param>
        /// <returns>The handler</returns>
        private IngresRoleProviderHandler GetHandler(IngresConnection conn, IngresTransaction tran)
        {
            IngresRoleProviderHandler handler = new IngresRoleProviderHandler(conn, tran, this.config, this);
           
            return handler;
        }
        #endregion
    }
    #endregion
}
