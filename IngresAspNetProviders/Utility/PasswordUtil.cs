#region Code Header

/*  
 * Author               : Oliver P. Oyston (Luminary Solutions)
 * 
 * File Name            : PasswordUtil.cs
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
    using System.Security.Cryptography;

    #endregion

    /// <summary>
    /// A helper class for working with passwords.
    /// </summary>
    internal static class PasswordUtil
    {
        /// <summary>
        /// A helper method to return a salt to be used in the hashing of passwords
        /// </summary>
        /// <param name="length">The length of the salt that we wish to return.</param>
        /// <returns>A password salt.</returns>
        public static string GetSalt(int length)
        {
            // Create and populate random byte array
            byte[] array = new byte[length];

            // Create random salt and convert to string
            RNGCryptoServiceProvider rng = new RNGCryptoServiceProvider();

            rng.GetBytes(array);

            string salt = Convert.ToBase64String(array);

            return salt;
        }
    }
}
