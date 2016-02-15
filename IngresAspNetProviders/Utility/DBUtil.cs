#region Code Header
/*  
 * Author               : Les Benson (Luminary Solutions)
 * 
 * File Name            : DBUtil.cs
 *
 * Copyright            : (c) Luminary Solutions Limited
 * 
 * Version History
 *
 * Version  Date        Who     Description
 * -------  ----------  ---     --------------
 * 1.0      31/08/2006  ljb     Original Luminary Version.
 * 1.1      20/12/2007  ljb     Additional helper functions for nullable GUIDs
 * 1.2      04/11/2008  opo     Converted to an internal class for use within the Ingres ASP.NET
 *                              Providers project. Amended layout and code styling to pass StyleCop.
 *                              validation.
*/
#endregion

namespace Ingres.Web.Security.Utility
{
    #region NameSpaces Used

    using System;
    using System.Data;

    #endregion

    /// <summary>
    /// A set of database helper functions to make the extraction of columns easier within code.
    /// </summary>
    /// <remarks>
    /// The class cannot be derived from.
    /// </remarks>
    internal static class DBUtil
    {
        #region Class Methods

        #region DBNull Helper Methods
        /// <summary>
        /// Helper function to return whether column is null.
        /// </summary>
        /// <param name="dr">The data reader.</param>
        /// <param name="columnIndex">The ordinal column index.</param>
        /// <returns>The string value of the column.</returns>
        public static bool IsColValNull(IDataReader dr, int columnIndex)
        {
            return dr.IsDBNull(columnIndex);
        }

        /// <summary>
        /// Helper function to return whether column is null.
        /// </summary>
        /// <param name="dr">The data reader.</param>
        /// <param name="columnName">The column name.</param>
        /// <returns>The string value of the column.</returns>
        public static bool IsColValNull(IDataReader dr, string columnName)
        {
            return IsColValNull(dr, dr.GetOrdinal(columnName));
        }
        #endregion
        
        #region String Helper Methods

        /// <summary>
        /// Helper function to return a string column using a supplied column 
        /// ordinal index from a data reader and a value to return if the column is null.
        /// </summary>
        /// <param name="dr">The data reader.</param>
        /// <param name="columnIndex">The ordinal column index.</param>
        /// <param name="defaultResult">The default value if the column is null.</param>
        /// <returns>The string value of the column.</returns>
        public static string ColValAsStringNullableDefault(IDataReader dr, int columnIndex, string defaultResult)
        {
            return dr.IsDBNull(columnIndex) ? defaultResult : dr.GetString(columnIndex);
        }

        /// <summary>
        /// Helper function to return a string column using a supplied column 
        /// ordinal index from a data reader.
        /// </summary>
        /// <param name="dr">The data reader.</param>
        /// <param name="columnIndex">The ordinal column index.</param>
        /// <returns>The string value of the column.</returns>
        public static string ColValAsString(IDataReader dr, int columnIndex)
        {
            return ColValAsStringNullableDefault(dr, columnIndex, string.Empty);
        }

        /// <summary>
        /// Helper function to return a string column using a supplied column 
        /// ordinal index from a data reader.
        /// </summary>
        /// <param name="dr">The data reader.</param>
        /// <param name="columnIndex">The ordinal column index.</param>
        /// <param name="defaultResult">The default value if the column could not be found.</param>
        /// <returns>The string value of the column.</returns>
        public static string ColValAsString(IDataReader dr, int columnIndex, string defaultResult)
        {
            return dr.GetSchemaTable().Columns.Count - 1 >= columnIndex ?
                                                                            ColValAsStringNullableDefault(dr, columnIndex, defaultResult) : defaultResult;
        }

        /// <summary>
        /// Helper function to return an string column using a supplied column 
        /// name from a data reader.
        /// </summary>
        /// <param name="dr">The data reader.</param>
        /// <param name="columnName">The column name.</param>
        /// <returns>The string value of the column.</returns>
        public static string ColValAsString(IDataReader dr, string columnName)
        {
            return ColValAsString(dr, dr.GetOrdinal(columnName));
        }

