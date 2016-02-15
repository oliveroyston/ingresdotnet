#region Code Header

/*  
 * Author               : Oliver P. Oyston (Luminary Solutions)
 * 
 * File Name            : IngresMembershipProviderConfiguration.cs
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
 * 1.0      04/10/2008  opo     Original Version
*/

#endregion

namespace Ingres.Web.Security
{
    #region NameSpaces Used

    using System;
    using System.Collections.Specialized;
    using System.Configuration;
    using System.Configuration.Provider;
    using System.Text.RegularExpressions;
    using System.Web;
    using System.Web.Security;

    #endregion

    #region Ingres Membership Provider Configuration

    /// <summary>
    /// Holds the configuration details for the Ingres Membership Provider.
    /// </summary>
    internal class IngresMembershipProviderConfiguration
    {
        #region Private Fields

        /// <summary>
        /// The default password attempt window.
        /// </summary>
        private const int DefaultPasswordAttemptWindow = 10;

        /// <summary>
        /// The default maximum invalid password attempts.
        /// </summary>
        private const int DefaultMaxInvalidPasswordAttempts = 5;

        /// <summary>
        /// The default minimum required non-alphanumeric characters in a password.
        /// </summary>
        private const int DefaultMinRequiredNonAlphanumericCharacters = 1;

        /// <summary>
        /// The default minimum required password length.
        /// </summary>
        private const int DefaultMinRequiredPasswordLength = 7;

        /// <summary>
        /// The default value for whether password reset is enabled.
        /// </summary>
        private const bool DefaultEnablePasswordReset = true;

        /// <summary>
        /// The default password strength regular expression to use.
        /// </summary>
        private readonly string DefaultPasswordStrengthRegularExpression = string.Empty;

        /// <summary>
        /// The default value for whether password retrieval us enabled.
        /// </summary>
        private const bool DefaultEnablePasswordRetrieval = true;

        /// <summary>
        /// The default value for whether password questions and answers are required.
        /// </summary>
        private const bool DefaultRequiresQuestionAndAnswer = false;

        /// <summary>
        /// The default value to use for whether users are required to have unique emails.
        /// </summary>
        private const bool DefaultRequiresUniqueEmail = true;

        /// <summary>
        /// The time duration to allow before a command timeout (in seconds).
        /// </summary>
        private int commandTimeout;

        /// <summary>
        /// The name of the ASP.NET Membership Provider.
        /// </summary>
        private string providerName;

        /// <summary>
        /// The connection string.
        /// </summary>
        private string connectionString;

        /// <summary>
        /// The name of the application.
        /// </summary>
        private string applicationName;

        /// <summary>
        /// Whether password reset functionality is enabled or not.
        /// </summary>
        private bool enablePasswordReset;

        /// <summary>
        /// Whether password retrieval functionality is enabled or not.
        /// </summary>
        private bool enablePasswordRetrieval;

        /// <summary>
        /// Whether password questions and answers are required.
        /// </summary>
        private bool requiresQuestionAndAnswer;

        /// <summary>
        /// Indicates whether a user must have a unique email or not.
        /// </summary>
        private bool requiresUniqueEmail;

        /// <summary>
        /// The maximum invalid password attempts.
        /// </summary>
        private int maxInvalidPasswordAttempts;

        /// <summary>
        /// The password attempt window.
        /// </summary>
        private int passwordAttemptWindow;

        /// <summary>
        /// The membership password format to use for the application.
        /// </summary>
        private MembershipPasswordFormat passwordFormat;

        /// <summary>
        /// The regular expression used to validate password strength.
        /// </summary>
        private string passwordStrengthRegularExpression;

        /// <summary>
        /// The minimum password length.
        /// </summary>
        private int minRequiredPasswordLength;

        /// <summary>
        /// The minimum required number of non-alphanumeric characters for a password.
        /// </summary>
        private int minRequiredNonAlphanumericCharacters;

        /// <summary>
        /// The minimum length of a new password.
        /// </summary>
        private int newPasswordLength;

