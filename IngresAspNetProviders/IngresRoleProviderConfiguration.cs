#region Code Header

/*  
 * Author               : Oliver P. Oyston (Luminary Solutions)
 * 
 * File Name            : IngresRoleProviderConfiguration.cs
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

    #endregion

    #region Ingres Role Provider Configuration

    /// <summary>
    /// Holds the configuration details for the Ingres Role Provider.
    /// </summary>
    internal class IngresRoleProviderConfiguration
    {
        #region Private Fields

        /// <summary>
        /// The application id for the application that we are providing role functionality for.
        /// The application id is a Guid stored as a string and is lower case. We store the value
        /// as a string because Ingres does not natively support Guids but we still want to use 
        /// them for globally unique id's.
        /// </summary>
        private string applicationId;

        /// <summary>
        /// A boolean indicating whether the applicationId field is the current value. This is
        /// used so that we do not have to access the database to retrieve the application id
        /// unless we absolutely must (e.g. due to changing the application which we are providing 
        /// the Role functionality for).
        /// </summary>
        private bool isApplicationIdCurrent = false;

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

        #endregion

        #region Constructors

        /// <summary>
        /// Initializes a new instance of the IngresRoleProviderConfiguration class.
        /// </summary>
        /// <param name="name">The name of the provider.</param>
        /// <param name="config">A NameValueCollection of configuration settings.</param>
        public IngresRoleProviderConfiguration(string name, NameValueCollection config)
        {
            this.Initialize(name, config);
        }

        #endregion

        #region Getters and Setters

        /// <summary>
        /// Gets or sets a value indicating whether the application id is current.
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
        /// Gets or sets the application name.
        /// </summary>
        public string ApplicationName
        {
            get
            {
                // we should have an application name set before we try and retrieve it
                if (string.IsNullOrEmpty(this.applicationName))
                {
                    throw new ProviderException(Messages.ValidApplicationNameNotSet);
                }

                return this.applicationName;
            }

            set
            {
                // validate the application before setting
                if (string.IsNullOrEmpty(value))
                {
                    throw new ProviderException(Messages.MustSpecifyApplicationName);
                }

                if (value.Length > 256)
                {
                    throw new ProviderException(string.Format(Messages.ApplicationNameTooLong));
                }

                // this.IsApplicationIdCurrent = false;
                this.applicationName = value;
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

            if (string.IsNullOrEmpty(name))
            {
                name = "IngresRoleProvider";
            }

            this.providerName = name;

            if (String.IsNullOrEmpty(config["description"]))
            {
                config.Remove("description");
                config.Add("description", "Ingres Role provider");
            }

            if (config["applicationName"] == null || config["applicationName"].Trim() == string.Empty)
            {
                this.ApplicationName = System.Web.Hosting.HostingEnvironment.ApplicationVirtualPath;
            }
            else
            {
                this.ApplicationName = config["applicationName"];
            }

            this.CommandTimeout = Convert.ToInt32(GetConfigValue(config["commandTimeout"], "30"));

            // Initialize the Ingres connection with values from the App.config file
            ConnectionStringSettingsCollection connectionSettings = ConfigurationManager.ConnectionStrings;

            if (connectionSettings.Count == 0)
            {
                throw new ProviderException(string.Format(Messages.ConnectionStringNotFound));
            }

            this.ConnectionString = connectionSettings[0].ToString();

            // Remove config entries and ensure nothing unexpected is present.
            // Note: we do not remove the description or name as these are needed
            // to initialize the provider base.
            config.Remove("connectionStringName");
            config.Remove("applicationName");
            config.Remove("commandTimeout");
            
            if (config.Count > 1)
            {
                string attribUnrecognized = config.GetKey(0);

                if (!String.IsNullOrEmpty(attribUnrecognized))
                {
                    throw new ProviderException(string.Format(Messages.AttributeNotRecognized, attribUnrecognized));
                }
            }
        #endregion
    }

    #endregion
    }
}
