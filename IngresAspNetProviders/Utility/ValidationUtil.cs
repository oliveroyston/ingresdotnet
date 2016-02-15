#region Code Header

/*  
 * Author               : Oliver P. Oyston (Luminary Solutions)
 * 
 * File Name            : ValidationUtil.cs
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
    using System.Collections;
    using System.Globalization;
    using System.Text.RegularExpressions;

    #endregion

    #region Validation Utilities

    /// <summary>
    /// A helper class to use in validation of input parameters.
    /// </summary>
    internal sealed class ValidationUtil
    {
        /// <summary>
        /// Regex used in validing whether a string is a valid guid. Ingres doesn't support the
        /// Guid type natively but we still want to use Guids for unique identifiers.
        /// </summary>
        private static readonly Regex isGuid = new Regex(Messages.GuidRegex, RegexOptions.Compiled);

        /// <summary>
        /// Helper method to determine if a string is a valid Guid using regular expressions.
        /// </summary>
        /// <param name="candidateGuid">The candidtate Guid.</param>
        /// <returns>Whether the string represents a valid Guid.</returns>
        public static bool IsValidGuid(string candidateGuid)
        {
            bool isValid = false;

            if (candidateGuid != null)
            {
                if (isGuid.IsMatch(candidateGuid))
                {
                    isValid = true;
                }
            }

            return isValid;
        }

        /// <summary>
        /// Helper method to determine if a string is a valid email.
        /// </summary>
        /// <param name="email">The email to check.</param>
        /// <returns>Whether the provided email is valid.</returns>
        public static bool IsValidEmail(string email)
        {
            string strRegex = Messages.EmailRegex;

            Regex re = new Regex(strRegex);
            
            if (re.IsMatch(email))
            {
                return true;
            }

            return false;
        }

        /// <summary>
        /// Checks an input parameter to ensure that it appears to be valid.
        /// </summary>
        /// <param name="param">The parameter.</param>
        /// <param name="checkForNull">Whether to check if the parameter is null.</param>
        /// <param name="checkIfEmpty">Whether to check if the parameter is empty.</param>
        /// <param name="checkForCommas">Whether to check for commas in the parameter.</param>
        /// <param name="maxSize">The maximum allowed size of the parameter.</param>
        /// <param name="paramName">The name of the parameter to use in messages.</param>
        internal static void CheckParameterIsOK(ref string param, bool checkForNull, bool checkIfEmpty, bool checkForCommas, int maxSize, string paramName)
        {
            if (param == null)
            {
                if (checkForNull)
                {
                    throw new ArgumentNullException(paramName);
                }

                return;
            }

            param = param.Trim();

            if (checkIfEmpty && param.Length < 1)
            {
                throw new ArgumentException(string.Format(Messages.ParameterEmpty, paramName), paramName);
            }

            if (maxSize > 0 && param.Length > maxSize)
            {
                throw new ArgumentException(string.Format(Messages.ParameterTooLong, paramName, maxSize.ToString(CultureInfo.InvariantCulture)), paramName);
            }

            if (checkForCommas && param.Contains(","))
            {
                throw new ArgumentException(string.Format(Messages.ParameterContainsCommas, paramName), paramName);
            }
        }

        /// <summary>
        /// Checks an array input parameter to ensure that it appears to be valid.
        /// </summary>
        /// <param name="param">The parameter.</param>
        /// <param name="checkForNull">Whether to check if the parameter is null.</param>
        /// <param name="checkIfEmpty">Whether to check if the parameter is empty.</param>
        /// <param name="checkForCommas">Whether to check for commas in the parameter.</param>
        /// <param name="maxSize">The maximum allowed size of the parameter.</param>
        /// <param name="paramName">The name of the parameter to use in messages.</param>
        internal static void CheckArrayParameterIsOk(ref string[] param, bool checkForNull, bool checkIfEmpty, bool checkForCommas, int maxSize, string paramName)
        {
            if (param == null)
            {
                throw new ArgumentNullException(paramName);
            }

            if (param.Length < 1)
            {
                throw new ArgumentException(String.Format(Messages.ParameterArrayEmpty,  paramName), paramName);
            }

            Hashtable values = new Hashtable(param.Length);

            for (int i = param.Length - 1; i >= 0; i--)
            {
                CheckParameterIsOK(ref param[i], checkForNull, checkIfEmpty, checkForCommas, maxSize, paramName + "[ " + i.ToString(CultureInfo.InvariantCulture) + " ]");

                if (values.Contains(param[i]))
                {
                    throw new ArgumentException(String.Format(Messages.DuplicateArrayElement, paramName), paramName);
                }
                
                values.Add(param[i], param[i]);
            }
        }

        /// <summary>
        /// Validates a parameter.
        /// </summary>
        /// <param name="param">The parameter.</param>
        /// <param name="checkForNull">Whether to check if the parameter is null.</param>
        /// <param name="checkIfEmpty">Whether to check if the parameter is empty.</param>
        /// <param name="checkForCommas">Whether to check for commas in the parameter.</param>
        /// <param name="maxSize">The maximum allowed size of the parameter.</param>
        /// <returns>Whether the parameter is valid.</returns>
        internal static bool IsParameterValid(ref string param, bool checkForNull, bool checkIfEmpty, bool checkForCommas, int maxSize)
        {
            if (param == null)
            {
                return !checkForNull;
            }

            param = param.Trim();

            if ((checkIfEmpty && param.Length < 1) || (maxSize > 0 && param.Length > maxSize) || (checkForCommas && param.Contains(",")))
            {
                return false;
            }

            return true;
        }
    }
    #endregion
}