        /// <summary>
        /// Helper function to return an string column using a supplied column 
        /// name from a data reader.
        /// </summary>
        /// <param name="dr">The data reader.</param>
        /// <param name="columnName">The column name.</param>
        /// <param name="defaultResult">The default value if the column could not be found.</param>
        /// <returns>The string value of the column.</returns>
        public static string ColValAsString(IDataReader dr, string columnName, string defaultResult)
        {
            int ordinal = -1;

            try
            {
                ordinal = dr.GetOrdinal(columnName);
                return ColValAsString(dr, dr.GetOrdinal(columnName), defaultResult);
            }
            catch (Exception)
            {
                return defaultResult;
            }
        }
        #endregion
        
        #region Int16 Helper Methods
        /// <summary>
        /// Helper function to return a short column using a supplied column 
        /// ordinal index from a data reader.
        /// </summary>
        /// <param name="dr">The data reader.</param>
        /// <param name="columnIndex">The ordinal column index.</param>
        /// <returns>The date time value of the column or null.</returns>
        public static Nullable<short> ColValAsNullableInt16(IDataReader dr, int columnIndex)
        {
            return dr.IsDBNull(columnIndex) ? (Nullable<short>)null : dr.GetInt16(columnIndex);
        }

        /// <summary>
        /// Helper function to return a short column using a supplied column 
        /// name from a data reader.
        /// </summary>
        /// <param name="dr">The data reader.</param>
        /// <param name="columnName">The column name.</param>
        /// <returns>The date time value of the column or null.</returns>
        public static Nullable<short> ColValAsNullableInt16(IDataReader dr, string columnName)
        {
            return ColValAsNullableInt16(dr, dr.GetOrdinal(columnName));
        }

        /// <summary>
        /// Helper function to return an short column using a supplied column 
        /// ordinal index from a data reader.
        /// </summary>
        /// <param name="dr">The data reader.</param>
        /// <param name="columnIndex">The ordinal column index.</param>
        /// <returns>The short integer value of the column.</returns>
        public static short ColValAsInt16(IDataReader dr, int columnIndex)
        {
            return dr.IsDBNull(columnIndex) ? ((short)0) : dr.GetInt16(columnIndex);
        }

        /// <summary>
        /// Helper function to return a short column using a supplied column 
        /// ordinal index from a data reader.
        /// </summary>
        /// <param name="dr">The data reader.</param>
        /// <param name="columnIndex">The ordinal column index.</param>
        /// <param name="defaultResult">The default value if the column could not be found.</param>
        /// <returns>The short integer value of the column.</returns>
        public static short ColValAsInt16(IDataReader dr, int columnIndex, short defaultResult)
        {
            return dr.GetSchemaTable().Columns.Count - 1 >= columnIndex ?
                                                                            ColValAsInt16(dr, columnIndex) : defaultResult;
        }

        /// <summary>
        /// Helper function to return a short column using a supplied column 
        /// name from a data reader.
        /// </summary>
        /// <param name="dr">The data reader.</param>
        /// <param name="columnName">The column name.</param>
        /// <returns>The short integer value of the column.</returns>
        public static short ColValAsInt16(IDataReader dr, string columnName)
        {
            return ColValAsInt16(dr, dr.GetOrdinal(columnName));
        }

        /// <summary>
        /// Helper function to return a short column using a supplied column 
        /// name from a data reader.
        /// </summary>
        /// <param name="dr">The data reader.</param>
        /// <param name="columnName">The column name.</param>
        /// <param name="defaultResult">The default value if the column could not be found.</param>
        /// <returns>The int value of the column.</returns>
        public static short ColValAsInt16(IDataReader dr, string columnName, short defaultResult)
        {
            return dr.GetSchemaTable().Columns.Contains(columnName) ?
                                                                        ColValAsInt16(dr, dr.GetOrdinal(columnName)) : defaultResult;
        }
        #endregion
        
        #region Int32 Helper Methods

        /// <summary>
        /// Helper function to return a int column using a supplied column 
        /// ordinal index from a data reader.
        /// </summary>
        /// <param name="dr">The data reader.</param>
        /// <param name="columnIndex">The ordinal column index.</param>
        /// <returns>The date time value of the column or null.</returns>
        public static Nullable<int> ColValAsNullableInt32(IDataReader dr, int columnIndex)
        {
            return dr.IsDBNull(columnIndex) ? (Nullable<int>)null : dr.GetInt32(columnIndex);
        }

