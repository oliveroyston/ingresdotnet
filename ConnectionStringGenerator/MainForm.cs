#region Code Header

/*  
 * Author               : Oliver P. Oyston (Luminary Solutions)
 * 
 * File Name            : MainForm.cs
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

namespace ConnectionStringGenerator
{
    #region Namespaces Used

    using System;
    using System.Data;
    using System.ServiceProcess;
    using System.Threading;
    using System.Windows.Forms;
    using Ingres.Client;
    using TimeoutException = System.ServiceProcess.TimeoutException;

    #endregion

    /// <summary>
    /// A simple WinForms application to help in testing connection strings for
    /// use with .NET and the Ingres .NET data provider.
    /// </summary>
    public partial class MainForm : Form
    {
        /// <summary>
        /// The name of the Ingres service.
        /// </summary>
        public const string INGRES_SERVICE_NAME = "Ingres Intelligent Database [II]";

        /// <summary>
        /// The amount of time (in milliseconds) to wait for the service to start before
        /// throwing a timeout exception.
        /// </summary>
        public const int INGRES_SERVICE_OPERATION_TIMEOUT = 90000;

        /// <summary>
        /// A connection string builder to use in building up our connection string. This
        /// object will be the selected object for the property grid on the page.
        /// </summary>
        private IngresConnectionStringBuilder builder;

        /// <summary>
        /// Initializes a new instance of the MainForm class.
        /// </summary>
        public MainForm()
        {
            // Initialise the form.
            this.InitializeComponent();

            // Create a new ingres connection string builder.
            this.builder = new IngresConnectionStringBuilder();

            // Default the database name to the default Ingres ASP.NET providers database
            this.builder.Database = "aspnetdb";
            
            // Set the property grids selected item to be the string builder.
            this.connectionStringPropertyGrid.SelectedObject = this.builder;

            // Trigger the property changed event so that the connection string text box
            // is updated for us.
            this.ConnectionStringPropertyGridValueChanged(this, null);

            // Determine the state of the ingres service and then start the timer to automatically
            // check the state periodically.
            this.DetermineIngresServiceState();

            this.timer.Start();
        }

        /// <summary>
        /// Attempts to start the Ingres service on the local mcachine. This static method is
        /// ran in a seperate thread so that the application is still usable whilst we attempt
        /// to start the service
        /// </summary>
        private static void StartIngresService()
        {
            ServiceController service = new ServiceController(INGRES_SERVICE_NAME);

            try
            {
                TimeSpan timeout = TimeSpan.FromMilliseconds(INGRES_SERVICE_OPERATION_TIMEOUT);

                if (service.Status == ServiceControllerStatus.Stopped)
                {
                    service.Start();
                }
                else
                {
                    return;
                }

                service.WaitForStatus(ServiceControllerStatus.Running, timeout);

                MessageBox.Show("The Ingres service was successfully started.");
            }
            catch (TimeoutException)
            {
                MessageBox.Show(
                    "The Ingres service failed to start in the allowed time. The service might still be in the process of starting...");
            }
            catch (Exception)
            {
                MessageBox.Show("An error occurred attempting to start the Ingres service. ");
            }
            finally
            {
                service.Close();
            }
        }

        /// <summary>
        /// Handles the press of the test connection string button.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The event arguments.</param>
        private void TestConnectionString(object sender, EventArgs e)
        {
            this.btnTest.Enabled = false;
            
            IngresConnection conn = new IngresConnection();

            if (this.tbConnectionString.Text.Trim() == string.Empty)
            {
                MessageBox.Show("A connection string must be entered.");

                this.btnTest.Enabled = true;

                return;
            }

            try
            {
                conn.ConnectionString = this.tbConnectionString.Text;
            }
            catch (Exception)
            {
                this.tbConnectionResult.Text = "Invalid connection string.";

                this.btnTest.Enabled = true;

                return;
            }

            try
            {
                conn.Open();

                this.tbConnectionResult.Text = "Successfully opened a connection!";

                MessageBox.Show("A connection to the database was successfully opened!\n\nCopy the generated connection string for use in the web config file.", "Successful Connection");
            }
            catch (Exception ex)
            {
                this.tbConnectionResult.Text = ex.Message;
            }
            finally
            {
                if (conn.State != ConnectionState.Closed)
                {
                    conn.Close();
                }

                conn = null;

                this.btnTest.Enabled = true;
            }
        }

        /// <summary>
        /// Handles the property value changed event of the connection string property grid.
        /// </summary>
        /// <param name="s">The sender.</param>
        /// <param name="e">The event arguments.</param>
        private void ConnectionStringPropertyGridValueChanged(object s, PropertyValueChangedEventArgs e)
        {
            this.tbConnectionString.Text = this.builder.ConnectionString;
        }

        /// <summary>
        /// Handler for copt to cliboard toolbar button.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The event arguments.</param>/// 
        private void CopyConnectionStringToClipboard(object sender, EventArgs e)
        {
            // Copy the connection string textbox content to the clipboard.
            Clipboard.SetDataObject(this.tbConnectionString.Text, true);
        }

        /// <summary>
        /// Helper mathod to determine the state of the Ingres service. We enable the 
        /// button to start the service if the service is stopped. Otherwise the button
        /// is disabled.
        /// </summary>
        private void DetermineIngresServiceState()
        {
            ServiceController service = new ServiceController(INGRES_SERVICE_NAME);

            try
            {
                switch (service.Status)
                {
                    case ServiceControllerStatus.Stopped:
                        this.btnStartService.Enabled = true;
                        break;
                    default:
                        // disable all buttons
                        this.btnStartService.Enabled = false;
                        break;        
                }
            }
            catch (Exception)
            {
                // Suppress error disable all buttons.
                this.btnStartService.Enabled = false;
            }
            finally
            {
                // Close the service controller to free up any resources.
                service.Close();
            }
        }

        /// <summary>
        /// Handles the tick event of the timer.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The event arguments.</param>
        private void TimerTick(object sender, EventArgs e)
        {
            this.DetermineIngresServiceState();
        }

        /// <summary>
        /// Handler for the click event of the toolbar button to start the Ingres service.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The event arguments.</param>
        private void StartIngresService(object sender, EventArgs e)
        {
            this.btnStartService.Enabled = false;

            MessageBox.Show("Attempting to start the Ingres service in the background.\n\nYou may continue to work...");

            Thread thread = new Thread(MainForm.StartIngresService);

            thread.Start();
        }
   }
}
