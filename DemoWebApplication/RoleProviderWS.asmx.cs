#region Code Header

/*  
 * Author               : Oliver P. Oyston (Luminary Solutions)
 * 
 * File Name            : RoleProviderWS.asmx.cs
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
 * 1.0      16/11/2008  opo     Original Version
*/

#endregion

namespace DemoWebApplication
{
    #region NameSpaces Used

    using System.Web.Security;
    using System.Web.Services;

    #endregion

    #region Role Provider Web Service

    /// <summary>
    /// Web Service for the Ingres ASP.NET Role Provider. Please see the Ingres ASP.NET Role 
    /// Provider for documentation of the methods.
    /// </summary>
    [WebService(Namespace = "http://tempuri.org/")]
    [WebServiceBinding(ConformsTo = WsiProfiles.BasicProfile1_1)]
    [System.ComponentModel.ToolboxItem(false)]
    public class RoleProviderWS : System.Web.Services.WebService
    {
        /// <summary>
        /// This is just a wrapper around the corresponding provider method. Please see the compiled
        /// help manual or the thoroughly documented ASP.NET Role and Membership Provider source code
        /// for more information.
        /// </summary>
        [WebMethod]
        public void AddUsersToRoles(string[] usernames, string[] roleNames)
        {
            Roles.AddUsersToRoles(usernames, roleNames);
        }

        /// <summary>
        /// This is just a wrapper around the corresponding provider method. Please see the compiled
        /// help manual or the thoroughly documented ASP.NET Role and Membership Provider source code
        /// for more information.
        /// </summary>
        [WebMethod]
        public void CreateRole(string roleName)
        {
            Roles.CreateRole(roleName);
        }

        /// <summary>
        /// This is just a wrapper around the corresponding provider method. Please see the compiled
        /// help manual or the thoroughly documented ASP.NET Role and Membership Provider source code
        /// for more information.
        /// </summary>
        [WebMethod]
        public bool DeleteRole(string roleName, bool throwOnPopulatedRole)
        {
            return Roles.DeleteRole(roleName, throwOnPopulatedRole);
        }

        /// <summary>
        /// This is just a wrapper around the corresponding provider method. Please see the compiled
        /// help manual or the thoroughly documented ASP.NET Role and Membership Provider source code
        /// for more information.
        /// </summary>
        [WebMethod]
        public string[] FindUsersInRole(string roleName, string usernameToMatch)
        {
            return Roles.FindUsersInRole(roleName, usernameToMatch);
        }

        /// <summary>
        /// This is just a wrapper around the corresponding provider method. Please see the compiled
        /// help manual or the thoroughly documented ASP.NET Role and Membership Provider source code
        /// for more information.
        /// </summary>
        [WebMethod]
        public string[] GetAllRoles()
        {
            return Roles.GetAllRoles();
        }

        /// <summary>
        /// This is just a wrapper around the corresponding provider method. Please see the compiled
        /// help manual or the thoroughly documented ASP.NET Role and Membership Provider source code
        /// for more information.
        /// </summary>
        [WebMethod]
        public string[] GetRolesForUser(string username)
        {
            return Roles.GetRolesForUser(username);
        }

        /// <summary>
        /// This is just a wrapper around the corresponding provider method. Please see the compiled
        /// help manual or the thoroughly documented ASP.NET Role and Membership Provider source code
        /// for more information.
        /// </summary>
        [WebMethod]
        public string[] GetUsersInRole(string roleName)
        {
            return Roles.GetUsersInRole(roleName);
        }

        /// <summary>
        /// This is just a wrapper around the corresponding provider method. Please see the compiled
        /// help manual or the thoroughly documented ASP.NET Role and Membership Provider source code
        /// for more information.
        /// </summary>
        [WebMethod]
        public bool IsUserInRole(string username, string roleName)
        {
            return Roles.IsUserInRole(username, roleName);
        }

        /// <summary>
        /// This is just a wrapper around the corresponding provider method. Please see the compiled
        /// help manual or the thoroughly documented ASP.NET Role and Membership Provider source code
        /// for more information.
        /// </summary>
        [WebMethod]
        public void RemoveUsersFromRoles(string[] usernames, string[] roleNames)
        {
            Roles.RemoveUsersFromRoles(usernames, roleNames);
        }

        /// <summary>
        /// This is just a wrapper around the corresponding provider method. Please see the compiled
        /// help manual or the thoroughly documented ASP.NET Role and Membership Provider source code
        /// for more information.
        /// </summary>
        [WebMethod]
        public bool RoleExists(string roleName)
        {
            return Roles.RoleExists(roleName);
        }
    }

    #endregion
}