        /// <summary>
        /// Helper function to return a int column using a supplied column 
        /// name from a data reader.
        /// </summary>
        /// <param name="dr">The data reader.</param>
        /// <param name="columnName">The column name.</param>
        /// <returns>The date time value of the column or null.</returns>
        public static Nullable<int> ColValAsNullableInt32(IDataReader dr, string columnName)
        {
            return ColValAsNullableInt32(dr, dr.GetOrdinal(columnName));
        }

        /// <summary>
        /// Helper function to return an integer column using a supplied column 
        /// ordinal index from a data reader.
        /// </summary>
        /// <param name="dr">The data reader.</param>
        /// <param name="columnIndex">The ordinal column index.</param>
        /// <returns>The integer value of the column.</returns>
        public static int ColValAsInt32(IDataReader dr, int columnIndex)
        {
            return dr.IsDBNull(columnIndex) ? 0 : dr.GetInt32(columnIndex);
        }

        /// <summary>
        /// Helper function to return a integer column using a supplied column 
        /// ordinal index from a data reader.
        /// </summary>
        /// <param name="dr">The data reader.</param>
        /// <param name="columnIndex">The ordinal column index.</param>
        /// <param name="defaultResult">The default value if the column could not be found.</param>
        /// <returns>The int value of the column.</returns>
        public static int ColValAsInt32(IDataReader dr, int columnIndex, int defaultResult)
        {
            return dr.GetSchemaTable().Columns.Count - 1 >= columnIndex ?
                                                                            ColValAsInt32(dr, columnIndex) : defaultResult;
        }

        /// <summary>
        /// Helper function to return an integer column using a supplied column 
        /// name from a data reader.
        /// </summary>
        /// <param name="dr">The data reader.</param>
        /// <param name="columnName">The column name.</param>
        /// <returns>The integer value of the column.</returns>
        public static int ColValAsInt32(IDataReader dr, string columnName)
        {
            return ColValAsInt32(dr, dr.GetOrdinal(columnName));
        }

        /// <summary>
        /// Helper function to return an integer column using a supplied column 
        /// name from a data reader.
        /// </summary>
        /// <param name="dr">The data reader.</param>
        /// <param name="columnName">The column name.</param>
        /// <param name="defaultResult">The default value if the column could not be found.</param>
        /// <returns>The int value of the column.</returns>
        public static int ColValAsInt32(IDataReader dr, string columnName, int defaultResult)
        {
            return dr.GetSchemaTable().Columns.Contains(columnName) ?
                                                                        ColValAsInt32(dr, dr.GetOrdinal(columnName)) : defaultResult;
        }
        #endregion
        
        #region Int64 Helper Methods

        /// <summary>
        /// Helper function to return a long column using a supplied column 
        /// ordinal index from a data reader.
        /// </summary>
        /// <param name="dr">The data reader.</param>
        /// <param name="columnIndex">The ordinal column index.</param>
        /// <returns>The date time value of the column or null.</returns>
        public static Nullable<long> ColValAsNullableInt64(IDataReader dr, int columnIndex)
        {
            return dr.IsDBNull(columnIndex) ? (Nullable<long>)null : dr.GetInt64(columnIndex);
        }

        /// <summary>
        /// Helper function to return a long column using a supplied column 
        /// name from a data reader.
        /// </summary>
        /// <param name="dr">The data reader.</param>
        /// <param name="columnName">The column name.</param>
        /// <returns>The date time value of the column or null.</returns>
        public static Nullable<long> ColValAsNullableInt64(IDataReader dr, string columnName)
        {
            return ColValAsNullableInt64(dr, dr.GetOrdinal(columnName));
        }

        /// <summary>
        /// Helper function to return an long column using a supplied column 
        /// ordinal index from a data reader.
        /// </summary>
        /// <param name="dr">The data reader.</param>
        /// <param name="columnIndex">The ordinal column index.</param>
        /// <returns>The large integer value of the column.</returns>
        public static long ColValAsInt64(IDataReader dr, int columnIndex)
        {
            return dr.IsDBNull(columnIndex) ? ((long)0) : dr.GetInt64(columnIndex);
        }

        /// <summary>
        /// Helper function to return a long column using a supplied column 
        /// ordinal index from a data reader.
        /// </summary>
        /// <param name="dr">The data reader.</param>
        /// <param name="columnIndex">The ordinal column index.</param>
        /// <param name="defaultResult">The default value if the column could not be found.</param>
        /// <returns>The large integer value of the column.</returns>
        public static long ColValAsInt64(IDataReader dr, int columnIndex, long defaultResult)
        {
            return dr.GetSchemaTable().Columns.Count - 1 >= columnIndex ?
                                                                            ColValAsInt64(dr, columnIndex) : defaultResult;
        }