        /// <summary>
        /// A boolean indicating whether the applicationId field is the current value. This is
        /// used so that we do not have to access the database to retrieve the application id
        /// unless we absolutely must (e.g. due to changing the application which we are providing 
        /// the Role functionality for).
        /// </summary>
        private bool isApplicationIdCurrent;

        /// <summary>
        /// The application id for the application that we are providing role functionality for.
        /// The application id is a Guid stored as a string and is lower case. We store the value
        /// as a string because Ingres does not natively support Guids but we still want to use 
        /// them for globally unique id's.
        /// </summary>
        private string applicationId;

        #endregion

        #region Constructor

        /// <summary>
        /// Initializes a new instance of the IngresMembershipProviderConfiguration class.
        /// </summary>
        /// <param name="name">The name of the provider.</param>
        /// <param name="config">A NameValueCollection of configuration settings.</param>
        public IngresMembershipProviderConfiguration(string name, NameValueCollection config)
        {
            this.Initialize(name, config);
        }

        #endregion

        #region Getters and Setters

        /// <summary>
        /// Gets or sets the name of the application that we are providing membership functionality for.
        /// </summary>
        public string ApplicationName
        {
            get
            {
                return this.applicationName;
            }

            set
            {
                // Ensure that the application name is valid before setting
                if (string.IsNullOrEmpty(value))
                {
                    throw new ArgumentNullException("value");
                }

                // The size of the application name must not exceed the max allowed in the database
                if (value.Length > 256)
                {
                    throw new ProviderException(string.Format(Messages.ApplicationNameTooLong));
                }

                this.applicationName = value;
            }
        }

        /// <summary>
        /// Gets a value indicating whether password reset is enabled.
        /// </summary>
        public bool EnablePasswordReset
        {
            get
            {
                return this.enablePasswordReset;
            }
        }

        /// <summary>
        /// Gets a value indicating whether password retrieval is enabled.
        /// </summary>
        public bool EnablePasswordRetrieval
        {
            get
            {
                return this.enablePasswordRetrieval;
            }
        }

        /// <summary>
        /// Gets the maximum number of invalid password attempts.
        /// </summary>
        public int MaxInvalidPasswordAttempts
        {
            get
            {
                return this.maxInvalidPasswordAttempts;
            }
        }

        /// <summary>
        /// Gets the minimum required number of alphanumeric characters in a password.
        /// </summary>
        public int MinRequiredNonAlphanumericCharacters
        {
            get
            {
                return this.minRequiredNonAlphanumericCharacters;
            }
        }

        /// <summary>
        /// Gets the minimum required password length.
        /// </summary>
        public int MinRequiredPasswordLength
        {
            get
            {
                return this.minRequiredPasswordLength;
            }
        }

        /// <summary>
        /// Gets the password attempt window.
        /// </summary>
        public int PasswordAttemptWindow
        {
            get
            {
                return this.passwordAttemptWindow;
            }
        }

        /// <summary>
        /// Gets the password format.
        /// </summary>
        public MembershipPasswordFormat PasswordFormat
        {
            get
            {
                return this.passwordFormat;
            }
        }

        /// <summary>
        /// Gets the regular expression that is required the password is required to meet.
        /// </summary>
        public string PasswordStrengthRegularExpression
        {
            get
            {
                return this.passwordStrengthRegularExpression;
            }
        }

        /// <summary>
        /// Gets a value indicating whether we require a password question and answer.
        /// </summary>
        public bool RequiresQuestionAndAnswer
        {
            get
            {
                return this.requiresQuestionAndAnswer;
            }
        }

        /// <summary>
        /// Gets a value indicating whether we require a unique email.
        /// </summary>
        public bool RequiresUniqueEmail
        {
            get
            {
                return this.requiresUniqueEmail;
            }
        }

        /// <summary>
        /// Gets or sets the connection string.
        /// </summary>
        public string ConnectionString
        {
            get
            {
                return this.connectionString;
            }

            set
            {
                this.connectionString = value;
            }
        }

