#region Code Header

/*  
 * Author               : Oliver P. Oyston (Luminary Solutions)
 * 
 * File Name            : MembershipProviderWS.asmx.cs
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

    using System;
    using System.Collections.Generic;
    using System.Web.Security;
    using System.Web.Services;

    #endregion

    #region Membership Provider Web Service

    /// <summary>
    /// Web Service for the Ingres ASP.NET Membership Provider. Please see the Ingres ASP.NET 
    /// Membership Provider for documentation of the methods.
    /// </summary>
    [WebService(Namespace = "http://tempuri.org/")]
    [WebServiceBinding(ConformsTo = WsiProfiles.BasicProfile1_1)]
    [System.ComponentModel.ToolboxItem(false)]
    public class MembershipProviderWS : WebService
    {
        #region Web Service Implementation

        /// <summary>
        /// This is just a wrapper around the corresponding provider method. Please see the compiled
        /// help manual or the thoroughly documented ASP.NET Role and Membership Provider source code
        /// for more information.
        /// </summary>
        [WebMethod]
        public bool ChangePassword(string username, string oldPassword, string newPassword)
        {
            MembershipUser user = Membership.GetUser(username);

            return user.ChangePassword(oldPassword, newPassword);
        }

        /// <summary>
        /// This is just a wrapper around the corresponding provider method. Please see the compiled
        /// help manual or the thoroughly documented ASP.NET Role and Membership Provider source code
        /// for more information.
        /// </summary>
        [WebMethod]
        public bool ChangePasswordQuestionAndAnswer(string username, string password, string newPasswordQuestion, string newPasswordAnswer)
        {
            MembershipUser user = Membership.GetUser(username);

            return user.ChangePasswordQuestionAndAnswer(password, newPasswordQuestion, newPasswordAnswer);
        }

        /// <summary>
        /// This is just a wrapper around the corresponding provider method. Please see the compiled
        /// help manual or the thoroughly documented ASP.NET Role and Membership Provider source code
        /// for more information.
        /// </summary>
        [WebMethod]
        public WSMembershipUser CreateUser(string username, string password, string email, string passwordQuestion, string passwordAnswer, bool isApproved, object providerUserKey, out MembershipCreateStatus status)
        {
            return MapToWSMembershipUser(Membership.CreateUser(username, password, email, passwordQuestion, passwordAnswer, isApproved, providerUserKey, out status));
        }

        /// <summary>
        /// This is just a wrapper around the corresponding provider method. Please see the compiled
        /// help manual or the thoroughly documented ASP.NET Role and Membership Provider source code
        /// for more information.
        /// </summary>
        [WebMethod]
        public bool DeleteUser(string username, bool deleteAllRelatedData)
        {
            return Membership.DeleteUser(username, deleteAllRelatedData);
        }

        /// <summary>
        /// This is just a wrapper around the corresponding provider method. Please see the compiled
        /// help manual or the thoroughly documented ASP.NET Role and Membership Provider source code
        /// for more information.
        /// </summary>
        [WebMethod]
        public List<WSMembershipUser> FindUsersByEmail(string emailToMatch, int pageIndex, int pageSize, out int totalRecords)
        {
            return ConstructList(Membership.FindUsersByEmail(emailToMatch, pageIndex, pageSize, out totalRecords));
        }

        /// <summary>
        /// This is just a wrapper around the corresponding provider method. Please see the compiled
        /// help manual or the thoroughly documented ASP.NET Role and Membership Provider source code
        /// for more information.
        /// </summary>
        [WebMethod]
        public List<WSMembershipUser> FindUsersByName(string usernameToMatch, int pageIndex, int pageSize, out int totalRecords)
        {
            return ConstructList(Membership.FindUsersByName(usernameToMatch, pageIndex, pageSize, out totalRecords));
        }

        /// <summary>
        /// This is just a wrapper around the corresponding provider method. Please see the compiled
        /// help manual or the thoroughly documented ASP.NET Role and Membership Provider source code
        /// for more information.
        /// </summary>
        [WebMethod]
        public List<WSMembershipUser> GetAllUsers(int pageIndex, int pageSize, out int totalRecords)
        {
            return ConstructList(Membership.GetAllUsers(pageIndex, pageSize, out totalRecords));
        }

        /// <summary>
        /// This is just a wrapper around the corresponding provider method. Please see the compiled
        /// help manual or the thoroughly documented ASP.NET Role and Membership Provider source code
        /// for more information.
        /// </summary>
        [WebMethod]
        public int GetNumberOfUsersOnline()
        {
            return Membership.GetNumberOfUsersOnline();
        }

        /// <summary>
        /// This is just a wrapper around the corresponding provider method. Please see the compiled
        /// help manual or the thoroughly documented ASP.NET Role and Membership Provider source code
        /// for more information.
        /// </summary>
        [WebMethod]
        public string GetPassword(string username, string answer)
        {
            MembershipUser user = Membership.GetUser(username);

            return user.GetPassword(answer);
        }

        /// <summary>
        /// This is just a wrapper around the corresponding provider method. Please see the compiled
        /// help manual or the thoroughly documented ASP.NET Role and Membership Provider source code
        /// for more information.
        /// </summary>
        [WebMethod]
        public WSMembershipUser GetUserByUserName(string username, bool userIsOnline)
        {
            return MapToWSMembershipUser(Membership.GetUser(username, userIsOnline));
        }

        /// <summary>
        /// This is just a wrapper around the corresponding provider method. Please see the compiled
        /// help manual or the thoroughly documented ASP.NET Role and Membership Provider source code
        /// for more information.
        /// </summary>
        [WebMethod]
        public WSMembershipUser GetUserByKey(object providerUserKey, bool userIsOnline)
        {
            return MapToWSMembershipUser(Membership.GetUser(providerUserKey, userIsOnline));
        }

        /// <summary>
        /// This is just a wrapper around the corresponding provider method. Please see the compiled
        /// help manual or the thoroughly documented ASP.NET Role and Membership Provider source code
        /// for more information.
        /// </summary>
        [WebMethod]
        public string GetUserNameByEmail(string email)
        {
            return Membership.GetUserNameByEmail(email);
        }

        /// <summary>
        /// This is just a wrapper around the corresponding provider method. Please see the compiled
        /// help manual or the thoroughly documented ASP.NET Role and Membership Provider source code
        /// for more information.
        /// </summary>
        [WebMethod]
        public string ResetPassword(string username, string answer)
        {
            MembershipUser user = Membership.GetUser(username);

            return user.ResetPassword(answer);
        }

        /// <summary>
        /// This is just a wrapper around the corresponding provider method. Please see the compiled
        /// help manual or the thoroughly documented ASP.NET Role and Membership Provider source code
        /// for more information.
        /// </summary>
        [WebMethod]
        public bool UnlockUser(string userName)
        {
            MembershipUser user = Membership.GetUser(userName);

            return user.UnlockUser();
        }

        /// <summary>
        /// This is just a wrapper around the corresponding provider method. Please see the compiled
        /// help manual or the thoroughly documented ASP.NET Role and Membership Provider source code
        /// for more information.
        /// </summary>
        [WebMethod]
        public void UpdateUser(WSMembershipUser user)
        {
            Membership.UpdateUser(MapToMembershipUser(user));
        }

        /// <summary>
        /// This is just a wrapper around the corresponding provider method. Please see the compiled
        /// help manual or the thoroughly documented ASP.NET Role and Membership Provider source code
        /// for more information.
        /// </summary>
        [WebMethod]
        public bool ValidateUser(string username, string password)
        {
            return Membership.ValidateUser(username, password);
        }

        #endregion

        #region Helper Methods

        /// <summary>
        /// Maps a MembershipUser object to a WSMembershipUser.
        /// </summary>
        /// <param name="user">The user to map.</param>
        /// <returns>The mapped user.</returns>
        protected static WSMembershipUser MapToWSMembershipUser(MembershipUser user)
        {
            if (user == null)
            {
                return null;
            }

            WSMembershipUser webServiceUser = new WSMembershipUser();

            webServiceUser.Comment                 = user.Comment;
            webServiceUser.CreationDate            = user.CreationDate;
            webServiceUser.Email                   = user.Email;
            webServiceUser.IsApproved              = user.IsApproved;
            webServiceUser.IsLockedOut             = user.IsLockedOut;
            webServiceUser.IsOnline                = user.IsOnline;
            webServiceUser.LastActivityDate        = user.LastActivityDate;
            webServiceUser.LastLockoutDate         = user.LastLockoutDate;
            webServiceUser.LastLoginDate           = user.LastLoginDate;
            webServiceUser.LastPasswordChangedDate = user.LastPasswordChangedDate;
            webServiceUser.PasswordQuestion        = user.PasswordQuestion;
            webServiceUser.ProviderName            = user.ProviderName;
            webServiceUser.ProviderUserKey         = user.ProviderUserKey;
            webServiceUser.UserName                = user.UserName;

            return webServiceUser;
        }

        /// <summary>
        /// Maps a WSMembershipUser object to a MembershipUser.
        /// </summary>
        /// <param name="user">The user to map.</param>
        /// <returns>The mapped user.</returns>
        protected static MembershipUser MapToMembershipUser(WSMembershipUser user)
        {
            if (user == null)
            {
                return null;
            }

            MembershipUser membershipUser = new MembershipUser(
                                                                Membership.Provider.Name,
                                                                user.UserName,
                                                                user.ProviderUserKey,
                                                                user.Email,
                                                                user.PasswordQuestion,
                                                                user.Comment,
                                                                user.IsApproved,
                                                                user.IsLockedOut,
                                                                user.CreationDate,
                                                                user.LastLoginDate,
                                                                user.LastActivityDate,
                                                                user.LastPasswordChangedDate,
                user.LastLockoutDate);

            return membershipUser;
        }

        /// <summary>
        /// Converts a MembershipUserCollection to a list of WSMembershipUser.
        /// </summary>
        /// <param name="collection">The MembershipUserCollection to convert.</param>
        /// <returns>The collection converted to a list of WSMembershipUser.</returns>
        protected static List<WSMembershipUser> ConstructList(MembershipUserCollection collection)
        {
            if (collection == null)
            {
                return null;
            }

            List<WSMembershipUser> list = new List<WSMembershipUser>();
            
            foreach (MembershipUser user in collection)
            {
                list.Add(MapToWSMembershipUser(user));
            }
            
            return list;
        }

        #endregion

        #region WSMembershipUser Class

        /// <summary>
        /// A version of thr MembershipUser for use in Web Services.
        /// </summary>
        public class WSMembershipUser
        {
            #region Private Fields

            private string comment;
            private DateTime creationDate;
            private string email;
            private bool isApproved;
            private bool isLockedOut;
            private bool isOnline;
            private DateTime lastActivityDate;
            private DateTime lastLockoutDate;
            private DateTime lastLoginDate;
            private DateTime lastPasswordChangedDate;
            private string passwordQuestion;
            private string providerName;
            private object providerUserKey;
            private string userName;

            #endregion

            #region Constructor

            public WSMembershipUser()
            {
            }

            #endregion

            #region Getters and Setters

            /// <summary>
            /// Gets or sets the Comment.
            /// </summary>
            public string Comment
            {
                get
                {
                    return this.comment;
                }

                set
                {
                    this.comment = value;
                }
            }

            /// <summary>
            /// Gets or sets the CreationDate.
            /// </summary>
            public DateTime CreationDate
            {
                get
                {
                    return this.creationDate;
                }

                set
                {
                    this.creationDate = value;
                }
            }

            /// <summary>
            /// Gets or sets the Email.
            /// </summary>
            public string Email
            {
                get
                {
                    return this.email;
                }

                set
                {
                    this.email = value;
                }
            }

            /// <summary>
            /// Gets or sets a value indicating whether the user is approved.
            /// </summary>
            public bool IsApproved
            {
                get
                {
                    return this.isApproved;
                }

                set
                {
                    this.isApproved = value;
                }
            }

            /// <summary>
            /// Gets or sets a value indicating whether the user is locked out.
            /// </summary>
            public bool IsLockedOut
            {
                get
                {
                    return this.isLockedOut;
                }

                set
                {
                    this.isLockedOut = value;
                }
            }

            /// <summary>
            /// Gets or sets a value indicating whether the user is online.
            /// </summary>
            public bool IsOnline
            {
                get
                {
                    return this.isOnline;
                }

                set
                {
                    this.isOnline = value;
                }
            }

            /// <summary>
            /// Gets or sets the LastActivityDate.
            /// </summary>
            public DateTime LastActivityDate
            {
                get
                {
                    return this.lastActivityDate;
                }

                set
                {
                    this.lastActivityDate = value;
                }
            }

            /// <summary>
            /// Gets or sets the LastLockoutDate.
            /// </summary>
            public DateTime LastLockoutDate
            {
                get
                {
                    return this.lastLockoutDate;
                }

                set
                {
                    this.lastLockoutDate = value;
                }
            }

            /// <summary>
            /// Gets or sets the LastLoginDate.
            /// </summary>
            public DateTime LastLoginDate
            {
                get
                {
                    return this.lastLoginDate;
                }

                set
                {
                    this.lastLoginDate = value;
                }
            }

            /// <summary>
            /// Gets or sets the LastPasswordChangedDate.
            /// </summary>
            public DateTime LastPasswordChangedDate
            {
                get
                {
                    return this.lastPasswordChangedDate;
                }

                set
                {
                    this.lastPasswordChangedDate = value;
                }
            }

            /// <summary>
            /// Gets or sets the PasswordQuestion.
            /// </summary>
            public string PasswordQuestion
            {
                get
                {
                    return this.passwordQuestion;
                }

                set
                {
                    this.passwordQuestion = value;
                }
            }

            /// <summary>
            /// Gets or sets the ProviderName.
            /// </summary>
            public string ProviderName
            {
                get
                {
                    return this.providerName;
                }

                set
                {
                    this.providerName = value;
                }
            }

            /// <summary>
            /// Gets or sets the ProviderUserKey.
            /// </summary>
            public object ProviderUserKey
            {
                get
                {
                    return this.providerUserKey;
                }

                set
                {
                    this.providerUserKey = value;
                }
            }

            /// <summary>
            /// Gets or sets the UserName.
            /// </summary>
            public string UserName
            {
                get
                {
                    return this.userName;
                }

                set
                {
                    this.userName = value;
                }
            }

            #endregion
        }

        #endregion
    }

    #endregion
}