        /// <summary>
        /// Helper function to return a long column using a supplied column 
        /// name from a data reader.
        /// </summary>
        /// <param name="dr">The data reader.</param>
        /// <param name="columnName">The column name.</param>
        /// <returns>The large integer value of the column.</returns>
        public static long ColValAsInt64(IDataReader dr, string columnName)
        {
            return ColValAsInt64(dr, dr.GetOrdinal(columnName));
        }

        /// <summary>
        /// Helper function to return a long column using a supplied column 
        /// name from a data reader.
        /// </summary>
        /// <param name="dr">The data reader.</param>
        /// <param name="columnName">The column name.</param>
        /// <param name="defaultResult">The default value if the column could not be found.</param>
        /// <returns>The int value of the column.</returns>
        public static long ColValAsInt64(IDataReader dr, string columnName, long defaultResult)
        {
            return dr.GetSchemaTable().Columns.Contains(columnName) ?
                                                                        ColValAsInt64(dr, dr.GetOrdinal(columnName)) : defaultResult;
        }
        #endregion

        #region Decimal Helper Methods
        /// <summary>
        /// Helper function to return a decimal column using a supplied column 
        /// ordinal index from a data reader.
        /// </summary>
        /// <param name="dr">The data reader.</param>
        /// <param name="columnIndex">The ordinal column index.</param>
        /// <returns>The date time value of the column or null.</returns>
        public static Nullable<decimal> ColValAsNullableDecimal(IDataReader dr, int columnIndex)
        {
            return dr.IsDBNull(columnIndex) ? (Nullable<decimal>)null : dr.GetDecimal(columnIndex);
        }

        /// <summary>
        /// Helper function to return a short column using a supplied column 
        /// name from a data reader.
        /// </summary>
        /// <param name="dr">The data reader.</param>
        /// <param name="columnName">The column name.</param>
        /// <returns>The date time value of the column or null.</returns>
        public static Nullable<decimal> ColValAsNullableDecimal(IDataReader dr, string columnName)
        {
            return ColValAsNullableDecimal(dr, dr.GetOrdinal(columnName));
        }

        /// <summary>
        /// Helper function to return an decimal column using a supplied column 
        /// ordinal index from a data reader.
        /// </summary>
        /// <param name="dr">The data reader.</param>
        /// <param name="columnIndex">The ordinal column index.</param>
        /// <returns>The decimal value of the column.</returns>
        public static decimal ColValAsDecimal(IDataReader dr, int columnIndex)
        {
            return dr.IsDBNull(columnIndex) ? 0 : dr.GetDecimal(columnIndex);
        }

        /// <summary>
        /// Helper function to return an decimal column using a supplied column 
        /// name from a data reader.
        /// </summary>
        /// <param name="dr">The data reader.</param>
        /// <param name="columnName">The column name.</param>
        /// <returns>The decimal value of the column.</returns>
        public static decimal ColValAsDecimal(IDataReader dr, string columnName)
        {
            return ColValAsDecimal(dr, dr.GetOrdinal(columnName));
        }
        #endregion

        #region Boolean Helper Methods
        /// <summary>
        /// Helper function to return a bool column using a supplied column 
        /// ordinal index from a data reader.
        /// </summary>
        /// <param name="dr">The data reader.</param>
        /// <param name="columnIndex">The ordinal column index.</param>
        /// <returns>The date time value of the column or null.</returns>
        public static Nullable<bool> ColValAsNullableBool(IDataReader dr, int columnIndex)
        {
            return dr.IsDBNull(columnIndex) ? (Nullable<bool>)null : dr.GetBoolean(columnIndex);
        }

        /// <summary>
        /// Helper function to return a bool column using a supplied column 
        /// name from a data reader.
        /// </summary>
        /// <param name="dr">The data reader.</param>
        /// <param name="columnName">The column name.</param>
        /// <returns>The date time value of the column or null.</returns>
        public static Nullable<bool> ColValAsNullableBool(IDataReader dr, string columnName)
        {
            return ColValAsNullableBool(dr, dr.GetOrdinal(columnName));
        }

