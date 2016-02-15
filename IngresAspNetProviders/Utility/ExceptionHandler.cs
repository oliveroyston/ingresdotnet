#region Code Header

/*  
 * Author               : Oliver P. Oyston (Luminary Solutions)
 * 
 * File Name            : ExceptionHandler.cs
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
    #region NameSpaces Used

    using System;
    using System.Configuration.Provider;
    using System.Diagnostics;
    using System.Text;
    using System.Web.Security;
    using Ingres.Client;

    #endregion

    #region ExceptionHandler

    /// <summary>
    /// <para>
    /// A helper class for handling exceptions that may occur during the course method execution in
    /// the Ingres ASP.NET Providers.
    /// </para>
    /// <para>
    /// When debugging we simply rethrow errors. When deployed we throw all errors except for the
    /// errors relating to Ingres. For the Ingres exceptions we generate a helpful report and
    /// write the details to the Windows logs before throwing a "friendly" error message back to
    /// the user.
    /// </para>
    /// </summary>
    internal sealed class ExceptionHandler
    {
        /// <summary>
        /// Generate a detailed report for an ingres exception.
        /// </summary>
        /// <param name="ex">The exception to generate a report for.</param>
        /// <param name="message">A custom message to include in the report.</param>
        /// <returns>A detailed exception report.</returns>
        public static string ExceptionDetailReport(IngresException ex, string message)
        {
            StringBuilder sb = new StringBuilder();

            sb.AppendLine("*****************************************************************************");
            sb.AppendLine(String.Format("Ingres Exception Detail Dump - Generated {0}", DateTime.Now));
            sb.AppendLine("*****************************************************************************\n");
            sb.AppendLine(message + "\n\n");
            sb.AppendLine(string.Format("Source      : {0}\n", ex.Source));
            sb.AppendLine(string.Format("Message     : {0}\n", ex.Message));
            sb.AppendLine(string.Format("Help Link   : {0}\n", ex.HelpLink));
            sb.AppendLine(string.Format("Stack Trace : {0}\n\n", ex.StackTrace));
           
            IngresErrorCollection errorCollection = ex.Errors;

            foreach (IngresError ingresError in errorCollection)
            {
                sb.Append(Environment.NewLine);
                sb.AppendLine(String.Format("Message      :  {0}", ingresError.Message));
                sb.AppendLine(String.Format("Native Error :  {0}", ingresError.NativeError));
                sb.AppendLine(String.Format("Number       :  {0}", ingresError.Number));
                sb.AppendLine(String.Format("Source       :  {0}", ingresError.Source));
                sb.AppendLine(String.Format("SQL State    :  {0}", ingresError.SQLState));
            }

            return sb.ToString();
        }

        /// <summary>
        /// Log an error for an Ingres ASP.NET provider.
        /// </summary>
        /// <param name="message">[IN] The event message.</param>
        /// <param name="providerType">The provider that we are logging the error for.</param>
        internal static void LogError(string message, IngresAspNetProvider providerType)
        {
            WriteEvent(message, EventLogEntryType.Error, providerType);
        }

        /// <summary>
        /// Log a warning for an Ingres ASP.NET provider.
        /// </summary>
        /// <param name="message">[IN] The event message.</param>
        /// <param name="providerType">The provider that we are logging a warning for.</param>
        internal static void LogWarning(string message, IngresAspNetProvider providerType)
        {
            WriteEvent(message, EventLogEntryType.Warning, providerType);
        }

        /// <summary>
        /// Log a rollback warning for a Role provider method.
        /// </summary>
        /// <param name="method">The method that we are logging a rollback warning for.</param>
        internal static void LogRollbackWarning(RoleProviderMethod method)
        {
            WriteEvent(Messages.ErrorRollingBackTransaction, EventLogEntryType.Warning, IngresAspNetProvider.Role);
        }

        /// <summary>
        /// Log a rollback warning for a Membership Provider method.
        /// </summary>
        /// <param name="method">The method that we are logging a rollback warning for.</param>
        internal static void LogRollbackWarning(MembershipProviderMethod method)
        {
            WriteEvent(Messages.ErrorRollingBackTransaction, EventLogEntryType.Warning, IngresAspNetProvider.Membership);
        }

        /// <summary>
        /// Log information for an Ingres ASP.NET provider.
        /// </summary>
        /// <param name="message">[IN] The event message.</param>
        /// <param name="providerType">The provider that we are logging the information for.</param>
        internal static void LogInformation(string message, IngresAspNetProvider providerType)
        {
            WriteEvent(message, EventLogEntryType.Information, providerType);
        }

        /// <summary>
        /// A generic error handler for exceptions that occur in the Ingres ASP.NET Role 
        /// provider.
        /// </summary>
        /// <param name="ex">The exception to handle.</param>
        /// <param name="method">The role provider method that caused the exception to be thrown.</param>
        internal static void HandleException(Exception ex, RoleProviderMethod method)
        {
            HandleException(ex, IngresAspNetProvider.Role, method);
        }

        /// <summary>
        /// A generic error handler for exceptions that occur in the Ingres ASP.NET Membership 
        /// provider.
        /// </summary>
        /// <param name="ex">The exception.</param>
        /// <param name="method">The method where the exception was thrown.</param>
        internal static void HandleException(Exception ex, MembershipProviderMethod method)
        {
            HandleException(ex, IngresAspNetProvider.Membership, method);
        }

        /// <summary>
        /// A generic error handling helper method.
        /// </summary>
        /// <remarks>
        /// If we have an IngresException then we pass off the handling to the HandleIngresException method.
        /// Otherwise we throw the exception if is a provider exception. If the exception is not a provider
        /// exception then we throw a provider exception with a generic message.
        /// </remarks>
        /// <param name="ex">The exception that we wish to handle.</param>
        /// <param name="provider">The provider that we are handling the exception for.</param>
        /// <param name="method">The method that caused the exception to be thrown.</param>
        private static void HandleException(Exception ex, IngresAspNetProvider provider, Enum method)
        {
            // Determine the error type and handle it appropriately.
            Type exceptionType = ex.GetType();

            // if the exception is an Ingres exception then we handle it differently...
            if (exceptionType == typeof(IngresException))
            {
                switch (provider)
                {
                    case IngresAspNetProvider.Role:
                        HandleIngresException((IngresException)ex, IngresAspNetProvider.Role, method.ToString());
                        break;
                    case IngresAspNetProvider.Membership:
                        HandleIngresException((IngresException)ex, IngresAspNetProvider.Membership, method.ToString());
                        break;
                }
            }
            else if (exceptionType == typeof(ArgumentException)     ||
                     exceptionType == typeof(ArgumentNullException) ||
                     exceptionType == typeof(ProviderException)     ||
                     exceptionType == typeof(MembershipPasswordException))
            {
                // re-thow errors if they are known errors that we have explicitly thrown (or the .NET 
                // framework throws on out behalf.)
                throw ex;
            }
            else
            {
                // re-wrap unknown errors as ProviderExceptions - keeping the original exception
                // as an inner exception.
                throw new ProviderException(String.Format(Messages.UnknownError), ex);
            }
        }

        /// <summary>
        /// A helper function that writes exception detail to the event log. Exceptions are written
        /// to the event log as a security measure to avoid private database details from being 
        /// returned to the browser. If a method does not return a status or boolean indicating the
        /// action succeeded or failed, a generic exception is also thrown by the caller.
        /// </summary>
        /// <param name="eventMessage">The message to write to the log.</param>
        /// <param name="type">The event log entry type.</param>
        /// <param name="provider">The provider that we are providing logging for.</param>
        private static void WriteEvent(string eventMessage, EventLogEntryType type, IngresAspNetProvider provider)
        {
            try
            {
                string source;

                switch (provider)
                {
                    case IngresAspNetProvider.Membership:
                        source = Messages.IngresMembershipProvider;
                        break;
                    case IngresAspNetProvider.Role:
                        source = Messages.IngresRoleProvider;
                        break;
                    default:
                        source = Messages.UnkownIngresAspNetProvider;
                        break;
                }

                const string Log = "log";

                // If the source for this application log entries is not present then attempt to create it.
                if (!EventLog.SourceExists(source))
                {
                    EventLog.CreateEventSource(source, Log);
                }

                // Write the entry if we could create the source.
                if (EventLog.SourceExists(source))
                {
                    EventLog.WriteEntry(source, eventMessage, type);
                }
            }
            catch
            {
                // suppress any errors in writing to the event logs
            }
        }

        /// <summary>
        /// Manages error handling when we encounter an IngresException. If we are debugging we
        /// simply rethro the exception - otherwise log the error and throw a "friendly" error to
        /// the user.
        /// </summary>
        /// <param name="ex">The Ingres exception to handle.</param>
        /// <param name="provider">The Ingres ASP.NET provider that caused the exception to be thrown.</param>
        /// <param name="detail">The detail of the exception.</param>
        private static void HandleIngresException(IngresException ex, IngresAspNetProvider provider, string detail)
        {
            #if DEBUG // The debugging constant is automatically handled by Visual Studio for us

            // When we are debugging we want to actually throw the errors rather than just writing the
            // details to the event log and throwing a generic provider error.
            throw ex;

            #else

            // This code becomes reachable when we are not in debug mode
            LogError(ExceptionDetailReport(ex, detail), provider);

            throw new ProviderException(Messages.IngresExceptionErrorMessage);

            #endif
        }
    }

    #endregion
}
