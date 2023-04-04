using System;

namespace Yun.Common
{
    public class ConvertClass
    {
        public static Boolean BooleanWrapper(object o)
        {
            if (o == null)
                return false;
            if (o == DBNull.Value)
                return false;

            if (o is int)
            {
                if (IntegerWrapper(o) > 0)
                    return true;
                else
                    return false;
            }

            bool bVal;

            if (bool.TryParse(o.ToString(), out bVal))
                return bVal;
            else
                return false;
        }

        /// <summary>
        /// check the type of value first, then parse it.
        /// </summary>
        /// <param name="value"></param>
        /// <returns>null or DBNull.Value ,0
        ///                 value is not int type, 0</returns>
        public static int IntegerWrapper(object value)
        {
            if (value == null || value == DBNull.Value)
                return 0;

            if (value is bool)
            {
                return (bool)value == true ? 1 : 0;
            }
            else if (value is float || value is double)
            {
                return (int)((double)value);
            }
            else if (value is System.Enum)
            {
                return (int)value;
            }
            else
            {
                int iVal;
                if (int.TryParse(value.ToString(), out iVal))
                    return iVal;
                else
                    return 0;
            }
        }

        /// <summary>
        /// convert datatype to UInt32
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public static UInt32 UInt32Wrapper(object value)
        {
            if (value == null || value == DBNull.Value)
                return 0;

            if (value is bool)
            {
                return (bool)value == true ? (UInt32)1 : 0;
            }
            else if (value is float || value is double)
            {
                return (UInt32)((double)value);
            }
            else if (value is System.Enum)
            {
                return (UInt32)value;
            }
            else
            {
                UInt32 iVal;
                if (UInt32.TryParse(value.ToString(), out iVal))
                    return iVal;
                else
                    return 0;
            }
        }

        public static long longWrapper(object o)
        {
            if (o == null)
                return 0;
            if (o == DBNull.Value)
                return 0;

            long iVal;

            if (Int64.TryParse(o.ToString(), out iVal))
                return iVal;
            else
                return 0;
        }

        public static string StringWrapper(object o)
        {
            if (o == null) return string.Empty;
            if (o == DBNull.Value) return string.Empty;

            return o.ToString();
        }

        public static DateTime MiniDateTime
        {
            get
            {
                return new DateTime(1900, 1, 1);
            }
        }

        public const long InvalidLong = -10000000;

        public const decimal InvalidDecimal = -10000000;

        public static DateTime? DateTickWrapper(object obj)
        {
            var tick = longWrapper(obj);
            if (tick <= 0)
                return null;
            try
            {
                DateTime tmp = new DateTime(tick);
                return tmp;
            }
            catch (Exception)
            {
                return MiniDateTime;
            }
        }

        public static DateTime DateWrapper(object o)
        {
            if (o == null) return MiniDateTime;
            if (o == DBNull.Value) return MiniDateTime;

            DateTime d;
            if (DateTime.TryParse(o.ToString(), out d))
                return d;
            else
                return MiniDateTime;
        }

        public static decimal DecimalWrapper(object o, decimal defaultValue = 0M)
        {
            if (o == null)
                return defaultValue;
            if (o == DBNull.Value)
                return defaultValue;

            decimal dVal;

            if (decimal.TryParse(o.ToString(), out dVal))
                return dVal;
            else
                return defaultValue;
        }

        public static float SingleWrapper(object o)
        {
            if (o == null)
                return 0;
            if (o == DBNull.Value)
                return 0;

            float fVal;

            if (float.TryParse(o.ToString(), out fVal))
                return fVal;
            else
                return 0;
        }

        public static double DoubleWrapper(object o)
        {
            if (o == null)
                return 0;
            if (o == DBNull.Value)
                return 0;

            double dVal;

            if (double.TryParse(o.ToString(), out dVal))
                return dVal;
            else
                return 0;
        }

        #region used for code auto generator

        public static Int16 Int16Wrapper(object value)
        {
            if (value == null || value == DBNull.Value)
                return 0;

            if (value is bool)
            {
                if ((bool)value == true)
                    return 1;
                else
                    return 0;
            }
            else if (value is float || value is double)
            {
                return (Int16)((double)value);
            }
            else if (value is System.Enum)
            {
                return (Int16)value;
            }
            else
            {
                Int16 iVal;
                if (Int16.TryParse(value.ToString(), out iVal))
                    return iVal;
                else
                    return 0;
            }
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public static int Int32Wrapper(object value)
        {
            if (value == null || value == DBNull.Value)
                return 0;

            if (value is bool)
            {
                return (bool)value == true ? 1 : 0;
            }
            else if (value is float || value is double)
            {
                return (int)((double)value);
            }
            else if (value is System.Enum)
            {
                return (int)value;
            }
            else
            {
                int iVal;
                if (int.TryParse(value.ToString(), out iVal))
                    return iVal;
                else
                    return 0;
            }
        }

        public static long Int64Wrapper(object o)
        {
            if (o == null)
                return 0;
            if (o == DBNull.Value)
                return 0;

            long iVal;

            if (Int64.TryParse(o.ToString(), out iVal))
                return iVal;
            else
                return 0;
        }

        public static DateTime DateTimeWrapper(object o)
        {
            if (o == null) return MiniDateTime;
            if (o == DBNull.Value) return MiniDateTime;

            DateTime d;
            if (DateTime.TryParse(o.ToString(), out d))
                return d;
            else
                return MiniDateTime;
        }

        #endregion used for code auto generator
    }
}