        /// <summary>
        /// Helper function to return a boolean column using a supplied column 
        /// ordinal index from a data reader.
        /// </summary>
        /// <param name="dr">The data reader.</param>
        /// <param name="columnIndex">The ordinal column index.</param>
        /// <returns>The boolean value of the column.</returns>
        public static bool ColValAsBool(IDataReader dr, int columnIndex)
        {
            return dr.IsDBNull(columnIndex) ? false : dr.GetBoolean(columnIndex);
        }

        /// <summary>
        /// Helper function to return a boolean column using a supplied column 
        /// name from a data reader.
        /// </summary>
        /// <param name="dr">The data reader.</param>
        /// <param name="columnName">The column name.</param>
        /// <returns>The boolean value of the column.</returns>
        public static bool ColValAsBool(IDataReader dr, string columnName)
        {
            return ColValAsBool(dr, dr.GetOrdinal(columnName));
        }
        #endregion
        
        #region Char Helper Methods
        /// <summary>
        /// Helper function to return a char column using a supplied column 
        /// ordinal index from a data reader.
        /// </summary>
        /// <param name="dr">The data reader.</param>
        /// <param name="columnIndex">The ordinal column index.</param>
        /// <returns>The date time value of the column or null.</returns>
        public static Nullable<char> ColValAsNullableChar(IDataReader dr, int columnIndex)
        {
            return dr.IsDBNull(columnIndex) ? (Nullable<char>)null : dr.GetChar(columnIndex);
        }

        /// <summary>
        /// Helper function to return a char column using a supplied column 
        /// name from a data reader.
        /// </summary>
        /// <param name="dr">The data reader.</param>
        /// <param name="columnName">The column name.</param>
        /// <returns>The date time value of the column or null.</returns>
        public static Nullable<char> ColValAsNullableChar(IDataReader dr, string columnName)
        {
            return ColValAsNullableChar(dr, dr.GetOrdinal(columnName));
        }

        /// <summary>
        /// Helper function to return a char column using a supplied column 
        /// ordinal index from a data reader.
        /// </summary>
        /// <param name="dr">The data reader.</param>
        /// <param name="columnIndex">The ordinal column index.</param>
        /// <returns>The char value of the column.</returns>
        public static char ColValAsChar(IDataReader dr, int columnIndex)
        {
            return dr.IsDBNull(columnIndex) ? ' ' : dr.GetChar(columnIndex);
        }

        /// <summary>
        /// Helper function to return a char column using a supplied column 
        /// name from a data reader.
        /// </summary>
        /// <param name="dr">The data reader.</param>
        /// <param name="columnName">The column name.</param>
        /// <returns>The char value of the column.</returns>
        public static char ColValAsChar(IDataReader dr, string columnName)
        {
            return ColValAsChar(dr, dr.GetOrdinal(columnName));
        }
        #endregion
        
        #region DateTime Helper Methods
        /// <summary>
        /// Helper function to return a date time column using a supplied column 
        /// ordinal index from a data reader.
        /// </summary>
        /// <param name="dr">The data reader.</param>
        /// <param name="columnIndex">The ordinal column index.</param>
        /// <returns>The date time value of the column or null.</returns>
        public static Nullable<DateTime> ColValAsNullableDateTime(IDataReader dr, int columnIndex)
        {
            return dr.IsDBNull(columnIndex) ? (Nullable<DateTime>)null : dr.GetDateTime(columnIndex);
        }

        /// <summary>
        /// Helper function to return a date time column using a supplied column 
        /// name from a data reader.
        /// </summary>
        /// <param name="dr">The data reader.</param>
        /// <param name="columnName">The column name.</param>
        /// <returns>The date time value of the column or null.</returns>
        public static Nullable<DateTime> ColValAsNullableDateTime(IDataReader dr, string columnName)
        {
            return ColValAsNullableDateTime(dr, dr.GetOrdinal(columnName));
        }

        /// <summary>
        /// Helper function to return a date time column 
        /// using a supplied column ordinal index from a data reader.
        /// </summary>
        /// <param name="dr">The data reader.</param>
        /// <param name="columnIndex">The ordinal column index.</param>
        /// <returns>The date time representation of the date time column or null.</returns>
        public static DateTime ColValAsDateTime(IDataReader dr, int columnIndex)
        {
            return dr.IsDBNull(columnIndex) ? DateTime.MinValue : dr.GetDateTime(columnIndex);
        }

