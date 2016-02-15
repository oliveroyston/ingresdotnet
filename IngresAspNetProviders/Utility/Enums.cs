#region Code Header

/*  
 * Author               : Oliver P. Oyston (Luminary Solutions)
 * 
 * File Name            : Enums.cs
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

namespace Ingres.Web.Security.Utility
{
    #region Enums

    /// <summary>
    /// An enumeration of the existing Ingres ASP.NET Providers
    /// </summary>
    internal enum IngresAspNetProvider
    {
        /// <summary>
        /// The Ingres ASP.NET Role Provider.
        /// </summary>
        Role,

        /// <summary>
        /// The Ingres ASP.NET Membership Provider.
        /// </summary>
        Membership,
    }

    /// <summary>
    /// An enumeration of the overridden role provider methods.
    /// </summary>
    internal enum RoleProviderMethod
    {
        /// <summary>
        /// Represents the AddUsersToRoles method.
        /// </summary>
        AddUsersToRoles,
        
        /// <summary>
        /// Represents the CreateRole method.
        /// </summary>
        CreateRole,
        
        /// <summary>
        /// Represents the DeleteRole method.
        /// </summary>
        DeleteRole,
        
        /// <summary>
        /// Represents the FindUsersInRole method.
        /// </summary>
        FindUsersInRole,
        
        /// <summary>
        /// Represents the GetAllRoles method.
        /// </summary>
        GetAllRoles,
        
        /// <summary>
        /// Represents the GetRolesForUser method.
        /// </summary>
        GetRolesForUser,
        
        /// <summary>
        /// Represents the GetUsersInRole method.
        /// </summary>
        GetUsersInRole,
        
        /// <summary>
        /// Represents the IsUserInRole method.
        /// </summary>
        IsUserInRole,
        
        /// <summary>
        /// Represents the RemoveUsersFromRoles method.
        /// </summary>
        RemoveUsersFromRoles,
        
        /// <summary>
        /// Represents the RoleExists method.
        /// </summary>
        RoleExists,
    }

    /// <summary>
    /// An enumeration of the overridden membership provider methods.
    /// </summary>
    internal enum MembershipProviderMethod
    {
        /// <summary>
        /// Represents the ChangePassword method.
        /// </summary>
        ChangePassword,
        
        /// <summary>
        /// Represents the ChangePasswordQuestionAndAnswer method.
        /// </summary>
        ChangePasswordQuestionAndAnswer,
        
        /// <summary>
        /// Represents the CreateUser method.
        /// </summary>
        CreateUser,
        
        /// <summary>
        /// Represents the DeleteUser method.
        /// </summary>
        DeleteUser,
        
        /// <summary>
        /// Represents the FindUsersByEmail method.
        /// </summary>
        FindUsersByEmail,
        
        /// <summary>
        /// Represents the FindUsersByName method.
        /// </summary>
        FindUsersByName,
        
        /// <summary>
        /// Represents the GetAllUsers method.
        /// </summary>
        GetAllUsers,
        
        /// <summary>
        /// Represents the GetNumberOfUsersOnline method.
        /// </summary>
        GetNumberOfUsersOnline,
        
        /// <summary>
        /// Represents the GetPassword method.
        /// </summary>
        GetPassword,
        
        /// <summary>
        /// Represents the GetUser method.
        /// </summary>
        GetUser,
        
        /// <summary>
        /// Represents the GetUserByObject method.
        /// </summary>
        GetUserByObject,
        
        /// <summary>
        /// Represents the GetUserNameByEmail method.
        /// </summary>
        GetUserNameByEmail,
        
        /// <summary>
        /// Represents the ResetPassword method.
        /// </summary>
        ResetPassword,
        
        /// <summary>
        /// Represents the UnlockUser method.
        /// </summary>
        UnlockUser,
        
        /// <summary>
        /// Represents the UpdateUser method.
        /// </summary>
        UpdateUser,
        
        /// <summary>
        /// Represents the ValidateUser method.
        /// </summary>
        ValidateUser,
    }

    /// <summary>
    /// An enumeration of the different reasons we might want to update the failure
    /// count.
    /// </summary>
    internal enum FailureReason
    {
        /// <summary>
        /// A wrong password answer has been supplied.
        /// </summary>
        PasswordAnswer,
        
        /// <summary>
        /// A wrong password has been supplied.
        /// </summary>
        Password,
    }
    #endregion
}
