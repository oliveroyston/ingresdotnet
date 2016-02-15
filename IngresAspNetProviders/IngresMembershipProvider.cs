#region Code Header

/*  
 * Author               : Oliver P. Oyston (Luminary Solutions)
 * 
 * File Name            : IngresMembershipProvider.cs
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

    #region Ingres ASP.NET Membership Provider [Facade]

    /// <summary>
    /// A fully implemented custom membership provider to provide the interface between ASP.NET's 
    /// membership management service and the Ingres RDBMS. This membership provider is closely based 
    /// on the SQL Server implementation that shipped with .Net 2.0 and, by default, uses the same 
    /// table names and provides all the same functionality of its SQL Server counterpart so that 
    /// it can be used as a true "drop-in" alternative.
    /// </summary>
    /// <remarks>
    /// <para>
    /// ASP.NET membership gives you a built-in way to validate and store user credentials. ASP.NET 
    /// membership therefore helps you manage user authentication in your Web sites. You can use 
    /// ASP.NET membership with ASP.NET Forms authentication or with the ASP.NET login controls to 
    /// create a complete system for authenticating users.
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
    public class IngresMembershipProvider : MembershipProvider
    {
        /// <summary>
        /// The Ingres Membership provider configuration settings.
        /// </summary>
        private IngresMembershipProviderConfiguration config;

        #region Overridden Properties

        /// <summary>
        /// Gets or sets the application name. This property is read/write and defaults to the 
        /// <c>ApplicationPath</c> if not specified explicitly.
        /// </summary>
        /// <remarks>
        /// <para>
        /// The name of the application using the membership information specified in the 
        /// configuration file (Web.config). The <c>ApplicationName</c> is stored in the data
        /// source with related user information and used when querying for that information.
        /// </para>
        /// <para>
        /// Overrides a property from the <c>System.Web.Security.MembershipProvider</c> class to 
        /// provide an Ingres specific implementation.
        /// </para>
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

        /// <summary>
        /// Gets a value indicating whether passwords are able to be reset. This property is a 
        /// read-only Boolean value specified in the configuration file (Web.config).
        /// </summary>
        /// <remarks>
        /// <para>
        /// The EnablePasswordReset property indicates whether users can use the <c>ResetPassword</c> 
        /// method to overwrite their current password with a new, randomly generated password.
        /// </para>
        /// <para>
        /// Overrides a property from the <c>System.Web.Security.MembershipProvider</c> class to 
        /// provide an Ingres specific implementation.
        /// </para>
        /// </remarks>
        public override bool EnablePasswordReset
        {
            get
            {
                return this.config.EnablePasswordReset;
            }
        }

        /// <summary>
        /// Gets a value indicating whether password retrieval is enabled. This property is a 
        /// read-only Boolean value specified in the configuration file (Web.config).
        /// </summary>
        /// <remarks>
        /// <para>
        /// The <c>EnablePasswordRetrieval</c> property indicates whether users can retrieve their 
        /// password using the <c>GetPassword</c> method.
        /// </para>
        /// <para>
        /// Overrides a property from the <c>System.Web.Security.MembershipProvider</c> class to 
        /// provide an Ingres specific implementation.
        /// </para>
        /// </remarks>
        public override bool EnablePasswordRetrieval
        {
            get
            {
                return this.config.EnablePasswordRetrieval;
            }
        }

        /// <summary>
        /// Gets a value indicating whether we require password questions and answers. This 
        /// property is a read-only Boolean value specified in the configuration file (Web.config).
        /// </summary>
        /// <remarks>
        /// <para>
        /// The <c>RequiresQuestionAndAnswer</c> property indicates whether users must supply a password 
        /// answer in order to retrieve their password using the <c>GetPassword</c> method, or reset their
        /// password using the <c>ResetPassword</c> method.
        /// </para>
        /// <para>
        /// Overrides a property from the <c>System.Web.Security.MembershipProvider</c> class to provide 
        /// an Ingres specific implementation.
        /// </para>
        /// </remarks>
        public override bool RequiresQuestionAndAnswer
        {
            get
            {
                return this.config.RequiresQuestionAndAnswer;
            }
        }

        /// <summary>
        /// Gets a value indicating whether we require users to have unique emails. This property 
        /// is a read-only Boolean value specified in the configuration file (Web.config).
        /// </summary>
        /// <remarks>
        /// <para>
        /// The <c>RequiresUniqueEmail</c> property indicates whether users must supply a unique 
        /// ex-mail address value when creating a user. If a user already exists in the data source
        /// for the current <c>ApplicationName</c>, then the <c>CreateUser</c> method returns 
        /// <c>null</c> and a status value of <c>DuplicateEmail</c>.
        /// </para>
        /// <para>
        /// Overrides a property from the <c>System.Web.Security.MembershipProvider</c> class to 
        /// provide an Ingres specific implementation.
        /// </para>
        /// </remarks>
        public override bool RequiresUniqueEmail
        {
            get
            {
                return this.config.RequiresUniqueEmail;
            }
        }

        /// <summary>
        /// Gets the maximum number of invalid password attempts. This property is a read-only 
        /// integer value specified in the configuration file (Web.config).
        /// </summary>
        /// <remarks>
        /// <para>
        /// The <c>MaxInvalidPasswordAttempts</c> works in conjunction with the <c>PasswordAttemptWindow</c> 
        /// to guard against an unwanted source guessing the password or password answer of a 
        /// membership user through repeated attempts. If the number of invalid passwords or 
        /// password questions supplied for a membership user exceeds the <c>MaxInvalidPasswordAttempts</c> 
        /// within the number of minutes identified by the <c>PasswordAttemptWindow</c>, then the 
        /// membership user is locked out by setting the <c>IsLockedOut</c> property to true until the user 
        /// is unlocked using the <c>UnlockUser</c> method. If a valid password or password answer is 
        /// supplied before the <c>MaxInvalidPasswordAttempts</c> is reached, the counter that tracks the 
        /// number of invalid attempts is reset to zero.
        /// </para>
        /// <para>
        /// If the <c>RequiresQuestionAndAnswer</c> property is set to false, invalid password answer 
        /// attempts are not tracked.
        /// </para>
        /// <para>
        /// Invalid password and password answer attempts are tracked in the <c>ValidateUser</c>, 
        /// <c>ChangePassword</c>, <c>ChangePasswordQuestionAndAnswer</c>, <c>GetPassword</c>, and 
        /// <c>ResetPassword</c> methods.
        /// </para>
        /// <para>
        /// Overrides a property from the <c>System.Web.Security.MembershipProvider</c> class to provide 
        /// an Ingres specific implementation.
        /// </para>
        /// </remarks>
        public override int MaxInvalidPasswordAttempts
        {
            get
            {
                return this.config.MaxInvalidPasswordAttempts;
            }
        }

        /// <summary>
        /// Gets the password attempt window. This property is a read-only Integer value specified 
        /// in the configuration file (Web.config).
        /// </summary>
        /// <remarks>
        /// <para>
        /// For a description, see the description of the <c>MaxInvalidPasswordAttempts</c> property.
        /// </para>
        /// <para>
        /// Overrides a property from the <c>System.Web.Security.MembershipProvider</c> class to provide 
        /// an Ingres specific implementation.
        /// </para>
        /// </remarks>
        public override int PasswordAttemptWindow
        {
            get
            {
                return this.config.PasswordAttemptWindow;
            }
        }

        /// <summary>
        /// Gets the password format. This property is a read-only <c>MembershipPasswordFormat</c> 
        /// value specified in the configuration file (Web.config).
        /// </summary>
        /// <remarks>
        /// <para>
        /// The <c>PasswordFormat</c> property indicates the format that passwords are stored in. 
        /// Passwords can be stored in Clear, Encrypted, and Hashed password formats. Clear 
        /// passwords are stored in plain text, which improves the performance of password storage 
        /// and retrieval but is less secure, as passwords are easily read if your data source is 
        /// compromised. Encrypted passwords are encrypted when stored and can be decrypted for 
        /// password comparison or password retrieval. This requires additional processing for 
        /// password storage and retrieval but is more secure, as passwords are not easily 
        /// determined if the data source is compromised. Hashed passwords are hashed using a 
        /// one-way hash algorithm and a randomly generated salt value when stored in the database.
        /// When a password is validated, it is hashed with the salt value in the database for 
        /// verification. Hashed passwords cannot be retrieved.
        /// </para>
        /// <para>
        /// You can use the <c>EncryptPassword</c> and <c>DecryptPassword</c> virtual methods of the 
        /// MembershipProvider class to encrypt and decrypt password values, or you can supply your 
        /// own encryption code. If you use the <c>EncryptPassword</c> and <c>DecryptPassword</c> 
        /// virtual methods of the <c>MembershipProvider</c> class, Encrypted passwords are encrypted
        /// using the key information supplied in the machineKey Element (ASP.NET Settings Schema) in 
        /// your configuration.
        /// </para>
        /// <para>
        /// Overrides a property from the <c>System.Web.Security.MembershipProvider</c> class to provide 
        /// an Ingres specific implementation.
        /// </para>
        /// </remarks>
        public override MembershipPasswordFormat PasswordFormat
        {
            get
            {
                return this.config.PasswordFormat;
            }
        }

        /// <summary>
        /// Gets the minimum required non-alphanumeric characters that must be in users passwords.
        /// This property is read-only <c>MembershipPasswordFormat</c> value specified in the 
        /// configuration file (Web.config).
        /// </summary>
        /// <remarks>
        /// <para>
        /// The <c>PasswordFormat</c> property indicates the format that passwords are stored in. 
        /// Passwords can be stored in Clear, Encrypted, and Hashed password formats. Clear 
        /// passwords are stored in plain text, which improves the performance of password storage 
        /// and retrieval but is less secure, as passwords are easily read if your data source is 
        /// compromised. Encrypted passwords are encrypted when stored and can be decrypted for 
        /// password comparison or password retrieval. This requires additional processing for 
        /// password storage and retrieval but is more secure, as passwords are not easily 
        /// determined if the data source is compromised. Hashed passwords are hashed using a 
        /// one-way hash algorithm and a randomly generated salt value when stored in the database.
        /// When a password is validated, it is hashed with the salt value in the database for 
        /// verification. Hashed passwords cannot be retrieved.
        /// </para>
        /// <para>
        /// Encrypted passwords are encrypted using the key information supplied in the machineKey 
        /// Element (ASP.NET Settings Schema) in your configuration.
        /// </para>
        /// <para>
        /// Overrides a property from the <c>System.Web.Security.MembershipProvider</c> class to provide 
        /// an Ingres specific implementation.
        /// </para>
        /// </remarks>
        public override int MinRequiredNonAlphanumericCharacters
        {
            get
            {
                return this.config.MinRequiredNonAlphanumericCharacters;
            }
        }

        /// <summary>
        /// Gets the minimum required password length. This property is a read-only Integer value 
        /// specified in the configuration file (Web.config).
        /// </summary>
        /// <remarks>
        /// <para>
        /// The <c>MinRequiredPasswordLength</c> property indicates the minimum length of a password.
        /// </para>
        /// <para>
        /// Overrides a property from the <c>System.Web.Security.MembershipProvider</c> class to provide 
        /// an Ingres specific implementation.
        /// </para>
        /// </remarks>
        public override int MinRequiredPasswordLength
        {
            get
            {
                return this.config.MinRequiredPasswordLength;
            }
        }

        /// <summary>
        /// Gets the regular expression used to check if a password if of the required strength.
        /// This property is a read-only string specified in the configuration file (Web.config).
        /// </summary>
        /// <remarks>
        /// <para>
        /// The <c>PasswordStrengthRegularExpression</c> property specifies the regular expression to be
        /// used to ensure that a password meets the required strength policy.
        /// </para>
        /// <para
        /// >Overrides a property from the <c>System.Web.Security.MembershipProvider</c> class to provide 
        /// an Ingres specific implementation.
        /// </para>
        /// </remarks>
        public override string PasswordStrengthRegularExpression
        {
            get
            {
                return this.config.PasswordStrengthRegularExpression;
            }
        }

        #endregion

        #region System Configuration Initialization

        /// <summary>
        /// Takes, as input, the name of the provider and a NameValueCollection of configuration 
        /// settings. Used to set property values for the provider instance including 
        /// implementation-specific values and options specified in the configuration file 
        /// (Machine.config or Web.config) supplied in the configuration.
        /// </summary>
        /// <remarks>
        /// Overrides a method from the System.Configuration.Provider.ProviderBase class to provide 
        /// an Ingres specific implementation.
        /// </remarks>
        /// <param name="name">The name of the provider.</param>
        /// <param name="coll">A NameValueCollection of configuration settings.</param>
        public override void Initialize(string name, NameValueCollection coll)
        {
            // Initialise the configuration.
            this.config = new IngresMembershipProviderConfiguration(name, coll);

            // Initialize the abstract base class.
            base.Initialize(name, coll);
        }

        #endregion

        #region Overridden Methods

        #region ChangePassword

        /// <summary>
        /// Takes, as input, a user name, a current password, and a new password, and updates the 
        /// password in the data source if the supplied user name and current password are valid. 
        /// The <c>ChangePassword</c> method returns true if the password was updated successfully; 
        /// otherwise, false.
        /// </summary>
        /// <remarks>
        /// <para>
        /// The <c>ChangePassword</c> method raises the <c>ValidatingPassword</c> event, if a 
        /// <c>MembershipValidatePasswordEventHandler</c> has been specified, and continues or cancels the 
        /// change-password action based on the results of the event. You can use the 
        /// <c>OnValidatingPassword</c> virtual method to execute the specified 
        /// <c>MembershipValidatePasswordEventHandler</c>.
        /// </para>
        /// <para>
        /// This method overrides a method from the <c>System.Web.Security.MembershipProvider</c> 
        /// class to provide an Ingres specific implementation.
        /// </para>
        /// </remarks>
        /// <param name="username">The username.</param>
        /// <param name="oldPassword">The old password.</param>
        /// <param name="newPassword">The new password.</param>
        /// <returns>Returns true if the password was updated successfully; otherwise, false.</returns>
        public override bool ChangePassword(string username, string oldPassword, string newPassword)
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
                    result = this.GetHandler(conn, tran).ChangePassword(username, oldPassword, newPassword);

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
                    ExceptionHandler.LogRollbackWarning(MembershipProviderMethod.ChangePassword);
                }

                // Handle the exception appropriately
                ExceptionHandler.HandleException(ex, MembershipProviderMethod.ChangePassword);
            }

            return result;
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
        /// <para>
        /// If the supplied user name and password are not valid, false is returned.
        /// </para>
        /// <para>
        /// This method overrides a method from the <c>System.Web.Security.MembershipProvider</c> class to provide an 
        /// Ingres specific implementation.
        /// </para>
        /// </remarks>
        /// <param name="username">The username.</param>
        /// <param name="password">The password.</param>
        /// <param name="newPasswordQuestion">The new password question.</param>
        /// <param name="newPasswordAnswer">The new password answer.</param>
        /// <returns>Returns true if the password question and answer are updated successfully; otherwise, false.</returns>
        public override bool ChangePasswordQuestionAndAnswer(string username, string password, string newPasswordQuestion, string newPasswordAnswer)
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
                    result = this.GetHandler(conn, tran).ChangePasswordQuestionAndAnswer(username, password, newPasswordQuestion, newPasswordAnswer);

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
                    ExceptionHandler.LogRollbackWarning(MembershipProviderMethod.ChangePasswordQuestionAndAnswer);
                }

                // Handle the exception appropriately
                ExceptionHandler.HandleException(ex, MembershipProviderMethod.ChangePasswordQuestionAndAnswer);
            }

            return result;
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
        /// <para>
        /// The <c>CreateUser</c> method raises the <c>ValidatingPassword</c> event, if a 
        /// <c>MembershipValidatePasswordEventHandler</c> has been specified, and continues or cancels the 
        /// create-user action based on the results of the event. You can use the 
        /// <c>OnValidatingPassword</c> virtual method to execute the specified 
        /// <c>MembershipValidatePasswordEventHandler</c>.
        /// </para>
        /// <para>
        /// This method overrides a method from the <c>System.Web.Security.MembershipProvider</c> class to provide an 
        /// Ingres specific implementation.
        /// </para>
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
        public override MembershipUser CreateUser(string username, string password, string email, string passwordQuestion, string passwordAnswer, bool isApproved, object providerUserKey, out MembershipCreateStatus status)
        {
            status = MembershipCreateStatus.ProviderError;
            
            MembershipUser result = null;

            IngresTransaction tran = null;
            
            try
            {
                using (IngresConnection conn = new IngresConnection(this.config.ConnectionString))
                {
                    // Open the connection and start a new transaction
                    conn.Open();

                    tran = conn.BeginTransaction();

                    // Call the implementation of the method
                    result = this.GetHandler(conn, tran).CreateUser(username, password, email, passwordQuestion, passwordAnswer, isApproved, providerUserKey, out status);

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
                    ExceptionHandler.LogRollbackWarning(MembershipProviderMethod.CreateUser);
                }

                // Handle the exception appropriately
                ExceptionHandler.HandleException(ex, MembershipProviderMethod.CreateUser);
            }

            return result;
        }

        #endregion

        #region DeleteUser

        /// <summary>
        /// Takes, as input, the name of a user and deletes that user's information from the data 
        /// source. The <c>DeleteUser</c> method returns true if the user was successfully deleted; 
        /// otherwise, false. An additional Boolean parameter is included to indicate whether 
        /// related information for the user, such as role or profile information is also deleted.
        /// </summary>
        /// <remark>
        /// This method overrides a method from the <c>System.Web.Security.MembershipProvider</c> class to provide an 
        /// Ingres specific implementation.
        /// </remark>
        /// <param name="username">The username to delete.</param>
        /// <param name="deleteAllRelatedData">Whether to delete all related data or not.</param>
        /// <returns>Returns true if the user was successfully deleted; otherwise, false.</returns>
        public override bool DeleteUser(string username, bool deleteAllRelatedData)
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
                    result = this.GetHandler(conn, tran).DeleteUser(username, deleteAllRelatedData);

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
                    ExceptionHandler.LogRollbackWarning(MembershipProviderMethod.DeleteUser);
                }

                // Handle the exception appropriately
                ExceptionHandler.HandleException(ex, MembershipProviderMethod.DeleteUser);
            }

            return result;
        }

        #endregion

        #region FindUsersByEmail

        /// <summary>
        /// Returns a list of membership users where the user name contains a match of the supplied 
        /// emailToMatch for the configured <c>ApplicationName</c>. For example, if the <c>emailToMatch</c> 
        /// parameter is set to "address@example.com," then users with the ex-mail addresses 
        /// "address1@example.com," "address2@example.com," and so on are returned. Wildcard support 
        /// is included based on the data source. Users are returned in alphabetical order by user 
        /// name.
        /// </summary>
        /// <remarks>
        /// <para>
        /// The results returned by <c>FindUsersByEmail</c> are constrained by the <c>pageIndex</c> and <c>pageSize</c> 
        /// parameters. The <c>pageSize</c> parameter identifies the number of <c>MembershipUser</c> objects to 
        /// return in the <c>MembershipUserCollection</c> collection. The <c>pageIndex</c> parameter identifies 
        /// which page of results to return, where 1 identifies the first page. The <c>totalRecords</c> 
        /// parameter is an out parameter that is set to the total number of membership users that 
        /// matched the <c>emailToMatch</c> value. For example, if 13 users were found where <c>emailToMatch</c> 
        /// matched part of or the entire user name, and the <c>pageIndex</c> value was 2 with a <c>pageSize</c> 
        /// of 5, then the <c>MembershipUserCollection</c> would contain the sixth through the tenth users 
        /// returned. <c>totalRecords</c> would be set to 13.
        /// </para>
        /// <para>
        /// This method overrides a method from the <c>System.Web.Security.MembershipProvider</c> class to provide an 
        /// Ingres specific implementation.
        /// </para>
        /// </remarks>
        /// <param name="emailToMatch">The email to match.</param>
        /// <param name="pageIndex">The page number to return.</param>
        /// <param name="pageSize">The number of users per page.</param>
        /// <param name="totalRecords">[out] the number of matched users.</param>
        /// <returns>Returns a list of membership users where the user name contains a match of the 
        /// supplied emailToMatch for the configured ApplicationName.</returns>
        public override MembershipUserCollection FindUsersByEmail(string emailToMatch, int pageIndex, int pageSize, out int totalRecords)
        {
            totalRecords = 0; 

            MembershipUserCollection result = new MembershipUserCollection();

            IngresTransaction tran = null;
            
            try
            {
                using (IngresConnection conn = new IngresConnection(this.config.ConnectionString))
                {
                    // Open the connection and start a new transaction
                    conn.Open();

                    tran = conn.BeginTransaction();

                    // Call the implementation of the method
                    result = this.GetHandler(conn, tran).FindUsersByEmail(emailToMatch, pageIndex, pageSize, out totalRecords);

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
                    ExceptionHandler.LogRollbackWarning(MembershipProviderMethod.FindUsersByEmail);
                }

                // Handle the exception appropriately
                ExceptionHandler.HandleException(ex, MembershipProviderMethod.FindUsersByEmail);
            }

            return result;
        }

        #endregion

        #region FindUsersByName

        /// <summary>
        /// Returns a list of membership users where the user name contains a match of the supplied 
        /// <c>usernameToMatch</c> for the configured <c>ApplicationName</c>. For example, if the 
        /// <c>usernameToMatch</c> parameter is set to "user," then the users "user1," "user2," 
        /// "user3," and so on are returned. Wildcard support is included based on the data source. 
        /// Users are returned in alphabetical order by user name.
        /// </summary>
        /// <remarks>
        /// <para>
        /// The results returned by <c>FindUsersByName</c> are constrained by the <c>pageIndex</c> 
        /// and <c>pageSize</c> parameters. The <c>pageSize</c> parameter identifies the number of 
        /// <c>MembershipUser</c> objects to return in the <c>MembershipUserCollection</c>. The 
        /// <c>pageIndex</c> parameter identifies which page of results to return, where 1 identifies 
        /// the first page. The <c>totalRecords</c> parameter is an out parameter that is set to the 
        /// total number of membership users that matched the <c>usernameToMatch</c> value. For example, 
        /// if 13 users were found where <c>usernameToMatch</c> matched part of or the entire user 
        /// name, and the <c>pageIndex</c> value was 2 with a <c>pageSize</c> of 5, then the 
        /// <c>MembershipUserCollection</c> would contain the sixth through the tenth users returned. 
        /// <c>totalRecords</c> would be set to 13.
        /// </para>
        /// <para>
        /// This method overrides a method from the <c>System.Web.Security.MembershipProvider</c> class to provide an 
        /// Ingres specific implementation.
        /// </para>
        /// </remarks>
        /// <param name="usernameToMatch">The username to match.</param>
        /// <param name="pageIndex">The page to return.</param>
        /// <param name="pageSize">The number of users to return.</param>
        /// <param name="totalRecords">[out] the total number of matches.</param>
        /// <returns>Returns a list of membership users where the user name contains a match of the 
        /// supplied usernameToMatch for the configured ApplicationName.</returns>
        public override MembershipUserCollection FindUsersByName(string usernameToMatch, int pageIndex, int pageSize, out int totalRecords)
        {
            totalRecords = 0;

            MembershipUserCollection result = new MembershipUserCollection();

            IngresTransaction tran = null;
            
            try
            {
                using (IngresConnection conn = new IngresConnection(this.config.ConnectionString))
                {
                    // Open the connection and start a new transaction
                    conn.Open();

                    tran = conn.BeginTransaction();

                    // Call the implementation of the method
                    result = this.GetHandler(conn, tran).FindUsersByName(usernameToMatch, pageIndex, pageSize, out totalRecords);

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
                    ExceptionHandler.LogRollbackWarning(MembershipProviderMethod.FindUsersByName);
                }

                // Handle the exception appropriately
                ExceptionHandler.HandleException(ex, MembershipProviderMethod.FindUsersByName);
            }

            return result;
        }

        #endregion

        #region GetAllUsers

        /// <summary>
        /// Returns a <c>MembershipUserCollection</c> populated with <c>MembershipUser</c> objects for all of the 
        /// users in the data source.
        /// </summary>
        /// <remarks>
        /// <para>
        /// The results returned by <c>GetAllUsers</c> are constrained by the <c>pageIndex</c> and <c>pageSize</c> 
        /// parameters. The <c>pageSize</c> parameter identifies the maximum number of <c>MembershipUser</c> 
        /// objects to return in the <c>MembershipUserCollection</c>. The <c>pageIndex</c> parameter identifies 
        /// which page of results to return, where 1 identifies the first page. The <c>totalRecords</c> 
        /// parameter is an out parameter that is set to the total number of membership users. For 
        /// example, if 13 users were in the database for the application, and the pageIndex value 
        /// was 2 with a pageSize of 5, the <c>MembershipUserCollection</c> returned would contain the 
        /// sixth through the tenth users returned. <c>totalRecords</c> would be set to 13.
        /// </para>
        /// <para>
        /// This method overrides a method from the <c>System.Web.Security.MembershipProvider</c> class to provide an 
        /// Ingres specific implementation.
        /// </para>
        /// </remarks>
        /// <param name="pageIndex">Which page to return.</param>
        /// <param name="pageSize">The maximum number of users to return.</param>
        /// <param name="totalRecords">[out] The total number of users.</param>
        /// <returns>Returns a MembershipUserCollection populated with MembershipUser objects 
        /// for all of the users in the data source.</returns>
        public override MembershipUserCollection GetAllUsers(int pageIndex, int pageSize, out int totalRecords)
        {
            totalRecords = 0;

            MembershipUserCollection result = new MembershipUserCollection();

            IngresTransaction tran = null;
            
            try
            {
                using (IngresConnection conn = new IngresConnection(this.config.ConnectionString))
                {
                    // Open the connection and start a new transaction
                    conn.Open();

                    tran = conn.BeginTransaction();

                    // Call the implementation of the method
                    result = this.GetHandler(conn, tran).GetAllUsers(pageIndex, pageSize, out totalRecords);

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
                    ExceptionHandler.LogRollbackWarning(MembershipProviderMethod.GetAllUsers);
                }

                // Handle the exception appropriately
                ExceptionHandler.HandleException(ex, MembershipProviderMethod.GetAllUsers);
            }

            return result;
        }

        #endregion

        #region GetNumberOfUsersOnline

        /// <summary>
        /// Returns an integer value that is the count of all the users in the data source where 
        /// the <c>LastActivityDate</c> is greater than the current date and time minus the 
        /// <c>UserIsOnlineTimeWindow</c> property. The <c>UserIsOnlineTimeWindow</c> property is an integer 
        /// value specifying the number of minutes to use when determining whether a user is online.
        /// </summary>
        /// <remark>
        /// This method overrides a method from the <c>System.Web.Security.MembershipProvider</c> class to provide an 
        /// Ingres specific implementation.
        /// </remark>
        /// <returns>The number of users online.</returns> 
        public override int GetNumberOfUsersOnline()
        {
            int result = 0;

            IngresTransaction tran = null;
            
            try
            {
                using (IngresConnection conn = new IngresConnection(this.config.ConnectionString))
                {
                    // Open the connection and start a new transaction
                    conn.Open();

                    tran = conn.BeginTransaction();

                    // Call the implementation of the method
                    result = this.GetHandler(conn, tran).GetNumberOfUsersOnline(conn, tran);

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
                    ExceptionHandler.LogRollbackWarning(MembershipProviderMethod.GetNumberOfUsersOnline);
                }

                // Handle the exception appropriately
                ExceptionHandler.HandleException(ex, MembershipProviderMethod.GetNumberOfUsersOnline);
            }

            return result;
        }

        #endregion

        #region GetPassword

        /// <summary>
        /// Takes, as input, a user name and a password answer and retrieves the password for that 
        /// user from the data source and returns the password as a string.
        /// </summary>
        /// <remarks>
        /// <para>
        /// GetPassword ensures that the <c>EnablePasswordRetrieval</c> property is set to true before 
        /// performing any action. If the <c>EnablePasswordRetrieval</c> property is false, an 
        /// <c>ProviderException</c> is thrown.
        /// </para>
        /// <para>
        /// The <c>GetPassword</c> method also checks the value of the <c>RequiresQuestionAndAnswer</c> property. 
        /// If the <c>RequiresQuestionAndAnswer</c> property is true, the <c>GetPassword</c> method checks the 
        /// value of the supplied answer parameter against the stored password answer in the data 
        /// source. If they do not match, a <c>MembershipPasswordException</c> is thrown.
        /// </para>
        /// <para>
        /// This method overrides a method from the <c>System.Web.Security.MembershipProvider</c> 
        /// class to provide an Ingres specific implementation.
        /// </para>
        /// </remarks>
        /// <param name="username">The username.</param>
        /// <param name="answer">The password answer.</param>
        /// <returns>The password for the given username.</returns>
        public override string GetPassword(string username, string answer)
        {
            string result = string.Empty;

            IngresTransaction tran = null;
            
            try
            {
                using (IngresConnection conn = new IngresConnection(this.config.ConnectionString))
                {
                    // Open the connection and start a new transaction
                    conn.Open();

                    tran = conn.BeginTransaction();

                    // Call the implementation of the method
                    result = this.GetHandler(conn, tran).GetPassword(username, answer);

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
                    ExceptionHandler.LogRollbackWarning(MembershipProviderMethod.GetPassword);
                }

                // Handle the exception appropriately
                ExceptionHandler.HandleException(ex, MembershipProviderMethod.GetPassword);
            }

            return result;
        }

        #endregion

        #region GetUser

        /// <summary>
        /// Takes, as input, a user name and a Boolean value indicating whether to update the 
        /// <c>LastActivityDate</c> value for the user to show that the user is currently online. The 
        /// <c>GetUser</c> method returns a <c>MembershipUser</c> object populated with current values from the 
        /// data source for the specified user. If the user name is not found in the data source, 
        /// the <c>GetUser</c> method returns <c>null</c>.
        /// </summary>
        /// <remark>
        /// This method overrides a method from the <c>System.Web.Security.MembershipProvider</c> class to provide an 
        /// Ingres specific implementation.
        /// </remark>
        /// <param name="username">The username.</param>
        /// <param name="userIsOnline">Whether the user us currently online.</param>
        /// <returns>The membership user with the specified username.</returns>
        public override MembershipUser GetUser(string username, bool userIsOnline)
        {
            MembershipUser result = null;

            IngresTransaction tran = null;
            
            try
            {
                using (IngresConnection conn = new IngresConnection(this.config.ConnectionString))
                {
                    // Open the connection and start a new transaction
                    conn.Open();

                    tran = conn.BeginTransaction();

                    // Call the implementation of the method
                    result = this.GetHandler(conn, tran).GetUser(username, userIsOnline);

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
                    ExceptionHandler.LogRollbackWarning(MembershipProviderMethod.GetUser);
                }

                // Handle the exception appropriately
                ExceptionHandler.HandleException(ex, MembershipProviderMethod.GetUser);
            }

            return result;
        }

        #endregion

        #region GetUser

        /// <summary>
        /// Takes, as input, a unique user identifier and a Boolean value indicating whether to 
        /// update the <c>LastActivityDate</c> value for the user to show that the user is currently 
        /// online. The <c>GetUser</c> method returns a <c>MembershipUser</c> object populated with current 
        /// values from the data source for the specified user. If the user name is not found in 
        /// the data source, the <c>GetUser</c> method returns null.
        /// </summary>
        /// <remark>
        /// This method overrides a method from the <c>System.Web.Security.MembershipProvider</c> class to provide an 
        /// Ingres specific implementation.
        /// </remark>
        /// <param name="providerUserKey">The unique indentifer for the user.</param>
        /// <param name="userIsOnline">Whether the user is online.</param>
        /// <returns>The membership user with the specified provider user key.</returns>
        public override MembershipUser GetUser(object providerUserKey, bool userIsOnline)
        {
            MembershipUser result = null;

            IngresTransaction tran = null;
            
            try
            {
                using (IngresConnection conn = new IngresConnection(this.config.ConnectionString))
                {
                    // Open the connection and start a new transaction
                    conn.Open();

                    tran = conn.BeginTransaction();

                    // Call the implementation of the method
                    result = this.GetHandler(conn, tran).GetUser(providerUserKey, userIsOnline);

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
                    ExceptionHandler.LogRollbackWarning(MembershipProviderMethod.GetUserByObject);
                }

                // Handle the exception appropriately
                ExceptionHandler.HandleException(ex, MembershipProviderMethod.GetUserByObject);
            }

            return result;
        }

        #endregion

        #region GetUserNameByEmail

        /// <summary>
        /// Takes, as input, an ex-mail address and returns the first user name from the data source 
        /// where the ex-mail address matches the supplied email parameter value.
        /// </summary>
        /// <remarks>
        /// <para>
        /// If no user name is found with a matching ex-mail address, an empty string is returned.
        /// </para>
        /// <para>
        /// If multiple user names are found that match a particular ex-mail address, only the first 
        /// user name found is returned.
        /// </para>
        /// <para>
        /// This method overrides a method from the <c>System.Web.Security.MembershipProvider</c> class to provide an 
        /// Ingres specific implementation.
        /// </para>
        /// </remarks>
        /// <param name="email">The email to get the username for.</param>
        /// <returns>The first user name from the data source where the ex-mail address matches the 
        /// supplied email parameter value.</returns>
        public override string GetUserNameByEmail(string email)
        {
            string result = string.Empty;

            IngresTransaction tran = null;

            try
            {
                using (IngresConnection conn = new IngresConnection(this.config.ConnectionString))
                {
                    // Open the connection and start a new transaction
                    conn.Open();

                    tran = conn.BeginTransaction();

                    // Call the implementation of the method
                    result = this.GetHandler(conn, tran).GetUserNameByEmail(email);

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
                    ExceptionHandler.LogRollbackWarning(MembershipProviderMethod.GetUserNameByEmail);
                }

                // Handle the exception appropriately
                ExceptionHandler.HandleException(ex, MembershipProviderMethod.GetUserNameByEmail);
            }

            return result;
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
        /// <para>
        /// The <c>ResetPassword</c> method ensures that the <c>EnablePasswordReset</c> property is set to true 
        /// before performing any action. If the <c>EnablePasswordReset</c> property is false, a 
        /// <c>NotSupportedException</c> is thrown. The <c>ResetPassword</c> method also checks the value of the 
        /// <c>RequiresQuestionAndAnswer</c> property. If the <c>RequiresQuestionAndAnswer</c> property is true, 
        /// the <c>ResetPassword</c> method checks the value of the supplied answer parameter against the 
        /// stored password answer in the data source. If they do not match, a 
        /// <c>MembershipPasswordException</c> is thrown.
        /// </para>
        /// <para>
        /// The <c>ResetPassword</c> method raises the <c>ValidatingPassword</c> event, if a 
        /// <c>MembershipValidatePasswordEventHandler</c> has been specified, to validate the newly 
        /// generated password and continues or cancels the reset-password action based on the 
        /// results of the event. You can use the <c>OnValidatingPassword</c> virtual method to execute 
        /// the specified <c>MembershipValidatePasswordEventHandler</c>.
        /// </para>
        /// <para>
        /// This method overrides a method from the <c>System.Web.Security.MembershipProvider</c> class to provide an 
        /// Ingres specific implementation.
        /// </para>
        /// </remarks>
        /// <param name="username">The username to reset the password for.</param>
        /// <param name="answer">The answer to the users password question.</param>
        /// <returns>Whether the membership user was successfully updated.</returns>
        public override string ResetPassword(string username, string answer)
        {
            string result = string.Empty;

            IngresTransaction tran = null;

            try
            {
                using (IngresConnection conn = new IngresConnection(this.config.ConnectionString))
                {
                    // Open the connection and start a new transaction
                    conn.Open();

                    tran = conn.BeginTransaction();

                    // Call the implementation of the method
                    result = this.GetHandler(conn, tran).ResetPassword(username, answer);

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
                    ExceptionHandler.LogRollbackWarning(MembershipProviderMethod.ResetPassword);
                }

                // Handle the exception appropriately
                ExceptionHandler.HandleException(ex, MembershipProviderMethod.ResetPassword);
            }

            return result;
        }

        #endregion

        #region UnlockUser

        /// <summary>
        /// Takes, as input, a user name, and updates the field in the data source that stores the 
        /// IsLockedOut property to false. The <c>UnlockUser</c> method returns true if the record for the 
        /// membership user is updated successfully; otherwise false.
        /// </summary>
        /// <remark>
        /// This method overrides a method from the <c>System.Web.Security.MembershipProvider</c> 
        /// class to provide an Ingres specific implementation.
        /// </remark>
        /// <param name="userName">The username to unlock.</param>
        /// <returns>Whether the membership user was successfully unlocked.</returns>
        public override bool UnlockUser(string userName)
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
                    result = this.GetHandler(conn, tran).UnlockUser(userName);

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
                    ExceptionHandler.LogRollbackWarning(MembershipProviderMethod.UnlockUser);
                }

                // Handle the exception appropriately
                ExceptionHandler.HandleException(ex, MembershipProviderMethod.UnlockUser);
            }

            return result;
        }

        #endregion

        #region UpdateUser

        /// <summary>
        /// Takes, as input, a <c>MembershipUser</c> object populated with user information and updates 
        /// the data source with the supplied values.
        /// </summary>
        /// <remark>
        /// This method overrides a method from the <c>System.Web.Security.MembershipProvider</c> 
        /// class to provide an Ingres specific implementation.
        /// </remark>
        /// <param name="user">The membership user to update.</param>
        public override void UpdateUser(MembershipUser user)
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
                    this.GetHandler(conn, tran).UpdateUser(user);

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
                    ExceptionHandler.LogRollbackWarning(MembershipProviderMethod.UpdateUser);
                }

                // Handle the exception appropriately
                ExceptionHandler.HandleException(ex, MembershipProviderMethod.UpdateUser);
            }
        }

        #endregion

        #region ValidateUser

        /// <summary>
        /// Takes, as input, a user name and a password and verifies that the values match those in 
        /// the data source. The <c>ValidateUser</c> method returns true for a successful user name and 
        /// password match; otherwise, false.
        /// </summary>
        /// <remark>
        /// This method overrides a method from the <c>System.Web.Security.MembershipProvider</c> 
        /// class to provide an Ingres specific implementation.
        /// </remark>
        /// <param name="username">The username of the user we wish to validate.</param>
        /// <param name="password">The users password.</param>
        /// <returns>Whether the user was successfully validated.</returns>
        public override bool ValidateUser(string username, string password)
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
                    result = this.GetHandler(conn, tran).ValidateUser(username, password);

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
                    ExceptionHandler.LogRollbackWarning(MembershipProviderMethod.ValidateUser);
                }

                // Handle the exception appropriately
                ExceptionHandler.HandleException(ex, MembershipProviderMethod.ValidateUser);
            }

            return result;
        }

        #endregion

        #region DecryptPassword

        /// <summary>
        /// Decrypts an encrypted password.
        /// </summary>
        /// <param name="encodedPassword">A byte array that contains the encrypted password to decrypt</param>
        /// <returns>A byte array containing the decrypted password.</returns>
        internal new byte[] DecryptPassword(byte[] encodedPassword)
        {
            // Just use the base implementation - included here for documentation purposes only
            return base.DecryptPassword(encodedPassword);
        }

        #endregion

        #region EncryptPassword

        /// <summary>
        /// Encrypts a password.
        /// </summary>
        /// <param name="password">A byte array that contains the password to encrypt.</param>
        /// <returns>A byte array containing the encrypted password.</returns>
        internal new byte[] EncryptPassword(byte[] password)
        {
            // Just use the base implementation - included here for documentation purposes only
            return base.EncryptPassword(password);
        }

        /// <summary>
        /// Raises the validating password event.
        /// </summary>
        /// <param name="e">The event arguments.</param>
        internal new void OnValidatingPassword(ValidatePasswordEventArgs e)
        {
            base.OnValidatingPassword(e);
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
        private IngresMembershipProviderHandler GetHandler(IngresConnection conn, IngresTransaction tran)
        {
            IngresMembershipProviderHandler handler = new IngresMembershipProviderHandler(conn, tran, this.config, this);
            return handler;
        }

        #endregion
    }

    #endregion
}
