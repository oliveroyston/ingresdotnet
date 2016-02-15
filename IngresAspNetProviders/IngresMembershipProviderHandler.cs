#region Code Header

/*  
 * Author               : Oliver P. Oyston (Luminary Solutions)
 * 
 * File Name            : IngresMembershipProviderHandler.cs
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
    using System.Configuration.Provider;
    using System.Data;
    using System.Data.SqlTypes;
    using System.Globalization;
    using System.Security.Cryptography;
    using System.Text;
    using System.Text.RegularExpressions;
    using System.Web.Security;
    using Ingres.Client;
    using Ingres.Web.Security.Utility;

    #endregion

    #region Ingres Membership Provider Handler

    /// <summary>
    /// Implementation for the Ingres Membership provider.
    /// </summary>
    internal sealed class IngresMembershipProviderHandler
    {
        #region Private Fields

        /// <summary>
        /// The Ingres Membership provider facade.
        /// </summary>
        private readonly IngresMembershipProvider provider;

        /// <summary>
        /// The Ingres transaction to use.
        /// </summary>
        private readonly IngresTransaction tran;

        /// <summary>
        /// The Ingres connection to use.
        /// </summary>
        private readonly IngresConnection conn;

        /// <summary>
        /// The configuration to use.
        /// </summary>
        private IngresMembershipProviderConfiguration config;

        #endregion

        #region Constructors

        /// <summary>
        /// Initializes a new instance of the IngresMembershipProviderHandler class.
        /// </summary>
        /// <param name="conn">The Ingres connection to use.</param>
        /// <param name="tran">The Ingres transaction to use.</param>
        /// <param name="config">The configuration settings to use.</param>
        /// <param name="provider">The Ingres Membership Provider facade to use.</param>
        internal IngresMembershipProviderHandler(IngresConnection conn, IngresTransaction tran, IngresMembershipProviderConfiguration config, IngresMembershipProvider provider)
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
        public string ApplicationId
        {
            // Only expose the field with a getter. We don't want anybody to set the values of this
            // manually as we need to enforce the id integrity using Guids.
            get
            {
                // cache the id in a private field and only attempt the costly get from the
                // database if our value is out of date
                if (!this.config.IsApplicationIdCurrent)
                {
                    // For convenience this is done in a seperate transaction context
                    this.config.ApplicationId = this.GetApplicationId(new IngresConnection(this.config.ConnectionString), null);

                    // We should have an Id - throw an exception if we don't
                    if (this.config.ApplicationId == null)
                    {
                        throw new ProviderException(Messages.ApplicationNotFoundInTheDatabase);
                    }
                }

                return this.config.ApplicationId;
            }
        }

        #endregion

        #region Membership Provider Implementation

        #region ChangePassword

        /// <summary>
        /// Takes, as input, a user name, a current password, and a new password, and updates the 
        /// password in the data source if the supplied user name and current password are valid. 
        /// The <c>ChangePassword</c> method returns true if the password was updated successfully; 
        /// otherwise, false.
        /// </summary>
        /// <remarks>
        /// This is the main implementation for the <c>ChangePassword</c> method of the provider. Please
        /// see the corresponding method in the Facade, which calls this method, for full documentaion. 
        /// </remarks>
        /// <param name="username">The username.</param>
        /// <param name="oldPassword">The old password.</param>
        /// <param name="newPassword">The new password.</param>
        /// <returns>Returns true if the password was updated successfully; otherwise, false.</returns>
        internal bool ChangePassword(string username, string oldPassword, string newPassword)
        {
            // Validate the parameter details
            ValidationUtil.CheckParameterIsOK(ref username, true, true, true, 256, "username");
            ValidationUtil.CheckParameterIsOK(ref oldPassword, true, true, false, 128, "oldPassword");
            ValidationUtil.CheckParameterIsOK(ref newPassword, true, true, false, 128, "newPassword");

            // Declare the 'out' variables used when checking the password
            string salt;
            MembershipPasswordFormat passwordFormat;

            // Ensure that the correct old password has been specified (and return salt and password format).
            if (!this.CheckPassword(username, oldPassword, false, false, out salt, out passwordFormat))
            {
                return false;
            }

            // Ensure that the new password meets the minimum length requirement and that
            // it is not null.
            if ((newPassword.Length < this.config.MinRequiredPasswordLength) || (newPassword == null))
            {
                throw new ProviderException(string.Format(
                              Messages.PasswordTooShort,
                              "newPassword",
                              this.config.MinRequiredPasswordLength.ToString(CultureInfo.InvariantCulture)));
            }

            // Ensure that an appropriate number non-alphanumeric characters have been specified.
            int count = 0;

            for (int i = 0; i < newPassword.Length; i++)
            {
                if (!char.IsLetterOrDigit(newPassword, i))
                {
                    count++;
                }
            }

            if (count < this.config.MinRequiredNonAlphanumericCharacters)
            {
                throw new ArgumentException(string.Format(
                              Messages.NeedMoreNonAlphanumericChars,
                              "newPassword",
                              this.config.MinRequiredNonAlphanumericCharacters.ToString(CultureInfo.InvariantCulture)));
            }

            // Ensure that the password meets any regex strength requirements
            if (this.config.PasswordStrengthRegularExpression.Length > 0)
            {
                if (!Regex.IsMatch(newPassword, this.config.PasswordStrengthRegularExpression))
                {
                    throw new ArgumentException(string.Format(Messages.DoesNotMatchRegex, "newPassword"));
                }
            }

            string pass = this.EncodePassword(newPassword, passwordFormat, salt);

            // Ensure that our new password does not exceed the maximum length allowed in the database.
            if (pass.Length > 128)
            {
                throw new ArgumentException(string.Format(Messages.PasswordTooLong), "newPassword");
            }

            ValidatePasswordEventArgs args = new ValidatePasswordEventArgs(username, newPassword, true);

            // Raise the 'on validating password' event.
            this.provider.OnValidatingPassword(args);

            if (args.Cancel)
            {
                // The change password event is to be cancelled.
                if (args.FailureInformation != null)
                {
                    throw args.FailureInformation;
                }

                throw new ArgumentException(string.Format(Messages.CustomPasswordValidationFailed), "newPassword");
            }

            string sql = @"
                            UPDATE aspnet_Membership
                            FROM
                                aspnet_Applications,
                                aspnet_Users
                            SET Password                = ?,
                                PasswordFormat          = ?,
                                PasswordSalt            = ?,
                                LastPasswordChangedDate = ?
                            WHERE 
                                aspnet_Users.LoweredUserName               = ?
                            AND aspnet_Applications.LoweredApplicationName = ?
                            AND aspnet_Applications.ApplicationId          = aspnet_Users.ApplicationId
                            AND aspnet_Users.UserId                        = aspnet_Membership.UserId
                           ";

            // Create a new command and enrol in the current transaction.
            IngresCommand cmd = new IngresCommand(sql, this.conn);

            cmd.Transaction    = this.tran;
            cmd.CommandTimeout = this.config.CommandTimeout;

            // Add the required parameters
            cmd.Parameters.Add("Password", DbType.String).Value                  = pass;
            cmd.Parameters.Add("PasswordFormat", DbType.Int32).Value             = (int)passwordFormat;
            cmd.Parameters.Add("PasswordSalt", DbType.String).Value              = salt;
            cmd.Parameters.Add("LastPasswordChangedDate", DbType.DateTime).Value = DateTime.Now;
            cmd.Parameters.Add("LoweredUserName", DbType.String).Value           = username.ToLower();
            cmd.Parameters.Add("LoweredApplicationName", DbType.String).Value    = this.config.ApplicationName.ToLower();

            // Execute the command and ensure that only one row was updated.
            int rowsAffected = cmd.ExecuteNonQuery();

            // Ensure that no more than one row was updated.
            if (rowsAffected > 1)
            {
                throw new Exception(Messages.MoreThanOneRowWasAffectedWhenAttemptingToChangeAPassword);
            }

            // Return whether any rows were affected or not.
            return rowsAffected > 0;
        }

        #endregion

        #region ChangePasswordQuestionAndAnswer

        /// <summary>
        /// Takes, as input, a user name, a password, a password question, and a password answer, 
        /// and updates the password question and answer in the data source if the supplied user 
        /// name and password are valid. The <c>ChangePasswordQuestionAndAnswer</c> method returns true if 
        /// the password question and answer are updated successfully; otherwise, false.
        /// </summary>
        /// <remarks>
        /// This is the main implementation for the <c>ChangePasswordQuestionAndAnswer</c> method of the provider. Please
        /// see the corresponding method in the Facade, which calls this method, for full documentaion. 
        /// </remarks>
        /// <param name="username">The username.</param>
        /// <param name="password">The password.</param>
        /// <param name="newPasswordQuestion">The new password question.</param>
        /// <param name="newPasswordAnswer">The new password answer.</param>
        /// <returns>Returns true if the password question and answer are updated successfully; otherwise, false.</returns>
        internal bool ChangePasswordQuestionAndAnswer(
                                                     string username,
                                                     string password,
                                                     string newPasswordQuestion,
                                                     string newPasswordAnswer)
        {
            // Check the parameters
            ValidationUtil.CheckParameterIsOK(ref username, true, true, true, 256, "username");
            ValidationUtil.CheckParameterIsOK(ref password, true, true, false, 128, "password");

            string salt;
            MembershipPasswordFormat passwordFormat;

            // Check the password
            if (!this.CheckPassword(username, password, false, false, out salt, out passwordFormat))
            {
                return false;
            }

            ValidationUtil.CheckParameterIsOK(ref newPasswordQuestion, this.config.RequiresQuestionAndAnswer, this.config.RequiresQuestionAndAnswer, false, 256, "newPasswordQuestion");

            // validate and encode the password answer
            string encodedPasswordAnswer;

            if (newPasswordAnswer != null)
            {
                newPasswordAnswer = newPasswordAnswer.Trim();
            }

            ValidationUtil.CheckParameterIsOK(ref newPasswordAnswer, this.config.RequiresQuestionAndAnswer, this.config.RequiresQuestionAndAnswer, false, 128, "newPasswordAnswer");

            if (!string.IsNullOrEmpty(newPasswordAnswer))
            {
                encodedPasswordAnswer = this.EncodePassword(newPasswordAnswer.ToLower(CultureInfo.InvariantCulture), passwordFormat, salt);
            }
            else
            {
                encodedPasswordAnswer = newPasswordAnswer;
            }

            ValidationUtil.CheckParameterIsOK(ref encodedPasswordAnswer, this.config.RequiresQuestionAndAnswer, this.config.RequiresQuestionAndAnswer, false, 128, "newPasswordAnswer");

            // build up the required SQL
            string sql = @"
                            UPDATE aspnet_Membership
                            FROM
                                aspnet_Applications,
                                aspnet_Users
                            SET PasswordQuestion = ?,
                                PasswordAnswer   = ?
                            WHERE 
                                aspnet_Users.LoweredUserName               = ?
                            AND aspnet_Applications.LoweredApplicationName = ?
                            AND aspnet_Applications.ApplicationId          = aspnet_Users.ApplicationId
                            AND aspnet_Users.UserId                        = aspnet_Membership.UserId

                           ";

            // Create a new command and enrol in the current transaction
            IngresCommand cmd = new IngresCommand(sql, this.conn);
            
            cmd.Transaction    = this.tran;
            cmd.CommandTimeout = this.config.CommandTimeout;

            // Add the required parameters
            cmd.Parameters.Add("PasswordQuestion", DbType.String).Value       = newPasswordQuestion;
            cmd.Parameters.Add("PasswordAnswer", DbType.String).Value         = encodedPasswordAnswer;
            cmd.Parameters.Add("LoweredUsername", DbType.String).Value        = username.ToLower();
            cmd.Parameters.Add("LoweredApplicationName", DbType.String).Value = this.config.ApplicationName.ToLower();

            int rowsAffected = cmd.ExecuteNonQuery();

            // Ensure that no more than one row was updated.
            if (rowsAffected > 1)
            {
                throw new Exception(Messages.MoreThanOneRowWasAffectedWhenAttemptingToChangeAPassword);
            }

            return rowsAffected > 0;
        }

        #endregion

        #region CreateUser

        /// <summary>
        /// Takes, as input, the name of a new user, a password, and an email address and inserts 
        /// a new user for the application into the data source. The <c>CreateUser</c> method returns a 
        /// <c>MembershipUser</c> object populated with the information for the newly created user. 
        /// The <c>CreateUser</c> method also defines an out parameter that returns a 
        /// <c>MembershipCreateStatus</c> value that indicates whether the user was successfully created, 
        /// or a reason that the user was not successfully created.
        /// </summary>
        /// <remarks>
        /// This is the main implementation for the <c>CreateUser</c> method of the provider. Please
        /// see the corresponding method in the Facade, which calls this method, for full documentaion. 
        /// </remarks>
        /// <param name="username">name of the new user.</param>
        /// <param name="password">password for the new user.</param>
        /// <param name="email">email address of the new user.</param>
        /// <param name="passwordQuestion">password reset question for the new user.</param>
        /// <param name="passwordAnswer">password reset answer for the new user.</param>
        /// <param name="isApproved">a boolean indicating whether the user has been approved or not</param>
        /// <param name="providerUserKey">the identifier/key for the user.</param>
        /// <param name="status">membership creation status for the user.</param>
        /// <returns>A MembershipUser object populated with the information for the newly created user.</returns>
        internal MembershipUser CreateUser(
                                          string username,
                                          string password,
                                          string email,
                                          string passwordQuestion,
                                          string passwordAnswer,
                                          bool isApproved,
                                          object providerUserKey,
                                          out MembershipCreateStatus status)
        {
            // Ensure that the password is valid
            if (!ValidationUtil.IsParameterValid(ref password, true, true, false, 128))
            {
                status = MembershipCreateStatus.InvalidPassword;
                return null;
            }

            // Get a new salt and then encode the password
            string salt = PasswordUtil.GetSalt(16);

            string pass = this.EncodePassword(password, this.config.PasswordFormat, salt);

            // Ensure that the password does not exceed the maximum allowed length
            if (pass.Length > 128)
            {
                status = MembershipCreateStatus.InvalidPassword;
                return null;
            }

            // Encode the password answer and validate.
            string encodedPasswordAnswer;

            if (passwordAnswer != null)
            {
                passwordAnswer = passwordAnswer.Trim();
            }

            if (!string.IsNullOrEmpty(passwordAnswer))
            {
                if (passwordAnswer.Length > 128)
                {
                    status = MembershipCreateStatus.InvalidAnswer;
                    return null;
                }

                encodedPasswordAnswer = this.EncodePassword(passwordAnswer.ToLower(CultureInfo.InvariantCulture), this.config.PasswordFormat, salt);
            }
            else
            {
                encodedPasswordAnswer = passwordAnswer;
            }

            if (!ValidationUtil.IsParameterValid(ref encodedPasswordAnswer, this.config.RequiresQuestionAndAnswer, true, false, 128))
            {
                status = MembershipCreateStatus.InvalidAnswer;
                return null;
            }

            // Validate username.
            if (!ValidationUtil.IsParameterValid(ref username, true, true, true, 256))
            {
                status = MembershipCreateStatus.InvalidUserName;
                return null;
            }

            // Validate email
            if (!ValidationUtil.IsParameterValid(
                                                  ref email,
                                                  this.config.RequiresUniqueEmail,
                                                  this.config.RequiresUniqueEmail,
                                                  false,
                                                  256))
            {
                status = MembershipCreateStatus.InvalidEmail;
                return null;
            }

            // Validate password question.
            if (!ValidationUtil.IsParameterValid(ref passwordQuestion, this.config.RequiresQuestionAndAnswer, true, false, 256))
            {
                status = MembershipCreateStatus.InvalidQuestion;
                return null;
            }

            // Ensure that the key is valid (i.e. a valid Guid).
            if (providerUserKey != null)
            {
                if (!(providerUserKey is Guid))
                {
                    status = MembershipCreateStatus.InvalidProviderUserKey;
                    return null;
                }
            }

            // Check that the password is of a sufficient length.
            if (password.Length < this.config.MinRequiredPasswordLength)
            {
                status = MembershipCreateStatus.InvalidPassword;
                return null;
            }

            // Ensure that the password has the requires number of non-alphanumeric characters.
            int count = 0;

            for (int i = 0; i < password.Length; i++)
            {
                if (!char.IsLetterOrDigit(password, i))
                {
                    count++;
                }
            }

            if (count < this.config.MinRequiredNonAlphanumericCharacters)
            {
                status = MembershipCreateStatus.InvalidPassword;
                return null;
            }

            // Check that the password meets the required regex strength
            if (this.config.PasswordStrengthRegularExpression.Length > 0)
            {
                if (!Regex.IsMatch(password, this.config.PasswordStrengthRegularExpression))
                {
                    status = MembershipCreateStatus.InvalidPassword;
                    return null;
                }
            }

            // Raise the 'on validating password' event.
            ValidatePasswordEventArgs args = new ValidatePasswordEventArgs(username, password, true);

            this.provider.OnValidatingPassword(args);

            if (args.Cancel)
            {
                status = MembershipCreateStatus.InvalidPassword;
                return null;
            }

            // Make sure that if we require a unique email that there are
            // not any existing users with the users email.
            if (this.config.RequiresUniqueEmail && (this.GetUserNameByEmail(email) != string.Empty))
            {
                status = MembershipCreateStatus.DuplicateEmail;
                return null;
            }

            // Ensure the user doesn't exist
            MembershipUser user = this.GetUser(username, false);

            // The user doesn't exist so create him
            if (user == null)
            {
                this.CreateUserForApplication(username, false, DateTime.Now, ref providerUserKey);

                string sql = @"
                                INSERT INTO aspnet_Membership
                                   (ApplicationId,
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
                                    Comment)   
                                VALUES
                                   (?,
                                    ?,
                                    ?,
                                    ?,
                                    ?,
                                    ?,
                                    ?,
                                    ?,
                                    ?,
                                    ?,
                                    ?,
                                    ?,
                                    ?,
                                    ?,
                                    ?,
                                    ?,
                                    ?,
                                    ?,
                                    ?,
                                    ?,
                                    ?)
                                   ";

                // Create a new command and enrol in the current transaction
                IngresCommand cmd = new IngresCommand(sql, this.conn);
                
                cmd.Transaction    = this.tran;
                cmd.CommandTimeout = this.config.CommandTimeout;

                // Add the required parameters
                cmd.Parameters.Add("ApplicationId", DbType.String).Value                     = this.ApplicationId;
                cmd.Parameters.Add("UserId", DbType.String).Value                            = providerUserKey.ToString();
                cmd.Parameters.Add("Password", DbType.String).Value                          = pass;
                cmd.Parameters.Add("PasswordFormat", DbType.Int32).Value                     = (int)this.config.PasswordFormat;
                cmd.Parameters.Add("PasswordSalt", DbType.String).Value                      = salt;
                cmd.Parameters.Add("MobilePIN", DbType.String).Value                         = "PIN";
                cmd.Parameters.Add("Email", DbType.String).Value                             = email;
                cmd.Parameters.Add("LoweredEmail", DbType.String).Value                      = email.ToLower();
                cmd.Parameters.Add("PasswordQuestion", DbType.String).Value                  = passwordQuestion;
                cmd.Parameters.Add("PasswordAnswer", DbType.String).Value                    = encodedPasswordAnswer;
                cmd.Parameters.Add("IsApproved", DbType.String).Value                        = isApproved ? '1' : '0';
                cmd.Parameters.Add("IsLockedOut", DbType.String).Value                       = '0';
                cmd.Parameters.Add("CreateDate", DbType.DateTime).Value                      = DateTime.Now.ToString();
                cmd.Parameters.Add("LastLoginDate", DbType.DateTime).Value                   = DateTime.Now.ToString();
                cmd.Parameters.Add("LastPasswordChangedDate", DbType.DateTime).Value         = DateTime.Now.ToString();
                cmd.Parameters.Add("LastLockoutDate", DbType.DateTime).Value                 = DateTime.Now.ToString();
                cmd.Parameters.Add("FailPwdAttemptCount", DbType.Int32).Value                = 0;
                cmd.Parameters.Add("FailPwdAttemptWindowStart", DbType.DateTime).Value       = DateTime.Now.ToString();
                cmd.Parameters.Add("FailPwdAnswerAttemptCount", DbType.Int32).Value          = 0;
                cmd.Parameters.Add("FailPwdAnswerAttemptWindowStart", DbType.DateTime).Value = DateTime.Now.ToString();
                cmd.Parameters.Add("Comment", DbType.String).Value                           = "Created by the Ingres ASP.NET Membership Provider.";

                int rows = cmd.ExecuteNonQuery();

                if (rows > 1)
                {
                    throw new Exception(Messages.MoreThanOneRowWasAffected);
                }

                status = rows > 0 ? MembershipCreateStatus.Success : MembershipCreateStatus.UserRejected;

                return this.GetUser(username, false);
            }

            // The user existed so we return duplicate user name.
            status = MembershipCreateStatus.DuplicateUserName;

            return null;
        }

        #endregion

        #region DeleteUser

        /// <summary>
        /// Takes, as input, the name of a user and deletes that user's information from the data 
        /// source. The <c>DeleteUser</c> method returns true if the user was successfully deleted; 
        /// otherwise, false. An additional Boolean parameter is included to indicate whether 
        /// related information for the user, such as role or profile information is also deleted.
        /// </summary>
        /// <remarks>
        /// This is the main implementation for the <c>DeleteUser</c> method of the provider. Please
        /// see the corresponding method in the Facade, which calls this method, for full documentaion. 
        /// </remarks>
        /// <param name="username">The username to delete.</param>
        /// <param name="deleteAllRelatedData">Whether to delete all related data or not.</param>
        /// <returns>Returns true if the user was successfully deleted; otherwise, false.</returns>
        internal bool DeleteUser(string username, bool deleteAllRelatedData)
        {
            Guid userId;

            if (!this.GetUserIdByName(username, out userId))
            {
                throw new ProviderException(Messages.UserDoesNotExist);
            }

            // If the username is an empty string then throw an argument exception.
            ValidationUtil.CheckParameterIsOK(ref username, true, true, true, 256, "username");

            string sql = @"
                            DELETE FROM aspnet_Membership
                            WHERE 
                                UserId = ?
                           ";

            // create a new command and enrol in the current transaction
            IngresCommand cmd = new IngresCommand(sql, this.conn);

            cmd.Transaction    = this.tran;
            cmd.CommandTimeout = this.config.CommandTimeout;

            // Add the required parameters
            cmd.Parameters.Add("UserId", DbType.String).Value = userId.ToString();

            // Execute the query and check that an appropriate number of rows have been affected.
            int rows = cmd.ExecuteNonQuery();

            if (rows == 0)
            {
                return false;
            }

            if (rows != 1)
            {
                throw new Exception(Messages.MoreThanOneUserWouldHaveBeenDeleted);
            }

            if (deleteAllRelatedData)
            {
                // Delete from the Users in Roles table
                sql =
                    @"
                        DELETE FROM aspnet_UsersInRoles 
                        WHERE 
                            UserId = ?";

                cmd = new IngresCommand(sql, this.conn);

                cmd.Transaction    = this.tran;
                cmd.CommandTimeout = this.config.CommandTimeout;

                // Add the required parameters
                cmd.Parameters.Add("UserId", DbType.String).Value = userId.ToString();

                // Execute the query. 
                cmd.ExecuteNonQuery();

                // Delete from the Users table
                sql = @"
                        DELETE FROM aspnet_Users 
                        WHERE 
                            UserId = ?";

                cmd = new IngresCommand(sql, this.conn);

                cmd.Transaction    = this.tran;
                cmd.CommandTimeout = this.config.CommandTimeout;

                // Add the required parameters
                cmd.Parameters.Add("UserId", DbType.String).Value = userId.ToString();

                // Execute the query. 
                rows = cmd.ExecuteNonQuery();

                if (rows != 1)
                {
                    throw new Exception(Messages.ASingleUserShouldHaveBeenDeleted);
                }

                // We would also delete from the profile table when the profile provider is implemented.

                // We would also delete from the personalization table when the personalization provider is implemented.
            }

            // The user was successfully deleted and so return true.
            return true;
        }

        #endregion

        #region GetAllUsers

        /// <summary>
        /// Returns a <c>MembershipUserCollection</c> populated with <c>MembershipUser</c> objects for all of the 
        /// users in the data source.
        /// </summary>
        /// <remarks>
        /// This is the main implementation for the <c>GetAllUsers</c> method of the provider. Please
        /// see the corresponding method in the Facade, which calls this method, for full documentaion. 
        /// </remarks>
        /// <param name="pageIndex">Which page to return.</param>
        /// <param name="pageSize">The maximum number of users to return.</param>
        /// <param name="totalRecords">[out] The total number of users.</param>
        /// <returns>Returns a MembershipUserCollection populated with MembershipUser objects 
        /// for all of the users in the data source.</returns>
        internal MembershipUserCollection GetAllUsers(int pageIndex, int pageSize, out int totalRecords)
        {
            // Validate the input.
            if (pageIndex < 0)
            {
                throw new ArgumentException(string.Format(Messages.PageIndexInvalid), "pageIndex");
            }

            if (pageSize < 1)
            {
                throw new ArgumentException(string.Format(Messages.PageSizeInvalid), "pageSize");
            }

            long upperBound = ((long)pageIndex * pageSize) + (pageSize - 1);

            if (upperBound > Int32.MaxValue)
            {
                throw new ArgumentException(string.Format(Messages.PageIndexAndPageSizeCombinationInvalid), "pageIndex and pageSize");
            }

            // Put the SQL in a string.
            string sql = @"
                            SELECT 
                                Count(*) 
                            FROM
                                aspnet_Membership,
                                aspnet_Applications
                            WHERE 
                                aspnet_Applications.LoweredApplicationName = ?
                            AND aspnet_Applications.ApplicationId          = aspnet_Membership.ApplicationId";

            // Create a new command and enrol it in the current transaction
            IngresCommand cmd = new IngresCommand(sql, this.conn);

            cmd.Transaction    = this.tran;
            cmd.CommandTimeout = this.config.CommandTimeout;

            // Add the requires parameters
            cmd.Parameters.Add("ApplicationName", DbType.String).Value = this.config.ApplicationName.ToLower();

            MembershipUserCollection users = new MembershipUserCollection();

            // The result is the number of users that we have
            totalRecords = Convert.ToInt32(cmd.ExecuteScalar());

            // If we don't have any users then just return the empty MembershipUserCollection
            if (totalRecords <= 0)
            {
                return users;
            }

            // Create a new Ingres command
            cmd = new IngresCommand();

            cmd.Connection = this.conn;
            cmd.Transaction = this.tran;
            cmd.CommandTimeout = this.config.CommandTimeout;

            // Add the required parameters.
            cmd.Parameters.Add("ApplicationName", DbType.String).Value = this.config.ApplicationName.ToLower();

            // Build up the SQL to retrieve the users for the application.
            cmd.CommandText = @"
                                SELECT  
                                    aspnet_Membership.Email, 
                                    aspnet_Membership.PasswordQuestion, 
                                    aspnet_Membership.Comment, 
                                    aspnet_Membership.IsApproved,
                                    aspnet_Membership.CreateDate, 
                                    aspnet_Membership.LastLoginDate, 
                                    aspnet_Membership.LastPasswordChangedDate,
                                    aspnet_Users.UserId,
                                    aspnet_Users.UserName, 
                                    aspnet_Membership.IsLockedOut,
                                    aspnet_Membership.LastLockoutDate,
                                    aspnet_Users.LastActivityDate
                                FROM    
                                    aspnet_Users, 
                                    aspnet_Membership,
                                    aspnet_Applications
                                WHERE
                                    aspnet_Users.UserId                        = aspnet_Membership.UserId
                                AND aspnet_Applications.LoweredApplicationName = ?
                                AND aspnet_Applications.ApplicationId          = aspnet_Membership.ApplicationId
                                ORDER BY 
                                    Username ASC
                               ";

            // Execute the command as a reader and return the correct users as stipulated by the paging options
            IngresDataReader reader = cmd.ExecuteReader();

            int counter    = 0;
            int startIndex = (pageSize * pageIndex) + 1;
            int endIndex   = pageSize * (pageIndex + 1);

            // Note: This might have to be done differently if the performance is too poor!
            while (reader.Read())
            {
                counter++;

                if ((counter >= startIndex) && (counter <= endIndex))
                {
                    MembershipUser user = this.GetUserFromReader(reader);

                    users.Add(user);
                }

                if (counter >= endIndex)
                {
                    cmd.Cancel();
                }            
            }

            return users;
        }

        #endregion

        #region GetNumberOfUsersOnline

        /// <summary>
        /// Returns an integer value that is the count of all the users in the data source where 
        /// the <c>LastActivityDate</c> is greater than the current date and time minus the 
        /// <c>UserIsOnlineTimeWindow</c> property. The <c>UserIsOnlineTimeWindow</c> property is an integer 
        /// value specifying the number of minutes to use when determining whether a user is online.
        /// </summary>
        /// <remarks>
        /// This is the main implementation for the <c>GetNumberOfUsersOnline</c> method of the provider. Please
        /// see the corresponding method in the Facade, which calls this method, for full documentaion. 
        /// </remarks>
        /// <param name="conn">The Ingres connection to use.</param>
        /// <param name="tran">The Ingres transaction to use.</param>
        /// <returns>The number of users online.</returns> 
        internal int GetNumberOfUsersOnline(IngresConnection conn, IngresTransaction tran)
        {
            TimeSpan onlineSpan = new TimeSpan(0, Membership.UserIsOnlineTimeWindow, 0);
            DateTime compareTime = DateTime.Now.Subtract(onlineSpan);

            string sql = @"
                            SELECT 
                                Count(*) 
                            FROM 
                                aspnet_Membership,
                                aspnet_Applications,
                                aspnet_Users
                            WHERE 
                                aspnet_Users.LastActivityDate              > ? 
                            AND aspnet_Applications.LoweredApplicationName = ?
                            AND aspnet_Membership.ApplicationId            = aspnet_Applications.ApplicationId
                            AND aspnet_Users.UserId                        = aspnet_Membership.UserId
                           ";

            // Create the Ingres command.
            IngresCommand cmd = new IngresCommand(sql, this.conn);

            cmd.Transaction    = this.tran;
            cmd.CommandTimeout = this.config.CommandTimeout;

            // Add the required parameters.
            cmd.Parameters.Add("CompareDate", DbType.DateTime).Value          = compareTime;
            cmd.Parameters.Add("LoweredApplicationName", DbType.String).Value = this.config.ApplicationName.ToLower();

            // Execute the command to obtain the number of users online
            int numOnline = Convert.ToInt32(cmd.ExecuteScalar());

            return numOnline;
        }

        #endregion

        #region GetPassword

        /// <summary>
        /// Takes, as input, a user name and a password answer and retrieves the password for that 
        /// user from the data source and returns the password as a string.
        /// </summary>
        /// <remarks>
        /// This is the main implementation for the <c>GetPassword</c> method of the provider. Please
        /// see the corresponding method in the Facade, which calls this method, for full documentaion. 
        /// </remarks>
        /// <param name="username">The username.</param>
        /// <param name="answer">The password answer.</param>
        /// <returns>The password for the given username.</returns>
        internal string GetPassword(string username, string answer)
        {
            // Ensure that password retrieval is supported
            if (!this.config.EnablePasswordRetrieval)
            {
                throw new NotSupportedException(string.Format(Messages.PasswordRetrievalNotSupported));
            }

            // If an answer has been specified then convert it to lower case
            if (answer != null)
            {
                answer = answer.ToLower();
            }

            string sql = @"
                            SELECT  
                                aspnet_Users.UserId, 
                                aspnet_Membership.IsLockedOut, 
                                aspnet_Membership.Password, 
                                aspnet_Membership.PasswordFormat,
                                aspnet_Membership.PasswordSalt,
                                aspnet_Membership.PasswordAnswer, 
                                aspnet_Membership.FailPwdAttemptCount,
                                aspnet_Membership.FailPwdAnswerAttemptCount,
                                aspnet_Membership.IsApproved,
                                aspnet_Users.LastActivityDate, 
                                aspnet_Membership.LastLoginDate
                            FROM
                                aspnet_Applications,
                                aspnet_Users, 
                                aspnet_Membership
                            WHERE   
                                aspnet_Applications.LoweredApplicationName = ?
                            AND aspnet_Users.ApplicationId                 = aspnet_Applications.ApplicationId    
                            AND aspnet_Users.UserId                        = aspnet_Membership.UserId 
                            AND aspnet_Users.LoweredUserName               = ?
                           ";

            // Create the Ingres command.
            IngresCommand cmd = new IngresCommand(sql, this.conn);

            cmd.Transaction = this.tran;
            cmd.CommandTimeout = this.config.CommandTimeout;

            // Add the required parameters.
            cmd.Parameters.Add("LoweredApplicationName", DbType.String).Value = this.config.ApplicationName.ToLower();
            cmd.Parameters.Add("LoweredUserName", DbType.String).Value        = username.ToLower();

            // Execute the command and retrieve the required information
            string password;
            MembershipPasswordFormat usersPasswordFormat;
            string passwordSalt;
            string passwordAnswer;

            IngresDataReader reader = cmd.ExecuteReader(CommandBehavior.SingleRow);

            if (reader.HasRows)
            {
                reader.Read();

                try
                {
                    // Check that the user is not locked out.
                    if (DBUtil.ColValAsString(reader, "IsLockedOut") == "1" ? true : false)
                    {
                        throw new MembershipPasswordException(Messages.TheUserIsLockedOut);
                    }

                    // Determine the password format.
                    int tempPasswordFormat = DBUtil.ColValAsInt32(reader, "PasswordFormat");

                    switch (tempPasswordFormat)
                    {
                        case 0:
                            usersPasswordFormat = MembershipPasswordFormat.Clear;
                            break;
                        case 1:
                            usersPasswordFormat = MembershipPasswordFormat.Hashed;
                            break;
                        case 2:
                            usersPasswordFormat = MembershipPasswordFormat.Encrypted;
                            break;
                        default:
                            throw new ProviderException(Messages.PasswordIsStoredInAUnrecognisedFormat);
                    }

                    // Retrieved the password, salt and password answer.
                    password       = DBUtil.ColValAsString(reader, "Password");
                    passwordSalt   = DBUtil.ColValAsString(reader, "PasswordSalt");
                    passwordAnswer = DBUtil.ColValAsString(reader, "PasswordAnswer");
                }
                catch (Exception)
                {
                    throw new ProviderException(Messages.ErrorInTheGetPasswordMethod);
                }
            }
            else
            {
                // no errors occurred but we didnt have any rows -> user does not exist
                throw new MembershipPasswordException(Messages.SuppliedUserNameWasNotFound);
            }

            reader.Close();

            // If we require question and answer and the wrong answer was given...
            if (this.config.RequiresQuestionAndAnswer && !this.CheckPassword(answer, passwordAnswer, usersPasswordFormat, passwordSalt))
            {
                // ...updatate the failure count and throw an exception.
                this.UpdateFailureCount(username, FailureReason.PasswordAnswer);

                // If we got here then commit the transaction and then handle the exception
                this.tran.Commit();

                // Set the transaction to null so that we don't attempt to roll it back.
                this.tran.Dispose();

                throw new MembershipPasswordException(Messages.IncorrectPasswordAnswer);
            }

            // If the password was hashed we can't return it!
            if (usersPasswordFormat == MembershipPasswordFormat.Hashed)
            {
                throw new ProviderException(Messages.CannotUnencodeAHashedPassword);
            }

            // If the password is encrypted then we need do decrypt it before returning it.
            if (usersPasswordFormat == MembershipPasswordFormat.Encrypted)
            {
                password = this.DecodePassword(password, usersPasswordFormat);
            }

            // Otherwise the password is just a "clear" password and so we don't need to do anything to it.
            return password;
        }

        #endregion

        #region GetUser (by username)

        /// <summary>
        /// Takes, as input, a user name and a Boolean value indicating whether to update the 
        /// <c>LastActivityDate</c> value for the user to show that the user is currently online. The 
        /// <c>GetUser</c> method returns a <c>MembershipUser</c> object populated with current values from the 
        /// data source for the specified user. If the user name is not found in the data source, 
        /// the <c>GetUser</c> method returns <c>null</c>.
        /// </summary>
        /// <remarks>
        /// This is the main implementation for the <c>GetUser</c> method of the provider. Please
        /// see the corresponding method in the Facade, which calls this method, for full documentaion. 
        /// </remarks>
        /// <param name="username">The username.</param>
        /// <param name="userIsOnline">Whether the user us currently online.</param>
        /// <returns>The membership user with the specified username.</returns>
        internal MembershipUser GetUser(string username, bool userIsOnline)
        {
            // Ensure that a username has been provided
            if (string.IsNullOrEmpty(username))
            {
                throw new ProviderException(Messages.UsernameMustBeSuppliedToGetAUser);
            }

            string sql = @"
                            SELECT TOP 1 
                                aspnet_Membership.Email, 
                                aspnet_Membership.PasswordQuestion, 
                                aspnet_Membership.Comment, 
                                aspnet_Membership.IsApproved,
                                aspnet_Membership.CreateDate, 
                                aspnet_Membership.LastLoginDate, 
                                aspnet_Membership.LastPasswordChangedDate,
                                aspnet_Users.UserId,
                                aspnet_Users.UserName,
                                aspnet_Membership.IsLockedOut,
                                aspnet_Membership.LastLockoutDate,
                                aspnet_Users.LastActivityDate
                            FROM 
                                aspnet_Applications, 
                                aspnet_Users, 
                                aspnet_Membership
                            WHERE    
                                aspnet_Applications.LoweredApplicationName = ?
                            AND aspnet_Users.ApplicationId                 = aspnet_Applications.ApplicationId    
                            AND aspnet_Users.LoweredUserName               = ?
                            AND aspnet_Users.UserId                        = aspnet_Membership.UserId
                           ";

            IngresCommand cmd = new IngresCommand(sql, this.conn);

            cmd.Transaction    = this.tran;
            cmd.CommandTimeout = this.config.CommandTimeout;

            cmd.Parameters.Add("LoweredApplicationName", DbType.String).Value = this.config.ApplicationName.ToLower();
            cmd.Parameters.Add("LoweredUserName", DbType.String).Value        = username.ToLower();

            // Innocuously initialise the user
            MembershipUser user = null;

            IngresDataReader reader = cmd.ExecuteReader();

            if (reader.HasRows)
            {
                reader.Read();

                user = this.GetUserFromReader(reader);

                reader.Close();

                if (userIsOnline)
                {
                    sql = @"
                            UPDATE aspnet_Users 
                            FROM
                                aspnet_Applications,
                                aspnet_Membership
                            SET LastActivityDate = ?
                            WHERE 
                                aspnet_Users.UserId                        = aspnet_Membership.UserId
                            AND aspnet_Applications.ApplicationId          = aspnet_Membership.ApplicationId
                            AND aspnet_Users.LoweredUsername               = ? 
                            AND aspnet_Applications.LoweredApplicationName = ?
                           ";

                    IngresCommand updateCmd = new IngresCommand(sql, this.conn);

                    updateCmd.Transaction    = this.tran;
                    updateCmd.CommandTimeout = this.config.CommandTimeout;

                    updateCmd.Parameters.Add("ActivityDate", DbType.DateTime).Value         = DateTime.Now;
                    updateCmd.Parameters.Add("LoweredUsername", DbType.String).Value        = username.ToLower();
                    updateCmd.Parameters.Add("LoweredApplicationName", DbType.String).Value = this.config.ApplicationName.ToLower();

                    int rows = updateCmd.ExecuteNonQuery();

                    if (rows != 1)
                    {
                        throw new Exception(Messages.FailedToUpdateLastActivityDate);
                    }
                }
            }

            if (!reader.IsClosed)
            {
                reader.Close();
            }

            return user;
        }

        #endregion

        #region GetUser (by provider user key)

        /// <summary>
        /// Takes, as input, a unique user identifier and a Boolean value indicating whether to 
        /// update the <c>LastActivityDate</c> value for the user to show that the user is currently 
        /// online. The <c>GetUser</c> method returns a <c>MembershipUser</c> object populated with current 
        /// values from the data source for the specified user. If the user name is not found in 
        /// the data source, the <c>GetUser</c> method returns null.
        /// </summary>
        /// <remarks>
        /// This is the main implementation for the <c>GetUser</c> method of the provider. Please
        /// see the corresponding method in the Facade, which calls this method, for full documentaion. 
        /// </remarks>
        /// <param name="providerUserKey">The unique indentifer for the user.</param>
        /// <param name="userIsOnline">Whether the user is online.</param>
        /// <returns>The membership user with the specified provider user key.</returns>
        internal MembershipUser GetUser(object providerUserKey, bool userIsOnline)
        {
            // Validate the input. The providerUserKey should be a Guid
            if (providerUserKey == null)
            {
                throw new ArgumentNullException("providerUserKey");
            }

            if (!(providerUserKey is Guid))
            {
                throw new ArgumentException(string.Format(Messages.InvalidProviderUserKey), "providerUserKey");
            }

            string sql = @"
                            SELECT  
                                aspnet_Membership.Email, 
                                aspnet_Membership.PasswordQuestion, 
                                aspnet_Membership.Comment, 
                                aspnet_Membership.IsApproved,
                                aspnet_Membership.CreateDate, 
                                aspnet_Membership.LastLoginDate, 
                                aspnet_Membership.LastPasswordChangedDate,
                                aspnet_Users.UserId,
                                aspnet_Users.UserName, 
                                aspnet_Membership.IsLockedOut,
                                aspnet_Membership.LastLockoutDate,
                                aspnet_Users.LastActivityDate
                            FROM    
                                aspnet_Users, 
                                aspnet_Membership
                            WHERE
                                aspnet_Users.UserId = ?
                            AND aspnet_Users.UserId = aspnet_Membership.UserId
                           ";

            IngresCommand cmd = new IngresCommand(sql, this.conn);

            cmd.Transaction    = this.tran;
            cmd.CommandTimeout = this.config.CommandTimeout;

            cmd.Parameters.Add("UserId", DbType.String).Value = providerUserKey.ToString().ToLower();

            MembershipUser user = null;

            IngresDataReader reader = cmd.ExecuteReader();

            if (reader.HasRows)
            {
                reader.Read();

                user = this.GetUserFromReader(reader);

                if (userIsOnline)
                {
                    sql = @"
                            UPDATE aspnet_Users
                            SET
                                LastActivityDate = ?
                            WHERE
                                UserId = ?
                           ";

                    cmd = new IngresCommand(sql, this.conn);

                    cmd.Transaction    = this.tran;
                    cmd.CommandTimeout = this.config.CommandTimeout;

                    cmd.Parameters.Add("LastActivityDate", DbType.DateTime).Value = DateTime.Now;
                    cmd.Parameters.Add("UserId", DbType.String).Value             = providerUserKey.ToString().ToLower();

                    cmd.ExecuteNonQuery();
                }
            }

            return user;
        }

        #endregion

        #region UnlockUser

        /// <summary>
        /// Takes, as input, a user name, and updates the field in the data source that stores the 
        /// IsLockedOut property to false. The <c>UnlockUser</c> method returns true if the record for the 
        /// membership user is updated successfully; otherwise false.
        /// </summary>
        /// <remarks>
        /// This is the main implementation for the <c>UnlockUser</c> method of the provider. Please
        /// see the corresponding method in the Facade, which calls this method, for full documentaion. 
        /// </remarks>
        /// <param name="userName">The userName to unlock.</param>
        /// <returns>Whether the membership user was successfully unlocked.</returns>
        internal bool UnlockUser(string userName)
        {
            // Che
            ValidationUtil.CheckParameterIsOK(ref userName, true, true, true, 256, "userName");

            Guid userId;

            bool isValidUser = this.GetUserIdByName(userName, out userId);

            if (!isValidUser)
            {
                throw new ProviderException(Messages.YouCanNotUnlockAUserWithoutSuplyingAValidUserName);
            }

            string sql = @"
                            UPDATE aspnet_Membership
                            SET IsLockedOut                     = '0',
                                FailPwdAttemptCount             = 0,
                                FailPwdAttemptWindowStart       = ?,
                                FailPwdAnswerAttemptCount       = 0,
                                FailPwdAnswerAttemptWindowStart = ?,
                                LastLockoutDate                 = ?
                            WHERE 
                                UserId = ?
                           ";

            // Create a new Ingres command.
            IngresCommand cmd = new IngresCommand(sql, this.conn);

            cmd.Transaction    = this.tran;
            cmd.CommandTimeout = this.config.CommandTimeout;

            // Add the required parameters.
            cmd.Parameters.Add("FailPwdAttemptWindowStart",       DbType.DateTime).Value = DateTime.Now.ToString();
            cmd.Parameters.Add("FailPwdAnswerAttemptWindowStart", DbType.DateTime).Value = DateTime.Now.ToString();
            cmd.Parameters.Add("LastLockoutDate",                 DbType.DateTime).Value = DateTime.Now.ToString();
            cmd.Parameters.Add("UserId",                          DbType.String).Value   = userId.ToString();

            // Execute the query.
            int rows = cmd.ExecuteNonQuery();

            // Return whether the user was unlocked.
            return rows > 0;
        }

        #endregion

        #region GetUserNameByEmail

        /// <summary>
        /// Takes, as input, an ex-mail address and returns the first user name from the data source 
        /// where the ex-mail address matches the supplied email parameter value.
        /// </summary>
        /// <remarks>
        /// This is the main implementation for the <c>GetUserNameByEmail</c> method of the provider. Please
        /// see the corresponding method in the Facade, which calls this method, for full documentaion. 
        /// </remarks>
        /// <param name="email">The email to get the username for.</param>
        /// <returns>The first user name from the data source where the ex-mail address matches the 
        /// supplied email parameter value.</returns>
        internal string GetUserNameByEmail(string email)
        {
            ValidationUtil.CheckParameterIsOK(ref email, false, false, false, 256, "email");

            string sql;

            IngresCommand cmd;

            if (email == null)
            {
                // Build up the required SQL
                sql = @"
                        SELECT  
                            aspnet_Users.UserName
                        FROM    
                             aspnet_Applications, 
                             aspnet_Users, 
                             aspnet_Membership
                        WHERE   
                            aspnet_Applications.LoweredApplicationName = ?
                        AND aspnet_Users.ApplicationId                 = aspnet_Applications.ApplicationId    
                        AND aspnet_Users.UserId                        = aspnet_Membership.UserId 
                        AND aspnet_Membership.LoweredEmail IS NULL
                       ";

                // Create and Ingres command.
                cmd = new IngresCommand();

                cmd.Transaction    = this.tran;
                cmd.Connection     = this.conn;
                cmd.CommandText    = sql;
                cmd.CommandTimeout = this.config.CommandTimeout;

                // Add the required parameters.
                cmd.Parameters.Add("LoweredApplicationName", DbType.String).Value = this.config.ApplicationName.ToLower();
            }
            else
            {
                // Build up the required SQL.
                sql = @"
                        SELECT  
                            aspnet_Users.UserName
                        FROM
                            aspnet_Applications, 
                            aspnet_Users, 
                            aspnet_Membership
                        WHERE   
                            aspnet_Applications.LoweredApplicationName = ?
                        AND aspnet_Users.ApplicationId                 = aspnet_Applications.ApplicationId    
                        AND aspnet_Users.UserId                        = aspnet_Membership.UserId 
                        AND aspnet_Membership.LoweredEmail             = ?
                       ";

                // Create a new Ingres command.
                cmd = new IngresCommand();

                cmd.Transaction    = this.tran;
                cmd.Connection     = this.conn;
                cmd.CommandText    = sql;
                cmd.CommandTimeout = this.config.CommandTimeout;

                // Add the required parameters.
                cmd.Parameters.Add("LoweredApplicationName", DbType.String).Value = this.config.ApplicationName.ToLower();
                cmd.Parameters.Add("LoweredEmail", DbType.String).Value           = email.ToLower();
            }

            string username = string.Empty;

            object obj = cmd.ExecuteScalar();

            if (obj != null)
            {
                username = Convert.ToString(obj);
            }

            if (username == null)
            {
                username = string.Empty;
            }

            return username;
        }

        #endregion

        #region ResetPassword

        /// <summary>
        /// Takes, as input, a user name and a password answer and generates a new, random password 
        /// for the specified user. The <c>ResetPassword</c> method updates the user information in the 
        /// data source with the new password value and returns the new password as a string. A 
        /// convenient mechanism for generating a random password is the <c>GeneratePassword</c> method of 
        /// the <c>Membership</c> class.
        /// </summary>
        /// <remarks>
        /// This is the main implementation for the <c>ResetPassword</c> method of the provider. Please
        /// see the corresponding method in the Facade, which calls this method, for full documentaion. 
        /// </remarks>
        /// <param name="username">The username to reset the password for.</param>
        /// <param name="answer">The answer to the users password question.</param>
        /// <returns>Whether the membership user was successfully updated.</returns>
        internal string ResetPassword(string username, string answer)
        {
            MembershipPasswordFormat passwordFormat;
            string salt;

            // Ensure that password reset is enabled.
            if (!this.config.EnablePasswordReset)
            {
                throw new NotSupportedException(Messages.PasswordResetIsNotEnabled);
            }

            // Ensure that an answer has been supplied if appropriate.
            if (answer == null && this.config.RequiresQuestionAndAnswer)
            {
                this.UpdateFailureCount(username, FailureReason.PasswordAnswer);

                // If we got here then commit the transaction and then handle the exception
                this.tran.Commit();

                // Set the transaction to null so that we don't attempt to roll it back.
                this.tran.Dispose();

                throw new ProviderException(Messages.PasswordAnswerRequiredForPasswordReset);
            }

            // Generate a new password
            string newPassword = Membership.GeneratePassword(this.config.NewPasswordLength, this.config.MinRequiredNonAlphanumericCharacters);

            // Raise the 'on validating password' event.
            ValidatePasswordEventArgs args = new ValidatePasswordEventArgs(username, newPassword, true);

            this.provider.OnValidatingPassword(args);

            if (args.Cancel)
            {
                if (args.FailureInformation != null)
                {
                    throw args.FailureInformation;
                }

                throw new MembershipPasswordException(Messages.ResetPasswordCanceledDueToPasswordValidationFailure);
            }

            // Build up the required SQL.
            string sql = @"
                            SELECT 
                                PasswordAnswer,
                                PasswordFormat,
                                PasswordSalt, 
                                IsLockedOut 
                            FROM 
                                aspnet_Membership,
                                aspnet_Users,
                                aspnet_Applications
                            WHERE 
                                aspnet_Users.LoweredUsername               = ?
                            AND aspnet_Applications.LoweredApplicationName = ?
                            AND aspnet_Applications.ApplicationId = aspnet_Users.ApplicationId
                            AND aspnet_Applications.ApplicationId = aspnet_Membership.ApplicationId
                            AND aspnet_Membership.UserId = aspnet_Users.UserId
                           ";

            // Create a new Ingres command
            IngresCommand cmd = new IngresCommand(sql, this.conn);

            cmd.Transaction = this.tran;
            cmd.CommandTimeout = this.config.CommandTimeout;

            // Add the required parameters.
            cmd.Parameters.Add("LoweredUsername", DbType.String).Value = username.ToLower();
            cmd.Parameters.Add("ApplicationName", DbType.String).Value = this.config.ApplicationName.ToLower();

            // Execute the SQL as a reader.
            IngresDataReader reader = cmd.ExecuteReader(CommandBehavior.SingleRow);

            string passwordAnswer;

            if (reader.HasRows)
            {
                reader.Read();

                // Ensure that the user is not locked out.
                if (DBUtil.ColValAsString(reader, "IsLockedOut") == "1")
                {
                    throw new MembershipPasswordException(Messages.TheSuppliedUserIsLockedOut);
                }

                // Get the password answer and ensure that it is lowercased
                passwordAnswer = DBUtil.ColValAsString(reader, "PasswordAnswer");

                // Determine the password format.
                int tempPasswordFormat = DBUtil.ColValAsInt32(reader, "PasswordFormat");

                switch (tempPasswordFormat)
                {
                    case 0:
                        passwordFormat = MembershipPasswordFormat.Clear;
                        break;
                    case 1:
                        passwordFormat = MembershipPasswordFormat.Hashed;
                        break;
                    case 2:
                        passwordFormat = MembershipPasswordFormat.Encrypted;
                        break;
                    default:
                        throw new ProviderException(Messages.PasswordIsStoredInAUnrecognisedFormat);
                }

                // Get the salt.
                salt = DBUtil.ColValAsString(reader, "PasswordSalt");
            }
            else
            {
                throw new MembershipPasswordException(Messages.TheSuppliedUserNameIsNotFound);
            }

            if (answer == null)
            {
                answer = string.Empty;
            }

            reader.Close();

            // Check that the correct password answer was supplied...
            if (this.config.RequiresQuestionAndAnswer && !this.CheckPassword(answer.ToLower(CultureInfo.InvariantCulture), passwordAnswer, passwordFormat, salt))
            {
                // ... if not, update the failure count and throw an exception
                this.UpdateFailureCount(username, FailureReason.PasswordAnswer);

                // If we got here then commit the transaction and then handle the exception
                this.tran.Commit();

                // Set the transaction to null so that we don't attempt to roll it back.
                this.tran.Dispose();

                throw new MembershipPasswordException(Messages.IncorrectPasswordAnswer);
            }

            reader.Close();

            // Build up the SQL to reset the password
            sql = @"
                    UPDATE aspnet_Membership
                    FROM
                        aspnet_Users,
                        aspnet_Applications
                    SET Password                = ?,
                        LastPasswordChangedDate = ?
                    WHERE 
                        aspnet_Users.LoweredUsername               = ?
                    AND aspnet_Applications.LoweredApplicationName = ?
                    AND IsLockedOut                                = 0
                    AND aspnet_Membership.UserId                   = aspnet_Users.UserId
                    AND aspnet_Applications.ApplicationId          = aspnet_Membership.ApplicationId 
                   ";

            // Create a new Ingres command
            IngresCommand updateCmd = new IngresCommand(sql, this.conn);

            updateCmd.Transaction    = this.tran;
            updateCmd.CommandTimeout = this.config.CommandTimeout;

            // Add the required parameters.
            updateCmd.Parameters.Add("Password", DbType.String).Value                  = this.EncodePassword(newPassword, passwordFormat, PasswordUtil.GetSalt(16));
            updateCmd.Parameters.Add("LastPasswordChangedDate", DbType.DateTime).Value = DateTime.Now;
            updateCmd.Parameters.Add("LoweredUsername", DbType.String).Value           = username.ToLower();
            updateCmd.Parameters.Add("LoweredApplicationName", DbType.String).Value    = this.config.ApplicationName.ToLower();
            
            // Execute the query and ensure that a row was updated.
            int rowsAffected = updateCmd.ExecuteNonQuery();

            if (rowsAffected > 0)
            {
                return newPassword;
            }

            throw new MembershipPasswordException(Messages.PasswordNotReset);
        }

        #endregion

        #region Update

        /// <summary>
        /// Takes, as input, a <c>MembershipUser</c> object populated with user information and updates 
        /// the data source with the supplied values.
        /// </summary>
        /// <remarks>
        /// This is the main implementation for the <c>Update</c> method of the provider. Please
        /// see the corresponding method in the Facade, which calls this method, for full documentaion. 
        /// </remarks>
        /// <param name="user">The membership user to update.</param>
        internal void UpdateUser(MembershipUser user)
        {
            if (user == null)
            {
                throw new ArgumentNullException("user");
            }

            // Instatiate a string to hold the required sql.
            string applicationId;
            string userId;

            string sql = @"
                            SELECT  
                                aspnet_Users.UserId, 
                                aspnet_Applications.ApplicationId
                            FROM
                                aspnet_Users,
                                aspnet_Applications,
                                aspnet_Membership
                            WHERE
                                LoweredUserName                            = ?
                            AND aspnet_Users.ApplicationId                 = aspnet_Applications.ApplicationId  
                            AND aspnet_Applications.LoweredApplicationName = ?
                            AND aspnet_Users.UserId                        = aspnet_Membership.UserId
                           ";

            // Instantiate a new Ingres command using the new connection and sql and add required parameters. 
            IngresCommand cmd = new IngresCommand(sql, this.conn);

            cmd.Transaction    = this.tran;
            cmd.CommandTimeout = this.config.CommandTimeout;

            cmd.Parameters.Add("LoweredUserName", DbType.String).Value = user.UserName.ToLower();
            cmd.Parameters.Add("LoweredApplicationName", DbType.String).Value = this.config.ApplicationName.ToLower();

            // Execute the reader and retrieve the user and application ids
            IngresDataReader reader = cmd.ExecuteReader();

            if (reader.HasRows)
            {
                reader.Read();

                userId        = DBUtil.ColValAsString(reader, "UserId");
                applicationId = DBUtil.ColValAsString(reader, "ApplicationId");

                reader.Close();
            }
            else
            {
                throw new ProviderException("Could not find user.");
            }

            if (userId == null)
            {
                throw new ProviderException("Could not find user.");
            }

            // If we require a unique email then check that the email is unique.
            if (this.config.RequiresUniqueEmail)
            {
                sql = @"SELECT
                            COUNT(*)
                        FROM  
                            aspnet_Membership
                        WHERE 
                            ApplicationId = ?  
                        AND UserId       <> ?
                        AND LoweredEmail  = ?";

                // Get a new Ingres command using the same connection and transaction
                cmd = new IngresCommand(sql, this.conn);
                cmd.Transaction = this.tran;
                cmd.CommandTimeout = this.config.CommandTimeout;

                // Add parameters
                cmd.Parameters.Add("ApplicationId", DbType.String).Value = applicationId;
                cmd.Parameters.Add("UserId", DbType.String).Value = user.ProviderUserKey;
                cmd.Parameters.Add("LoweredEmail", DbType.String).Value = user.Email.ToLower();

                // Ensure that we don't get anything returned - throw an exception if we do.
                int records = (int)cmd.ExecuteScalar();

                if (records != 0)
                {
                    throw new ProviderException(Messages.UniqueEmailRequired);
                }
            }

            // Update the last activity date
            sql = @"UPDATE aspnet_Users
                    SET
                        LastActivityDate = ?
                    WHERE
                        UserId = ?";

            // Get a new Ingres command using the same connection and transaction
            cmd = new IngresCommand(sql, this.conn);

            cmd.Transaction    = this.tran;
            cmd.CommandTimeout = this.config.CommandTimeout;

            // Add required parameters.
            cmd.Parameters.Add("LastActivityDate", DbType.DateTime).Value = user.LastActivityDate;
            cmd.Parameters.Add("UserId", DbType.String).Value = userId;

            // Execute the query.
            int rows = cmd.ExecuteNonQuery();

            if (rows != 1)
            {
                throw new ProviderException(Messages.FailedToUpdateTheLastActivityDate);
            }

            // Update the memebership details
            sql = @"UPDATE aspnet_Membership
                    SET
                        Email            = ?,
                        LoweredEmail     = ?,
                        Comment          = ?,
                        IsApproved       = ?,
                        LastLoginDate    = ?
                    WHERE
                        UserId           =?";

            // Get a new Ingres command using the same connection and transaction
            cmd = new IngresCommand(sql, this.conn);

            cmd.Transaction    = this.tran;
            cmd.CommandTimeout = this.config.CommandTimeout;

            // Add the parameters
            cmd.Parameters.Add("Email", DbType.String).Value         = user.Email;
            cmd.Parameters.Add("LoweredEmail", DbType.String).Value  = user.Email.ToLower();
            cmd.Parameters.Add("Comment", DbType.String).Value       = user.Comment;
            cmd.Parameters.Add("IsApproved", DbType.String).Value    = user.IsApproved;
            cmd.Parameters.Add("LastLoginDate", DbType.String).Value = user.LastLoginDate;
            cmd.Parameters.Add("UserId", DbType.String).Value        = userId;

            rows = cmd.ExecuteNonQuery();

            if (rows != 1)
            {
                throw new ProviderException(Messages.FailedToUpdateMembershipDetails);
            }
        }

        #endregion

        #region ValidateUser

        /// <summary>
        /// Takes, as input, a user name and a password and verifies that the values match those in 
        /// the data source. The <c>ValidateUser</c> method returns true for a successful user name and 
        /// password match; otherwise, false.
        /// </summary>
        /// <remarks>
        /// This is the main implementation for the <c>ValidateUser</c> method of the provider. Please
        /// see the corresponding method in the Facade, which calls this method, for full documentaion. 
        /// </remarks>
        /// <param name="username">The username of the user we wish to validate.</param>
        /// <param name="password">The users password.</param>
        /// <returns>Whether the user was successfully validated.</returns>
        internal bool ValidateUser(string username, string password)
        {
            ValidationUtil.CheckParameterIsOK(ref username, true, true, true, 256, "username");
            ValidationUtil.CheckParameterIsOK(ref password, true, true, false, 128, "password");

            return this.CheckPassword(username, password, true, true);
        }

        #endregion

        #region FindUsersByName

        /// <summary>
        /// Returns a list of membership users where the user name contains a match of the supplied 
        /// <c>usernameToMatch</c> for the this.configured <c>ApplicationName</c>. For example, if the <c>usernameToMatch</c> 
        /// parameter is set to "user," then the users "user1," "user2," "user3," and so on are 
        /// returned. Wildcard support is included based on the data source. Users are returned in 
        /// alphabetical order by user name.
        /// </summary>
        /// <remarks>
        /// This is the main implementation for the <c>FindUsersByName</c> method of the provider. Please
        /// see the corresponding method in the Facade, which calls this method, for full documentaion. 
        /// </remarks>
        /// <param name="usernameToMatch">The username to match.</param>
        /// <param name="pageIndex">The page to return.</param>
        /// <param name="pageSize">The number of users to return.</param>
        /// <param name="totalRecords">[out] the total number of matches.</param>
        /// <returns>Returns a list of membership users where the user name contains a match of the 
        /// supplied usernameToMatch for the this.configured ApplicationName.</returns>
        internal MembershipUserCollection FindUsersByName(string usernameToMatch, int pageIndex, int pageSize, out int totalRecords)
        {
            // Validate the input
            ValidationUtil.CheckParameterIsOK(ref usernameToMatch, true, true, false, 256, "usernameToMatch");

            if (pageIndex < 0)
            {
                throw new ArgumentException(string.Format(Messages.PageIndexInvalid), "pageIndex");
            }

            if (pageSize < 1)
            {
                throw new ArgumentException(string.Format(Messages.PageSizeInvalid), "pageSize");
            }

            long upperBound = ((long)pageIndex * pageSize) + (pageSize - 1);

            if (upperBound > Int32.MaxValue)
            {
                throw new ArgumentException(string.Format(Messages.PageIndexAndPageSizeCombinationInvalid), "pageIndex and pageSize");
            }

            // Adjust the username so that it is in the correct format for an Ingres "LIKE" 
            usernameToMatch = String.Format("%{0}%", usernameToMatch);

            string sql = @"
                            SELECT 
                                COUNT(*) 
                            FROM 
                                aspnet_Membership,
                                aspnet_Applications,
                                aspnet_Users
                            WHERE 
                                aspnet_Users.LoweredUsername LIKE ?
                            AND aspnet_Applications.LoweredApplicationName = ?
                            AND aspnet_Applications.ApplicationId   = aspnet_Membership.ApplicationId
                            AND aspnet_Users.UserId                 = aspnet_Membership.UserId 
                           ";

            IngresCommand cmd = new IngresCommand(sql, this.conn);
            cmd.Connection = this.conn;
            cmd.Transaction = this.tran;
            cmd.CommandTimeout = this.config.CommandTimeout;

            cmd.Parameters.Add("LoweredUsername", DbType.String).Value = usernameToMatch.ToLower();
            cmd.Parameters.Add("ApplicationName", DbType.String).Value = this.config.ApplicationName.ToLower();

            MembershipUserCollection users = new MembershipUserCollection();

            totalRecords = Convert.ToInt32(cmd.ExecuteScalar());

            if (totalRecords <= 0)
            {
                return users;
            }

            // Create a new Ingres command
            cmd = new IngresCommand();

            cmd.Connection     = this.conn;
            cmd.Transaction    = this.tran;
            cmd.CommandTimeout = this.config.CommandTimeout;

            // Add the required parameters
            cmd.Parameters.Add("LoweredUsername", DbType.String).Value = usernameToMatch.ToLower();
            cmd.Parameters.Add("ApplicationName", DbType.String).Value = this.config.ApplicationName.ToLower();

            // Build up the SQL
            cmd.CommandText = @"
                                SELECT 
                                    aspnet_Membership.Email, 
                                    aspnet_Membership.PasswordQuestion, 
                                    aspnet_Membership.Comment, 
                                    aspnet_Membership.IsApproved,
                                    aspnet_Membership.CreateDate, 
                                    aspnet_Membership.LastLoginDate, 
                                    aspnet_Membership.LastPasswordChangedDate,
                                    aspnet_Users.UserId,
                                    aspnet_Users.UserName, 
                                    aspnet_Membership.IsLockedOut,
                                    aspnet_Membership.LastLockoutDate,
                                    aspnet_Users.LastActivityDate
                                FROM 
                                    aspnet_Users, 
                                    aspnet_Membership,
                                    aspnet_Applications
                                WHERE 
                                    aspnet_Users.LoweredUsername LIKE ?
                                AND aspnet_Users.UserId                        = aspnet_Membership.UserId
                                AND aspnet_Applications.LoweredApplicationName = ?
                                AND aspnet_Applications.ApplicationId          = aspnet_Membership.ApplicationId
                                ORDER BY 
                                    Username ASC
                               ";

            // Execute the query and return the correct subset as stipulated by the paging options.
            IngresDataReader reader = cmd.ExecuteReader();

            int counter    = 0;
            int startIndex = (pageSize * pageIndex) + 1;
            int endIndex   = pageSize * (pageIndex + 1);

            while (reader.Read())
            {
                counter++;

                if ((counter >= startIndex) && (counter <= endIndex))
                {
                    MembershipUser user = this.GetUserFromReader(reader);
                    users.Add(user);
                }

                if (counter >= endIndex)
                {
                    cmd.Cancel();
                }
            }

            return users;
        }

        #endregion

        #region FindUsersByEmail

        /// <summary>
        /// Returns a list of membership users where the user name contains a match of the supplied 
        /// emailToMatch for the this.configured <c>ApplicationName</c>. For example, if the <c>emailToMatch</c> 
        /// parameter is set to "address@example.com," then users with the ex-mail addresses 
        /// "address1@example.com," "address2@example.com," and so on are returned. Wildcard support 
        /// is included based on the data source. Users are returned in alphabetical order by user 
        /// name.
        /// </summary>
        /// <remarks>
        /// This is the main implementation for the <c>FindUsersByEmail</c> method of the provider. Please
        /// see the corresponding method in the Facade, which calls this method, for full documentaion. 
        /// </remarks>
        /// <param name="emailToMatch">The email to match.</param>
        /// <param name="pageIndex">The page number to return.</param>
        /// <param name="pageSize">The number of users per page.</param>
        /// <param name="totalRecords">[out] the number of matched users.</param>
        /// <returns>Returns a list of membership users where the user name contains a match of the 
        /// supplied emailToMatch for the this.configured ApplicationName.</returns>
        internal MembershipUserCollection FindUsersByEmail(string emailToMatch, int pageIndex, int pageSize, out int totalRecords)
        {
            // Validate the input.
            ValidationUtil.CheckParameterIsOK(ref emailToMatch, false, false, false, 256, "emailToMatch");

            if (pageIndex < 0)
            {
                throw new ArgumentException(string.Format(Messages.PageIndexInvalid), "pageIndex");
            }

            if (pageSize < 1)
            {
                throw new ArgumentException(string.Format(Messages.PageSizeInvalid), "pageSize");
            }

            long upperBound = ((long)pageIndex * pageSize) + (pageSize - 1);

            if (upperBound > Int32.MaxValue)
            {
                throw new ArgumentException(string.Format(Messages.PageIndexAndPageSizeCombinationInvalid), "pageIndex and pageSize");
            }

            // Adjust the email so that it is in the correct format for an Ingres "LIKE" 
            emailToMatch = String.Format("%{0}%", emailToMatch);

            string sql = @"
                            SELECT 
                                Count(*) 
                            FROM 
                                aspnet_Membership,
                                aspnet_Applications,
                                aspnet_Users
                            WHERE 
                                aspnet_Membership.LoweredEmail LIKE ?
                            AND aspnet_Users.UserId                        = aspnet_Membership.UserId
                            AND aspnet_Applications.LoweredApplicationName = ?
                            AND aspnet_Applications.ApplicationId          = aspnet_Membership.ApplicationId
                           ";

            // Create a new Ingres command.
            IngresCommand cmd = new IngresCommand(sql, this.conn);

            cmd.Transaction    = this.tran;
            cmd.CommandTimeout = this.config.CommandTimeout;

            // Add the required parameters.
            cmd.Parameters.Add("LoweredEmail", DbType.String).Value           = emailToMatch.ToLower();
            cmd.Parameters.Add("LoweredApplicationName", DbType.String).Value = this.config.ApplicationName.ToLower();

            MembershipUserCollection users = new MembershipUserCollection();

            // Execute the command and return if we don't have any users
            totalRecords = Convert.ToInt32(cmd.ExecuteScalar());

            if (totalRecords <= 0)
            {
                return users;
            }

            // "reset" the command
            cmd = new IngresCommand();

            cmd.Connection     = this.conn;
            cmd.Transaction    = this.tran;
            cmd.CommandTimeout = this.config.CommandTimeout;

            // Add the required parameters
            cmd.Parameters.Add("LoweredEmail", DbType.String).Value           = emailToMatch.ToLower();
            cmd.Parameters.Add("LoweredApplicationName", DbType.String).Value = this.config.ApplicationName.ToLower();

            // Build up the required SQL
            cmd.CommandText = @"
                                SELECT 
                                    aspnet_Membership.Email, 
                                    aspnet_Membership.PasswordQuestion, 
                                    aspnet_Membership.Comment, 
                                    aspnet_Membership.IsApproved,
                                    aspnet_Membership.CreateDate, 
                                    aspnet_Membership.LastLoginDate, 
                                    aspnet_Membership.LastPasswordChangedDate,
                                    aspnet_Users.UserId,
                                    aspnet_Users.UserName, 
                                    aspnet_Membership.IsLockedOut,
                                    aspnet_Membership.LastLockoutDate,
                                    aspnet_Users.LastActivityDate
                                FROM 
                                    aspnet_Users, 
                                    aspnet_Membership,
                                    aspnet_Applications
                                WHERE 
                                    aspnet_Membership.LoweredEmail LIKE ?
                                AND aspnet_Users.UserId                        = aspnet_Membership.UserId
                                AND aspnet_Applications.LoweredApplicationName = ?
                                AND aspnet_Applications.ApplicationId          = aspnet_Membership.ApplicationId
                                ORDER BY 
                                    Username ASC
                               ";

            // Execute the query.
            IngresDataReader reader = cmd.ExecuteReader();

            // Return the appropriate users for the paging options given
            int counter = 0;

            int startIndex = (pageSize * pageIndex) + 1;

            int endIndex = pageSize * (pageIndex + 1);

            while (reader.Read())
            {
                counter++;

                if ((counter >= startIndex) && (counter <= endIndex))
                {
                    MembershipUser user = this.GetUserFromReader(reader);
                    users.Add(user);
                }

                if (counter >= endIndex)
                {
                    cmd.Cancel();

                    break;
                }
            }

            return users;
        }

        #endregion

        #endregion

        #region Helper Methods

        #region GetUserFromReader

        /// <summary>
        /// Helper method to retrieve a membership user from a data reader. An IngresDataReader is 
        /// passed in and a MembershipUser is returned.
        /// </summary>
        /// <param name="reader">The Ingres Data Reader that we wish to get the user from.</param>
        /// <returns>The user from the reader.</returns>
        private MembershipUser GetUserFromReader(IngresDataReader reader)
        {
            string email                     = DBUtil.ColValAsString(reader,   "Email");
            string passwordQuestion          = DBUtil.ColValAsString(reader,   "PasswordQuestion");
            string comment                   = DBUtil.ColValAsString(reader,   "Comment");
            bool isApproved                  = DBUtil.ColValAsString(reader,   "IsApproved") == "1" ? true : false;
            DateTime creationDate            = DBUtil.ColValAsDateTime(reader, "CreateDate");
            DateTime lastLoginDate           = DBUtil.ColValAsDateTime(reader, "LastLoginDate");
            DateTime lastPasswordChangedDate = DBUtil.ColValAsDateTime(reader, "LastPasswordChangedDate");
            object providerUserKey           = DBUtil.ColValAsString(reader,   "UserId");
            string username                  = DBUtil.ColValAsString(reader,   "UserName");
            bool isLockedOut                 = DBUtil.ColValAsString(reader,   "IsLockedOut") == "1" ? true : false;
            DateTime lastLockedOutDate       = DBUtil.ColValAsDateTime(reader, "LastLockoutDate");
            DateTime lastActivityDate        = DBUtil.ColValAsDateTime(reader, "LastActivityDate");

            // Construct and return the user
            MembershipUser user = new MembershipUser(
                                                        this.config.ProviderName,
                                                        username,
                                                        providerUserKey,
                                                        email,
                                                        passwordQuestion,
                                                        comment,
                                                        isApproved,
                                                        isLockedOut,
                                                        creationDate,
                                                        lastLoginDate,
                                                        lastActivityDate,
                                                        lastPasswordChangedDate,
                                                        lastLockedOutDate);

            return user;
        }

        #endregion

        #region UpdateFailureCount

        /// <summary>
        /// A helper method that performs the checks and updates associated with password failure 
        /// tracking.
        /// </summary>
        /// <param name="username">The username to update the failure count for.</param>
        /// <param name="reason">The reason why we wish to update the failure count.</param>
        private void UpdateFailureCount(string username, FailureReason reason)
        {
            // The required SQL
            string sql = @"
                            SELECT 
                                FailPwdAttemptCount,
                                FailPwdAttemptWindowStart,
                                FailPwdAnswerAttemptCount,
                                FailPwdAnswerAttemptWindowStart
                            FROM 
                                aspnet_Membership,
                                aspnet_Users,
                                aspnet_Applications
                            WHERE 
                                aspnet_Users.LoweredUsername               = ?
                            AND aspnet_Applications.LoweredApplicationName = ?
                            AND aspnet_Users.UserId                        = aspnet_Membership.UserId
                            AND aspnet_Membership.ApplicationId            = aspnet_Applications.ApplicationId
                           ";

            // Instantiate an Ingres command (using the current connection and transaction) with the SQL
            IngresCommand cmd = new IngresCommand(sql, this.conn);
            cmd.Transaction = this.tran;
            cmd.CommandTimeout = this.config.CommandTimeout;

            // Add the required parameters
            cmd.Parameters.Add("LoweredUsername", DbType.String).Value = username.ToLower();
            cmd.Parameters.Add("LoweredApplicationName", DbType.String).Value = this.config.ApplicationName.ToLower();

            DateTime windowStart = new DateTime();
            int failureCount     = 0;

            // Execute the command as a reader
            IngresDataReader reader = cmd.ExecuteReader(CommandBehavior.SingleRow);

            if (reader.HasRows)
            {
                reader.Read();

                if (reason == FailureReason.Password)
                {
                    failureCount = DBUtil.ColValAsInt32(reader, "FailPwdAttemptCount");

                    try
                    {
                        windowStart = DBUtil.ColValAsDateTime(reader, "FailPwdAttemptWindowStart");
                    }
                    catch
                    {
                        windowStart = DateTime.Now;
                    }
                }

                if (reason == FailureReason.PasswordAnswer)
                {
                    failureCount = DBUtil.ColValAsInt32(reader,    "FailPwdAnswerAttemptCount");
                    windowStart  = DBUtil.ColValAsDateTime(reader, "FailPwdAnswerAttemptWindowStart");
                }
            }

            // Close the reader and build up a new command
            reader.Close();

            cmd = new IngresCommand();
            cmd.Connection = this.conn;
            cmd.Transaction = this.tran;
            cmd.CommandTimeout = this.config.CommandTimeout;

            DateTime windowEnd = windowStart.AddMinutes(this.config.PasswordAttemptWindow);

            if (failureCount == 0 || DateTime.Now > windowEnd)
            {
                // Start a new failure count and window
                if (reason == FailureReason.Password)
                {
                    // For password failed attempts:
                    cmd.CommandText = @"
                                        UPDATE aspnet_Membership
                                        FROM
                                            aspnet_Applications,
                                            aspnet_Users
                                        SET FailPwdAttemptCount       = ?,
                                            FailPwdAttemptWindowStart = ?
                                        WHERE 
                                            LoweredUsername                   = ? 
                                        AND LoweredApplicationName            = ?
                                        AND aspnet_Applications.ApplicationId = aspnet_Membership.ApplicationId
                                        AND aspnet_Membership.UserId          = aspnet_Users.UserId
                                       ";
                }

                if (reason == FailureReason.PasswordAnswer)
                {
                    // For password answer failed attempts:
                    cmd.CommandText = @"
                                        UPDATE aspnet_Membership
                                        FROM
                                            aspnet_Applications,
                                            aspnet_Users
                                        SET FailPwdAnswerAttemptCount       = ?,
                                            FailPwdAnswerAttemptWindowStart = ?
                                        WHERE 
                                            aspnet_Users.LoweredUsername               = ? 
                                        AND aspnet_Applications.LoweredApplicationName = ?
                                        AND aspnet_Applications.ApplicationId          = aspnet_Membership.ApplicationId
                                        AND aspnet_Membership.UserId                   = aspnet_Users.UserId
                                       ";
                }

                // Ad the required parameters
                cmd.Parameters.Add("Count",                  DbType.Int32).Value    = 1;
                cmd.Parameters.Add("WindowStart",            DbType.DateTime).Value = DateTime.Now;
                cmd.Parameters.Add("LoweredUsername",        DbType.String).Value   = username.ToLower();
                cmd.Parameters.Add("LoweredApplicationName", DbType.String).Value   = this.config.ApplicationName.ToLower();

                // Execute the query and ensure that one row was updated
                if (cmd.ExecuteNonQuery() != 1)
                {
                    throw new ProviderException(Messages.UnableToUpdateFailureCountAndWindowStart);
                }
            }
            else 
            {   // Not the first failure or still inside the window

                // If the failure accout is now greater than or equal to the max attempts allowed
                // then lock the user out
                if (failureCount++ >= this.config.MaxInvalidPasswordAttempts)
                {
                    // Password attempts have exceeded the failure threshold. Lock out
                    // the user.
                    cmd.CommandText = @"
                                        UPDATE aspnet_Membership
                                        FROM
                                            aspnet_Applications,
                                            aspnet_Users
                                        SET IsLockedOut       = ?, 
                                            LastLockoutDate   = ?
                                        WHERE 
                                            aspnet_Users.LoweredUsername               = ?
                                        AND aspnet_Applications.LoweredApplicationName = ?
                                        AND aspnet_Applications.ApplicationId          = aspnet_Membership.ApplicationId
                                        AND aspnet_Membership.UserId                   = aspnet_Users.UserId 
                                       ";

                    cmd.Parameters.Clear();

                    cmd.Parameters.Add("IsLockedOut",            DbType.String).Value   = '1';
                    cmd.Parameters.Add("LastLockoutDate",        DbType.DateTime).Value = DateTime.Now;
                    cmd.Parameters.Add("LoweredUsername",        DbType.String).Value   = username.ToLower();
                    cmd.Parameters.Add("LoweredApplicationName", DbType.String).Value   = this.config.ApplicationName.ToLower();

                    if (cmd.ExecuteNonQuery() < 0)
                    {
                        throw new ProviderException("Unable to lock out user.");
                    }
                }
                else
                {
                    // Otherwise just save the incremented failure count.
                    if (reason == FailureReason.Password)
                    {
                        cmd.CommandText = @"
                                            UPDATE aspnet_Membership
                                            FROM
                                                aspnet_Applications,
                                                aspnet_Users
                                            SET FailPwdAttemptCount = ?
                                            WHERE 
                                                aspnet_Users.LoweredUsername               = ?
                                            AND aspnet_Applications.LoweredApplicationName = ?
                                            AND aspnet_Applications.ApplicationId          = aspnet_Membership.ApplicationId
                                            AND aspnet_Membership.UserId                   = aspnet_Users.UserId 
                                           ";
                    }

                    if (reason == FailureReason.PasswordAnswer)
                    {
                        cmd.CommandText = @"
                                            UPDATE aspnet_Membership
                                            FROM
                                                aspnet_Applications,
                                                aspnet_Users
                                            SET FailPwdAnswerAttemptCount = ?
                                            WHERE 
                                                aspnet_Users.LoweredUsername               = ?
                                            AND aspnet_Applications.LoweredApplicationName = ?
                                            AND aspnet_Applications.ApplicationId          = aspnet_Membership.ApplicationId
                                            AND aspnet_Membership.UserId                   = aspnet_Users.UserId 
                                           ";
                    }

                    cmd.Parameters.Clear();

                    cmd.Parameters.Add("Count",                  DbType.Int32).Value  = failureCount;
                    cmd.Parameters.Add("LoweredUsername",        DbType.String).Value = username.ToLower();
                    cmd.Parameters.Add("LoweredApplicationName", DbType.String).Value = this.config.ApplicationName.ToLower();

                    if (cmd.ExecuteNonQuery() != 1)
                    {
                        throw new ProviderException(Messages.UnableToUpdateFailureCount);
                    }
                }
            }
        }

        #endregion

        #region CheckPassword

        /// <summary>
        /// Checks whether a supplied password for a user is valid or not.
        /// </summary>
        /// <param name="username">The username to check.</param>
        /// <param name="password">The supplied password to check.</param>
        /// <param name="updateLastLoginActivityDate">Whether to update the last login activity date.</param>
        /// <param name="failIfNotApproved">Whether the operation should fail if the user is not approved.</param>
        /// <returns>Whether the password is correct.</returns>
        private bool CheckPassword(string username, string password, bool updateLastLoginActivityDate, bool failIfNotApproved)
        {
            string salt;
            MembershipPasswordFormat passwordFormat;

            return this.CheckPassword(username, password, updateLastLoginActivityDate, failIfNotApproved, out salt, out passwordFormat);
        }

        /// <summary>
        /// Checks whether a supplied password for a user is valid or not.
        /// </summary>
        /// <param name="username">The username to check.</param>
        /// <param name="password">The supplied password to check.</param>
        /// <param name="updateLastLoginActivityDate">Whether to update the last login activity date.</param>
        /// <param name="failIfNotApproved">Whether the operation should fail if the user is not approved.</param>
        /// <param name="salt">The salt to use.</param>
        /// <param name="passwordFormat">The password format to use.</param>
        /// <returns>Whether the supplied password matches the password on the database.</returns>
        private bool CheckPassword(string username, string password, bool updateLastLoginActivityDate, bool failIfNotApproved, out string salt, out MembershipPasswordFormat passwordFormat)
        {
            // Default the 'out' parameters.
            salt = null;
            passwordFormat = MembershipPasswordFormat.Clear;

            // Assume that the user is not valid
            bool valid = false;

            // Build the required SQL string.
            string sql = @"
                            SELECT 
                                Password,
                                PasswordFormat,
                                PasswordSalt,
                                IsApproved
                            FROM 
                                aspnet_Membership,
                                aspnet_Users,
                                aspnet_Applications
                            WHERE 
                                aspnet_Users.LoweredUserName               = ?
                            AND aspnet_Applications.LoweredApplicationName = ?
                            AND IsLockedOut                                = 0
                            AND aspnet_Users.UserId = aspnet_Membership.UserId
                            AND aspnet_Applications.ApplicationId = aspnet_Users.ApplicationId
                            AND aspnet_Applications.ApplicationId = aspnet_Membership.ApplicationId
                           ";

            // Create a command and enrol in the current transaction
            IngresCommand cmd = new IngresCommand(sql);
            cmd.Connection = this.conn;
            cmd.Transaction = this.tran;
            cmd.CommandTimeout = this.config.CommandTimeout;

            // Add the required parameters.
            cmd.Parameters.Add("LoweredUserNameUsername", DbType.String).Value = username.ToLower();
            cmd.Parameters.Add("LoweredApplicationName", DbType.String).Value = this.config.ApplicationName.ToLower();

            bool approved;

            // Execute the command and determine the password and whether the user is locked out
            IngresDataReader reader = cmd.ExecuteReader(CommandBehavior.SingleRow);

            string databasePassword;

            if (reader.HasRows)
            {
                reader.Read();

                databasePassword = DBUtil.ColValAsString(reader, "Password");

                int tempPasswordFormat = DBUtil.ColValAsInt32(reader, "PasswordFormat");

                switch (tempPasswordFormat)
                {
                    case 0:
                        passwordFormat = MembershipPasswordFormat.Clear;
                        break;
                    case 1:
                        passwordFormat = MembershipPasswordFormat.Hashed;
                        break;
                    case 2:
                        passwordFormat = MembershipPasswordFormat.Encrypted;
                        break;
                    default:
                        throw new ProviderException(Messages.PasswordIsStoredInAUnrecognisedFormat);
                }

                salt = DBUtil.ColValAsString(reader, "PasswordSalt");

                approved = (DBUtil.ColValAsString(reader, "IsApproved") == "1");
            }
            else
            {
                return false;
            }

            reader.Close();

            if (this.CheckPassword(password, databasePassword, passwordFormat, salt))
            {
                if (approved)
                {
                    valid = true;

                    sql = @"
                            UPDATE aspnet_Membership
                            FROM
                                aspnet_Applications,
                                aspnet_Users 
                            SET LastLoginDate       = ?,
                                FailPwdAttemptCount = 0
                            WHERE 
                                aspnet_Users.LoweredUserName               = ?
                            AND aspnet_Applications.LoweredApplicationName = ?
                            AND aspnet_Applications.ApplicationId          = aspnet_Users.ApplicationId
                            AND aspnet_Applications.ApplicationId          = aspnet_Membership.ApplicationId
                            AND aspnet_Users.UserId                        = aspnet_Membership.UserId
                           ";

                    // Create a new command and enrol in the current transaction
                    cmd = new IngresCommand(sql, this.conn);
                    cmd.Transaction = this.tran;
                    cmd.CommandTimeout = this.config.CommandTimeout;

                    // Add the required parameters,
                    cmd.Parameters.Add("LastLoginDate", DbType.DateTime).Value = DateTime.Now;
                    cmd.Parameters.Add("Username", DbType.String).Value = username.ToLower();
                    cmd.Parameters.Add("ApplicationName", DbType.String).Value = this.config.ApplicationName.ToLower();

                    // Execute the query and ensure that only one row was updated
                    int rows = cmd.ExecuteNonQuery();

                    if (rows != 1)
                    {
                        throw new Exception(Messages.ErrorTryingToUpdateTheLastLoginDate);
                    }
                }
            }
            else
            {
                this.UpdateFailureCount(username, FailureReason.Password);
            }

            return valid;
        }

        /// <summary>
        /// Compares password values based on the MembershipPasswordFormat.
        /// </summary>
        /// <param name="password">The password supplied.</param>
        /// <param name="databasePassword">The password as stored on the database.</param>
        /// <param name="format">The password format.</param>
        /// <param name="salt">The salt to use.</param>
        /// <returns>Whether the supplied password matches the database password.</returns>
        private bool CheckPassword(string password, string databasePassword, MembershipPasswordFormat format, string salt)
        {
            return databasePassword == this.EncodePassword(password, format, salt);
        }

        #endregion

        #region DecodePassword

        /// <summary>
        /// Decrypts or leaves the password clear based on the MembershipPasswordFormat enum.
        /// </summary>
        /// <param name="encodedPassword">The encoded password.</param>
        /// <param name="usersPasswordFormat">The password format for the user.</param>
        /// <returns>The decoded password.</returns>
        private string DecodePassword(string encodedPassword, MembershipPasswordFormat usersPasswordFormat)
        {
            string decodePassword = string.Empty;

            switch (usersPasswordFormat)
            {
                case MembershipPasswordFormat.Clear:
                    // if the password isn't encrypted/hashed then we don't have to 
                    // do anything
                    break;
                case MembershipPasswordFormat.Encrypted:
                    // we can unencode an encrypted password so go ahead
                    // note: the salt is removed using the Substring(8)
                    decodePassword = Encoding.Unicode.GetString(provider.DecryptPassword(Convert.FromBase64String(encodedPassword))).Substring(8);
                    break;
                case MembershipPasswordFormat.Hashed:
                    // we can't unencode a hashed password so throw an exception
                    throw new ProviderException(Messages.CannotUnencodeAHashedPassword);
                default:
                    // just throw an exception
                    throw new ProviderException(Messages.UnsupportedPasswordFormat);
            }

            return decodePassword;
        }

        #endregion

        #region GetUserIdByName

        /// <summary>
        /// Retrieves the user Id for a given username.
        /// </summary>
        /// <param name="username">The username.</param>
        /// <param name="userId">[out] The user Id.</param>
        /// <returns>Whether the username/id exist on the database.</returns>
        private bool GetUserIdByName(string username, out Guid userId)
        {
            string userIdTemp = null;

            // Build up the required SQL
            string sql = @"
                            SELECT
                                aspnet_Users.UserId
                            FROM
                                aspnet_Users, 
                                aspnet_Applications, 
                                aspnet_Membership
                            WHERE
                                LoweredUserName                            = ? 
                            AND aspnet_Users.ApplicationId                 = aspnet_Applications.ApplicationId
                            AND aspnet_Applications.LoweredApplicationName = ? 
                            AND aspnet_Users.UserId                        = aspnet_Membership.UserId
                           ";

            // Create a new Ingres command.
            IngresCommand cmd = new IngresCommand(sql, this.conn);

            cmd.Transaction    = this.tran;
            cmd.CommandTimeout = this.config.CommandTimeout;

            // Add the required parameters.
            cmd.Parameters.Add("LoweredUserName", DbType.String).Value = username.ToLower();
            cmd.Parameters.Add("LoweredApplicationName", DbType.String).Value = this.config.ApplicationName.ToLower();

            // Execute the query
            IngresDataReader reader = cmd.ExecuteReader();

            // Retrieve the Id
            if (reader.HasRows)
            {
                reader.Read();

                userIdTemp = DBUtil.ColValAsString(reader, "UserId");
            }

            // If we didnt get an Id then the user doesn't exist.
            if (userIdTemp == null)
            {
                userId = Guid.Empty;

                return false;
            }

            if (!reader.IsClosed)
            {
                reader.Close();
            }

            // Parse the user Id as a Guid
            userId = (Guid)SqlGuid.Parse(userIdTemp);

            return true;
        }

        #endregion

        #region CreateUserForApplication

        /// <summary>
        /// Creates a new user in the asnet_Users table in the database.
        /// </summary>
        /// <param name="userName">The username for the user.</param>
        /// <param name="isUserAnonymous">Whether the user is anonymous.</param>
        /// <param name="lastActivityDate">The last activity date for the user,</param>
        /// <param name="userId">The Id for the user that we wish to create.</param>
        private void CreateUserForApplication(string userName, bool isUserAnonymous, DateTime lastActivityDate, ref object userId)
        {
            if (userId == null)
            {
                userId = Guid.NewGuid();
            }
            else
            {
                if (this.DoesUserIdExist((Guid)userId))
                {
                    throw new Exception("Error!!!!");
                }
            }

            string sql = @"
                            INSERT INTO aspnet_Users 
                               (ApplicationId, 
                                userId, 
                                userName, 
                                LoweredUserName, 
                                IsAnonymous, 
                                lastActivityDate)
                            VALUES 
                               (?, 
                                ?, 
                                ?, 
                                ?, 
                                ?, 
                                ?)
                           ";

            // Create a new command and enrol in the current transaction
            IngresCommand cmd = new IngresCommand(sql, this.conn);

            cmd.Transaction    = this.tran;
            cmd.CommandTimeout = this.config.CommandTimeout;

            // Add the required parameters
            cmd.Parameters.Add("ApplicationId", DbType.String).Value      = this.ApplicationId;
            cmd.Parameters.Add("UserId", DbType.String).Value             = userId.ToString();
            cmd.Parameters.Add("UserName", DbType.String).Value           = userName;
            cmd.Parameters.Add("LoweredUserName", DbType.String).Value    = userName.ToLower();
            cmd.Parameters.Add("IsAnonymous", DbType.String).Value        = isUserAnonymous.ToString();
            cmd.Parameters.Add("LastActivityDate", DbType.DateTime).Value = lastActivityDate;

            // Execute the command and ensure that the row was inserted
            int rows = cmd.ExecuteNonQuery();

            if (rows != 1)
            {
                throw new Exception(Messages.ErrorAttemptingToCreateAUser);
            }
        }

        #endregion

        #region DoesUserIdExist

        /// <summary>
        /// A helper function to determine if a user exists with a given id exists.
        /// </summary>
        /// <param name="userId">The Id to check.</param>
        /// <returns>Whether a user exists with the given Id.</returns>
        private bool DoesUserIdExist(Guid? userId)
        {
            string sql = @"
                            SELECT 
                                UserId 
                            FROM 
                                aspnet_Users
                            WHERE 
                                UserId = ? 
                           ";

            IngresCommand cmd = new IngresCommand(sql, this.conn);
            cmd.Transaction = this.tran;
            cmd.CommandTimeout = this.config.CommandTimeout;

            cmd.Parameters.Add("UserId", DbType.String).Value = userId.ToString();

            int rows = cmd.ExecuteNonQuery();

            return rows > 0;
        }

        #endregion

        #region GetApplicationId

        /// <summary>
        /// Gets the application Id for the current application.
        /// </summary>
        /// <param name="conn">The Ingres connection to use.</param>
        /// <param name="tran">The Ingres transaction to use.</param>
        /// <returns>The application id for the current application.</returns>
        private string GetApplicationId(IngresConnection conn, IngresTransaction tran)
        {
            string id = null;

            // Build the required SQL
            string sql = @"
                          SELECT  
                              ApplicationId 
                          FROM 
                              aspnet_Applications 
                          WHERE LoweredApplicationName = ?
                         ";

            // Create the new command and enrol in the current transaction
            IngresCommand cmd = new IngresCommand(sql, this.conn);

            cmd.Transaction    = this.tran;
            cmd.CommandTimeout = this.config.CommandTimeout;

            // Add the required parameters.
            cmd.Parameters.Add("LoweredApplicationName", DbType.String).Value = this.config.ApplicationName.ToLower();

            // Open the connection
            conn.Open();

            // Execute the command and read the id
            IngresDataReader reader = cmd.ExecuteReader();

            if (reader != null)
            {
                if (reader.HasRows)
                {
                    reader.Read();

                    id = DBUtil.ColValAsString(reader, "ApplicationId");

                    reader.Close();
                }
                else
                {
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

            IngresDataReader reader = cmd.ExecuteReader();

            if (reader != null)
            {
                if (reader.HasRows)
                {
                    reader.Read();
                    id = reader.GetString(0);

                    reader.Close();

                    return;
                }

                id = Guid.NewGuid().ToString();
            }

            if (reader != null)
            {
                reader.Close();
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

            cmd = new IngresCommand(sql, this.conn);
            cmd.Transaction = this.tran;
            cmd.CommandTimeout = this.config.CommandTimeout;

            cmd.Parameters.Add("ApplicationId", DbType.String).Value = id;
            cmd.Parameters.Add("ApplicationName", DbType.String).Value = name;
            cmd.Parameters.Add("LoweredApplicationName", DbType.String).Value = name.ToLower();

            cmd.ExecuteNonQuery();

            return;
        }
        #endregion

        #region EncodePassword

        /// <summary>
        /// Encrypts, Hashes, or leaves the password clear based on the MembershipPasswordFormat enum.
        /// </summary>
        /// <param name="password">The password that we wish to encode.</param>
        /// <param name="passwordFormat">The format of the password.</param>
        /// <param name="salt">The salt to use.</param>
        /// <returns>The encoded password.</returns>
        private string EncodePassword(string password, MembershipPasswordFormat passwordFormat, string salt)
        {
            // If the password is null then default to an empty string.
            if (password == null)
            {
                password = string.Empty;
            }

            // The following encryption/hashing is based on the SQL Server version for easy porting
            // of applications to Ingres.
            byte[] passwordBytes = Encoding.Unicode.GetBytes(password);
            byte[] saltBytes = Convert.FromBase64String(salt);
            byte[] allBytes = new byte[saltBytes.Length + passwordBytes.Length];

            Buffer.BlockCopy(saltBytes, 0, allBytes, 0, saltBytes.Length);
            Buffer.BlockCopy(passwordBytes, 0, allBytes, saltBytes.Length, passwordBytes.Length);

            string strWorker = password;

            switch (passwordFormat)
            {
                case MembershipPasswordFormat.Clear:
                    // We don't need to do anything
                    break;

                case MembershipPasswordFormat.Encrypted:
                    // Apply Base64 Encryption
                    strWorker = Convert.ToBase64String(this.provider.EncryptPassword(allBytes));
                    break;

                case MembershipPasswordFormat.Hashed:
                    // Hash the password
                    HashAlgorithm s = HashAlgorithm.Create(Membership.HashAlgorithmType);
                    byte[] returnBytes = s.ComputeHash(allBytes);

                    strWorker = Convert.ToBase64String(returnBytes);

                    break;

                default:
                    throw new ProviderException(Messages.UnsupportedPasswordFormat);
            }

            return strWorker;
        }

        #endregion

        #endregion
    }

    #endregion
}
