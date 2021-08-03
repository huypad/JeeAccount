using DpsLibs.Data;
using JeeAccount.Classes;
using JeeAccount.Models;
using JeeAccount.Models.AccountManagement;
using JeeAccount.Models.Common;
using JeeAccount.Models.JeeHR;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Reflection;

namespace JeeAccount.Services
{
    public static class GeneralService
    {
        private static Random random = new Random();

        public static string CreateListStringWhereIn(List<string> ListStringData)
        {
            string result = "";
            foreach (string data in ListStringData)
            {
                if (string.IsNullOrEmpty(result))
                {
                    result = $"'{data}'";
                }
                else
                {
                    result += $", '{data}'";
                }
            }
            return result;
        }

        public static Aspose.Imaging.Image RezizeImage(Aspose.Imaging.Image img, int maxHeight)
        {
            if (img.Height < maxHeight) return img;
            Double yRatio = (double)img.Height / maxHeight;
            Double ratio = yRatio;
            int nnx = (int)Math.Floor(img.Width / ratio);
            int nny = (int)Math.Floor(img.Height / ratio);
            img.Resize(nnx, nny);
            return img;
        }

        public static string Getlastname(string fullname)
        {
            if (fullname.Contains(' '))
            {
                string[] word = fullname.Split(' ');
                string lastName = word[0];
                for (var index = 1; index < word.Length - 1; index++)
                {
                    lastName += " " + word[index];
                }
                return lastName;
            }
            return fullname;
        }

        public static string GetFirstname(string fullname)
        {
            if (fullname.Contains(' '))
            {
                string[] word = fullname.Split(' ');
                string firstName = word[word.Length - 1];
                return firstName;
            }
            return fullname;
        }

        public static List<T> ConvertDataTableToList<T>(this DataTable dt)
        {
            List<T> data = new List<T>();
            foreach (DataRow row in dt.Rows)
            {
                T item = GetItem<T>(row);
                data.Add(item);
            }
            return data;
        }

        public static T GetItem<T>(DataRow dr)
        {
            Type temporary = typeof(T);
            T obj = Activator.CreateInstance<T>();

            foreach (DataColumn column in dr.Table.Columns)
            {
                foreach (PropertyInfo property in temporary.GetProperties())
                {
                    if (string.Equals(property.Name, column.ColumnName, StringComparison.OrdinalIgnoreCase))
                    {
                        var data = dr[column.ColumnName];
                        if (data != DBNull.Value)
                        {
                            try
                            {
                                var safeValue = dr[column.ColumnName] == null ? null : Convert.ChangeType(dr[column.ColumnName], data.GetType());
                                property.SetValue(obj, safeValue, null);
                            }
                            catch (Exception ex)
                            {
                                throw new Exception($"{ex.Message} ({column.ColumnName})");
                            }
                        }
                        break;
                    }
                    else
                        continue;
                }
            }
            return obj;
        }

        public static string GetColorFullNameUser(string fullname, bool tentruochosau = false)
        {
            if (tentruochosau) return GetColorNameUser(Getlastname(fullname).Substring(0, 1));
            return GetColorNameUser(GetFirstname(fullname).Substring(0, 1));
        }

        public static string GetColorNameUser(string name)
        {
            var result = "";
            switch (name)
            {
                case "A":
                    return result = "rgb(51, 152, 219)";

                case "Ă":
                    return result = "rgb(241, 196, 15)";

                case "Â":
                    return result = "rgb(142, 68, 173)";

                case "B":
                    return result = "#0cb929";

                case "C":
                    return result = "rgb(91, 101, 243)";

                case "D":
                    return result = "rgb(44, 62, 80)";

                case "Đ":
                    return result = "rgb(127, 140, 141)";

                case "E":
                    return result = "rgb(26, 188, 156)";

                case "Ê":
                    return result = "rgb(51, 152, 219)";

                case "G":
                    return result = "rgb(241, 196, 15)";

                case "H":
                    return result = "rgb(248, 48, 109)";

                case "I":
                    return result = "rgb(142, 68, 173)";

                case "K":
                    return result = "#2209b7";

                case "L":
                    return result = "rgb(44, 62, 80)";

                case "M":
                    return result = "rgb(127, 140, 141)";

                case "N":
                    return result = "rgb(197, 90, 240)";

                case "O":
                    return result = "rgb(51, 152, 219)";

                case "Ô":
                    return result = "rgb(241, 196, 15)";

                case "Ơ":
                    return result = "rgb(142, 68, 173)";

                case "P":
                    return result = "#02c7ad";

                case "Q":
                    return result = "rgb(211, 84, 0)";

                case "R":
                    return result = "rgb(44, 62, 80)";

                case "S":
                    return result = "rgb(127, 140, 141)";

                case "T":
                    return result = "#bd3d0a";

                case "U":
                    return result = "rgb(51, 152, 219)";

                case "Ư":
                    return result = "rgb(241, 196, 15)";

                case "V":
                    return result = "#759e13";

                case "X":
                    return result = "rgb(142, 68, 173)";

                case "W":
                    return result = "rgb(211, 84, 0)";
            }
            return result;
        }

        public static string ConvertDateToString(object value, bool includeHoursAndMinutes = false)
        {
            DateTime? dt = (value == System.DBNull.Value)
                ? (DateTime?)null
                : Convert.ToDateTime(value);

            if (dt == null)
                return string.Empty;
            return value == null ? string.Empty : Convert.ToDateTime(dt).ConvertDate(includeHoursAndMinutes);
        }

        public static string ConvertToString(object value)
        {
            return Convert.ToString(ReturnEmptyIfNull(value));
        }

        public static int ConvertToInt(object value)
        {
            return Convert.ToInt32(ReturnZeroIfNull(value));
        }

        public static long ConvertToLong(object value)
        {
            return Convert.ToInt64(ReturnZeroIfNull(value));
        }

        public static decimal ConvertToDecimal(object value)
        {
            return Convert.ToDecimal(ReturnZeroIfNull(value));
        }

        public static DateTime convertToDateTime(object date)
        {
            return Convert.ToDateTime(ReturnDateTimeMinIfNull(date));
        }

        public static string ConvertDate(this DateTime datetTime, bool includeHoursAndMinutes = false)
        {
            if (datetTime != DateTime.MinValue)
            {
                if (!includeHoursAndMinutes)
                    return datetTime.ToString("dd/MM/yyyy");
                return datetTime.ToString("dd/MM/yyyy HH:mm:ss.fff");
            }
            return null;
        }

        public static object ReturnEmptyIfNull(this object value)
        {
            if (value == DBNull.Value)
                return string.Empty;
            if (value == null)
                return string.Empty;
            return value;
        }

        public static object ReturnZeroIfNull(this object value)
        {
            if (value == DBNull.Value)
                return 0;
            if (value == null)
                return 0;
            return value;
        }

        public static object ReturnDateTimeMinIfNull(this object value)
        {
            if (value == DBNull.Value)
                return DateTime.MinValue;
            if (value == null)
                return DateTime.MinValue;
            return value;
        }
    }
}