        /// <summary>
        /// Helper function to return date time column 
        /// using a supplied column name from a data reader.
        /// </summary>
        /// <param name="dr">The data reader.</param>
        /// <param name="columnName">The column name.</param>
        /// <returns>The date time representation of the date time column or null.</returns>
        public static DateTime ColValAsDateTime(IDataReader dr, string columnName)
        {
            return ColValAsDateTime(dr, dr.GetOrdinal(columnName));
        }

        /// <summary>
        /// Helper function to return a string representation of a date time column 
        /// using a supplied column ordinal index from a data reader.
        /// </summary>
        /// <param name="dr">The data reader.</param>
        /// <param name="columnIndex">The ordinal column index.</param>
        /// <returns>The string representation of the date time column or null.</returns>
        public static string ColValDateTime(IDataReader dr, int columnIndex)
        {
            return dr.IsDBNull(columnIndex) ? string.Empty : dr.GetDateTime(columnIndex).ToString();
        }

        /// <summary>
        /// Helper function to return a string representation of an date time column 
        /// using a supplied column name from a data reader.
        /// </summary>
        /// <param name="dr">The data reader.</param>
        /// <param name="columnName">The column name.</param>
        /// <returns>The string representation of the date time column or null.</returns>
        public static string ColValDateTime(IDataReader dr, string columnName)
        {
            return ColValDateTime(dr, dr.GetOrdinal(columnName));
        }
        #endregion
        
        #region Byte Helper Methods
        /// <summary>
        /// Helper function to return a byte array column using a supplied column 
        /// ordinal index from a data reader.
        /// </summary>
        /// <param name="dr">The data reader.</param>
        /// <param name="columnIndex">The ordinal column index.</param>
        /// <returns>The byte value of the column.</returns>
        public static byte ColValAsByte(IDataReader dr, int columnIndex)
        {
            return dr.IsDBNull(columnIndex) ? new byte() : dr.GetByte(columnIndex);
        }

        /// <summary>
        /// Helper function to return a char column using a supplied column 
        /// name from a data reader.
        /// </summary>
        /// <param name="dr">The data reader.</param>
        /// <param name="columnName">The column name.</param>
        /// <returns>The byte value of the column.</returns>
        public static byte ColValAsByte(IDataReader dr, string columnName)
        {
            return ColValAsByte(dr, dr.GetOrdinal(columnName));
        }
        #endregion
        
        #region Guid Helper Methods
        /// <summary>
        /// Helper function to return a guid column using a supplied column 
        /// ordinal index from a data reader.
        /// </summary>
        /// <param name="dr">The data reader.</param>
        /// <param name="columnIndex">The ordinal column index.</param>
        /// <returns>The guid value of the column.</returns>
        public static Guid ColValAsGuid(IDataReader dr, int columnIndex)
        {
            return dr.IsDBNull(columnIndex) ? new Guid() : dr.GetGuid(columnIndex);
        }

        /// <summary>
        /// Helper function to return a nullable guid column using a supplied column 
        /// name from a data reader.
        /// </summary>
        /// <param name="dr">The data reader.</param>
        /// <param name="columnName">The column name.</param>
        /// <returns>The guid value of the column.</returns>
        public static Guid ColValAsGuid(IDataReader dr, string columnName)
        {
            return ColValAsGuid(dr, dr.GetOrdinal(columnName));
        }

        /// <summary>
        /// Helper function to return a nullable guid column using a supplied column 
        /// ordinal index from a data reader.
        /// </summary>
        /// <param name="dr">The data reader.</param>
        /// <param name="columnIndex">The ordinal column index.</param>
        /// <returns>The guid value of the column.</returns>
        public static Nullable<Guid> ColValAsNullableGuid(IDataReader dr, int columnIndex)
        {
            return dr.IsDBNull(columnIndex) ? (Nullable<Guid>)null : dr.GetGuid(columnIndex);
        }

        /// <summary>
        /// Helper function to return a nullable guid column using a supplied column 
        /// name from a data reader.
        /// </summary>
        /// <param name="dr">The data reader.</param>
        /// <param name="columnName">The column name.</param>
        /// <returns>The guid value of the column.</returns>
        public static Nullable<Guid> ColValAsNullableGuid(IDataReader dr, string columnName)
        {
            return ColValAsNullableGuid(dr, dr.GetOrdinal(columnName));
        }
        #endregion

        #endregion
    }
}