        /// <summary>
        /// Gets or sets the command timeout.
        /// </summary>
        public int CommandTimeout
        {
            get
            {
                return this.commandTimeout;
            }

            set
            {
                this.commandTimeout = value;
            }
        }

        /// <summary>
        /// Gets or sets the provider name.
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
        /// Gets or sets a value indicating whether the ApplicationId is current.
        /// </summary>
        public bool IsApplicationIdCurrent
        {
            get
            {
                return this.isApplicationIdCurrent;
            }

            set
            {
                this.isApplicationIdCurrent = value;
            }
        }

        /// <summary>
        /// Gets or sets the application id for the application that we are providing role functionality for.
        /// </summary>
        public string ApplicationId
        {
            get
            {
                return this.applicationId;
            }

            set
            {
                this.applicationId = value;
            }
        }

        /// <summary>
        /// Gets or sets the minimum length of a new password.
        /// </summary>
        public int NewPasswordLength
        {
            get
            {
                return this.newPasswordLength;
            }

            set
            {
                this.newPasswordLength = value;
            }
        }

        #endregion

        #region Class Methods

        /// <summary>
        /// A helper function to retrieve config values from the configuration file.
        /// </summary>
        /// <param name="configValue">The value in the config file.</param>
        /// <param name="defaultValue">The default balue.</param>
        /// <typeparam name="T">The type of the default value.</typeparam>
        /// <returns>The value from the config file (or the default value if appropriate).</returns>
        private static string GetConfigValue<T>(string configValue, T defaultValue)
        {
            if (String.IsNullOrEmpty(configValue))
            {
                return defaultValue.ToString();
            }

            return configValue;
        }

        /// <summary>
        /// Takes, as input, the name of the provider and a NameValueCollection of configuration 
        /// settings. Used to set property values for the provider instance including 
        /// implementation-specific values and options specified in the configuration file 
        /// (Machine.config or Web.config) supplied in the configuration.
        /// </summary>
        /// <param name="name">The name of the provider.</param>
        /// <param name="config">A NameValueCollection of configuration settings.</param>
        private void Initialize(string name, NameValueCollection config)
        {
            // Initialize values from web.config.
            if (config == null)
            {
                throw new ArgumentNullException("config");
            }

            // Add the name if we don't already have one.
            if (string.IsNullOrEmpty(name))
            {
                name = "IngresMembershipProvider";
            }

            this.ProviderName = name;

            // Add a description if we don't already have one.
            if (String.IsNullOrEmpty(config["description"]))
            {
                config.Remove("description");
                config.Add("description", "Ingres ASP.NET Membership Provider");
            }

            // Get the application name
            this.applicationName = GetConfigValue(config["applicationName"], System.Web.Hosting.HostingEnvironment.ApplicationVirtualPath);

            if (this.applicationName.Length > 256)
            {
                throw new ProviderException(string.Format(Messages.ApplicationNameTooLong));
            }

            // Get the details from the config file.
            this.maxInvalidPasswordAttempts = Convert.ToInt32(GetConfigValue(config["maxInvalidPasswordAttempts"], DefaultMaxInvalidPasswordAttempts));
            this.passwordAttemptWindow = Convert.ToInt32(GetConfigValue(config["passwordAttemptWindow"], DefaultPasswordAttemptWindow));
            this.minRequiredNonAlphanumericCharacters = Convert.ToInt32(GetConfigValue(config["minRequiredNonAlphanumericCharacters"], DefaultMinRequiredNonAlphanumericCharacters));
            this.minRequiredPasswordLength = Convert.ToInt32(GetConfigValue(config["minRequiredPasswordLength"], DefaultMinRequiredPasswordLength));
            this.passwordStrengthRegularExpression = Convert.ToString(GetConfigValue(config["passwordStrengthRegularExpression"], this.DefaultPasswordStrengthRegularExpression));
            this.enablePasswordReset = Convert.ToBoolean(GetConfigValue(config["enablePasswordReset"], DefaultEnablePasswordReset));
            this.enablePasswordRetrieval = Convert.ToBoolean(GetConfigValue(config["enablePasswordRetrieval"], DefaultEnablePasswordRetrieval));
            this.requiresQuestionAndAnswer = Convert.ToBoolean(GetConfigValue(config["requiresQuestionAndAnswer"], DefaultRequiresQuestionAndAnswer));
            this.requiresUniqueEmail = Convert.ToBoolean(GetConfigValue(config["requiresUniqueEmail"], DefaultRequiresUniqueEmail));

            this.CommandTimeout = Convert.ToInt32(GetConfigValue(config["commandTimeout"], "30"));

            // Default the new password length to the min required password length.
            this.NewPasswordLength = Convert.ToInt32(GetConfigValue(config["minRequiredPasswordLength"], DefaultMinRequiredPasswordLength));

            if (this.minRequiredNonAlphanumericCharacters > this.minRequiredPasswordLength)
            {
                throw new HttpException(
                    string.Format(Messages.MinRequiredNonalphCharGreaterThanMinRequiredPasswordLength));
            }

            if (this.passwordStrengthRegularExpression != null)
            {
                this.passwordStrengthRegularExpression = this.passwordStrengthRegularExpression.Trim();

                if (this.passwordStrengthRegularExpression.Length != 0)
                {
                    try
                    {
                        Regex regex = new Regex(this.passwordStrengthRegularExpression);
                    }
                    catch (ArgumentException e)
                    {
                        throw new ProviderException(e.Message, e);
                    }
                }
            }
            else
            {
                this.passwordStrengthRegularExpression = string.Empty;
            }

            // Get the password format from the config file - if we don't have one then default to hashed for
            // security reasons
            string passwordFormatFromConfig = config["passwordFormat"];

            if (passwordFormatFromConfig != null)
            {
                switch (passwordFormatFromConfig.ToLower())
                {
                    case "hashed":
                        this.passwordFormat = MembershipPasswordFormat.Hashed;
                        break;
                    case "encrypted":
                        this.passwordFormat = MembershipPasswordFormat.Encrypted;
                        break;
                    case "clear":
                        this.passwordFormat = MembershipPasswordFormat.Clear;
                        break;
                    default:
                        throw new ProviderException("Password format not supported.");
                }
            }
            else
            {
                // Default the password to hashed
                this.passwordFormat = MembershipPasswordFormat.Hashed;
            }

            // Initialize the Ingres connection with values from the App.config file
            ConnectionStringSettingsCollection connectionSettings = ConfigurationManager.ConnectionStrings;

            if (connectionSettings.Count == 0)
            {
                throw new InvalidOperationException(Messages.NoConnectionInformationSpecifiedInApplicationConfigurationFile);
            }

            this.ConnectionString = connectionSettings[0].ToString();

            // Remove config entries and ensure nothing unexpected is present.
            // Note: we do not remove the description or name as these are needed
            // to initialize the provider base.
            config.Remove("connectionStringName");
            config.Remove("enablePasswordRetrieval");
            config.Remove("enablePasswordReset");
            config.Remove("requiresQuestionAndAnswer");
            config.Remove("applicationName");
            config.Remove("requiresUniqueEmail");
            config.Remove("maxInvalidPasswordAttempts");
            config.Remove("passwordAttemptWindow");
            config.Remove("commandTimeout");
            config.Remove("passwordFormat");
            config.Remove("minRequiredPasswordLength");
            config.Remove("minRequiredNonalphanumericCharacters");
            config.Remove("passwordStrengthRegularExpression");
            
            if (config.Count > 1) 
            {
                string attribUnrecognized = config.GetKey(0);

                if (!String.IsNullOrEmpty(attribUnrecognized))
                {
                    throw new ProviderException(string.Format(Messages.AttributeNotRecognized, attribUnrecognized));
                }
            }
        }

        #endregion
    }

    #endregion
}
