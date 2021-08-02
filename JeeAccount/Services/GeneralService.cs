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

        public static string getlastname(string fullname)
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

        public static string getFirstname(string fullname)
        {
            if (fullname.Contains(' '))
            {
                string[] word = fullname.Split(' ');
                string firstName = word[word.Length - 1];
                return firstName;
            }
            return fullname;
        }

        public static IdentityServerReturn TranformIdentityServerReturnSqlModel(ReturnSqlModel returnSql)
        {
            IdentityServerReturn identity = new IdentityServerReturn();
            identity.statusCode = Int32.Parse(returnSql.ErrorCode);
            identity.message = returnSql.ErrorMessgage;
            return identity;
        }

        public static IdentityServerReturn TranformIdentityServerException(Exception ex)
        {
            IdentityServerReturn identity = new IdentityServerReturn();
            identity.statusCode = Int32.Parse(Constant.ERRORCODE_EXCEPTION);
            return identity;
        }

        private static Random random = new Random();

        public static bool IsExist(string sql, string connectionString)
        {
            using (DpsConnection cnn = new DpsConnection(connectionString))
            {
                DataTable dt = new DataTable();
                dt = cnn.CreateDataTable(sql);
                if (dt.Rows.Count == 0) return false;
                return true;
            }
        }

        public static bool IsExistCnn(string sql, DpsConnection cnn)
        {
            DataTable dt = new DataTable();
            dt = cnn.CreateDataTable(sql);
            if (dt.Rows.Count == 0) return false;
            return true;
        }

        public static bool IsExistCnn(string sql, DpsConnection cnn, SqlConditions conds)
        {
            DataTable dt = new DataTable();
            dt = cnn.CreateDataTable(sql, conds);
            if (dt.Rows.Count == 0) return false;
            return true;
        }

        public static string RandomString(int length)
        {
            const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789abcdefghjklmnoprstuwxyz@!@!@!";
            return new string(Enumerable.Repeat(chars, length)
              .Select(s => s[random.Next(s.Length)]).ToArray());
        }

        public static object GetUsernameByUserIDCnn(DpsConnection cnn, string UserID)
        {
            string sql = $"select username from AccountList where UserID = {UserID}";
            DataTable dt = new DataTable();
            dt = cnn.CreateDataTable(sql);
            if (dt.Rows.Count == 0) return null;
            if (string.IsNullOrEmpty(dt.Rows[0][0].ToString())) return null;
            return dt.Rows[0][0].ToString();
        }

        public static object GetUsernameByUserID(string connectionString, string UserID)
        {
            using (DpsConnection cnn = new DpsConnection(connectionString))
            {
                string sql = $"select username from AccountList where UserID = {UserID}";
                DataTable dt = new DataTable();
                dt = cnn.CreateDataTable(sql);
                if (dt.Rows.Count == 0) return null;
                if (string.IsNullOrEmpty(dt.Rows[0][0].ToString())) return null;
                return dt.Rows[0][0].ToString();
            }
        }

        public static UserNameDTO GetUsernameAndUserIDByStaffID(string connectionString, long Staffid)
        {
            using (DpsConnection cnn = new DpsConnection(connectionString))
            {
                string sql = $"select Username, UserID from AccountList where StaffID = {Staffid}";
                DataTable dt = new DataTable();
                dt = cnn.CreateDataTable(sql);
                return dt.AsEnumerable().Select(row => new UserNameDTO
                {
                    Username = row["Username"].ToString(),
                    UserId = Convert.ToInt64(row["UserID"]),
                    StaffID = Staffid
                }).SingleOrDefault();
            }
        }

        public static List<UserNameDTO> GetListUserNameDTOByCustomerid(string connectionString, long customerid)
        {
            using (DpsConnection cnn = new DpsConnection(connectionString))
            {
                string sql = $"select Username, UserID, StaffID from AccountList where CustomerID = {customerid}";
                DataTable dt = new DataTable();
                dt = cnn.CreateDataTable(sql);
                return dt.AsEnumerable().Select(row => new UserNameDTO
                {
                    Username = row["Username"].ToString(),
                    UserId = Convert.ToInt64(row["UserID"]),
                    StaffID = Convert.ToInt64(row["StaffID"])
                }).ToList<UserNameDTO>();
            }
        }

        public static List<UserNameDTO> GetListUserNameDTOByCustomeridCnn(DpsConnection cnn, long customerid)
        {
            string sql = $"select Username, UserID, StaffID from AccountList where CustomerID = {customerid}";
            DataTable dt = new DataTable();
            dt = cnn.CreateDataTable(sql);
            return dt.AsEnumerable().Select(row => new UserNameDTO
            {
                Username = row["Username"].ToString(),
                UserId = Convert.ToInt64(row["UserID"]),
                StaffID = Convert.ToInt64(row["StaffID"])
            }).ToList<UserNameDTO>();
        }

        public static UserNameDTO GetUsernameAndUserIDByStaffIDCnn(DpsConnection cnn, long Staffid)
        {
            string sql = $"select Username, UserID from AccountList where StaffID = {Staffid}";
            DataTable dt = new DataTable();
            dt = cnn.CreateDataTable(sql);
            return dt.AsEnumerable().Select(row => new UserNameDTO
            {
                Username = row["Username"].ToString(),
                UserId = Convert.ToInt64(row["UserID"]),
                StaffID = Staffid
            }).SingleOrDefault();
        }

        public static object GetCustomerIDByUserID(string connectionString, string UserID)
        {
            using (DpsConnection cnn = new DpsConnection(connectionString))
            {
                string sql = $"select CustomerID from AccountList where UserID = {UserID}";
                DataTable dt = new DataTable();
                dt = cnn.CreateDataTable(sql);
                if (dt.Rows.Count == 0) return null;
                if (string.IsNullOrEmpty(dt.Rows[0][0].ToString())) return null;
                return dt.Rows[0][0].ToString();
            }
        }

        public static object GetStaffIDByUserID(string connectionString, string UserID)
        {
            using (DpsConnection cnn = new DpsConnection(connectionString))
            {
                string sql = $"select StaffID from AccountList where UserID = {UserID}";
                DataTable dt = new DataTable();
                dt = cnn.CreateDataTable(sql);
                if (dt.Rows.Count == 0) return null;
                if (string.IsNullOrEmpty(dt.Rows[0][0].ToString())) return null;
                return dt.Rows[0][0].ToString();
            }
        }

        public static object GetUserIDByStaffID(string connectionString, string StaffID)
        {
            using (DpsConnection cnn = new DpsConnection(connectionString))
            {
                string sql = $"select UserID from AccountList where StaffID = {StaffID}";
                DataTable dt = new DataTable();
                dt = cnn.CreateDataTable(sql);
                if (dt.Rows.Count == 0) return null;
                if (string.IsNullOrEmpty(dt.Rows[0][0].ToString())) return null;
                return dt.Rows[0][0].ToString();
            }
        }

        public static object GetUserIDByStaffIDCnn(DpsConnection cnn, string StaffID)
        {
            string sql = $"select UserID from AccountList where StaffID = {StaffID}";
            DataTable dt = new DataTable();
            dt = cnn.CreateDataTable(sql);
            if (dt.Rows.Count == 0) return null;
            if (string.IsNullOrEmpty(dt.Rows[0][0].ToString())) return null;
            return dt.Rows[0][0].ToString();
        }

        public static bool CheckIsAdminByUserIDCnn(DpsConnection cnn, string UserId)
        {
            string sql = $"select * from AccountList where UserId = {UserId} and IsAdmin = 1";
            DataTable dt = new DataTable();
            dt = cnn.CreateDataTable(sql);
            if (dt.Rows.Count == 0) return false;
            return true;
        }

        public static bool CheckIsActiveByUserIDCnn(DpsConnection cnn, string UserId)
        {
            string sql = $"select * from AccountList where UserId = {UserId} and IsActive = 1";
            DataTable dt = new DataTable();
            dt = cnn.CreateDataTable(sql);
            if (dt.Rows.Count == 0) return false;
            return true;
        }

        public static bool CheckIsActiveByUsernameCnn(DpsConnection cnn, string username)
        {
            string sql = $"select * from AccountList where username = '{username}' and IsActive = 1";
            DataTable dt = new DataTable();
            dt = cnn.CreateDataTable(sql);
            if (dt.Rows.Count == 0) return false;
            return true;
        }

        public static bool CheckIsAdminByUsernameCnn(DpsConnection cnn, string username)
        {
            string sql = $"select * from AccountList where username = '{username} 'and IsAdmin = 1";
            DataTable dt = new DataTable();
            dt = cnn.CreateDataTable(sql);
            if (dt.Rows.Count == 0) return false;
            return true;
        }

        public static object GetStaffIDByUsername(string connectionString, string Username)
        {
            using (DpsConnection cnn = new DpsConnection(connectionString))
            {
                string sql = $"select StaffID from AccountList where Username = '{Username}'";
                DataTable dt = new DataTable();
                dt = cnn.CreateDataTable(sql);
                if (dt.Rows.Count == 0) return null;
                if (string.IsNullOrEmpty(dt.Rows[0][0].ToString())) return null;
                return dt.Rows[0][0].ToString();
            }
        }

        public static object GetCustomerIDByUsername(string connectionString, string Username)
        {
            using (DpsConnection cnn = new DpsConnection(connectionString))
            {
                string sql = $"select CustomerID from AccountList where Username = '{Username}'";
                DataTable dt = new DataTable();
                dt = cnn.CreateDataTable(sql);
                if (dt.Rows.Count == 0) return null;
                if (string.IsNullOrEmpty(dt.Rows[0][0].ToString())) return null;
                return dt.Rows[0][0].ToString();
            }
        }

        public static object GetUserIDByUsernameCnn(DpsConnection cnn, string Username)
        {
            string sql = $"select UserID from AccountList where Username = '{Username}'";
            DataTable dt = new DataTable();
            dt = cnn.CreateDataTable(sql);
            if (dt.Rows.Count == 0) return null;
            if (string.IsNullOrEmpty(dt.Rows[0][0].ToString())) return null;
            return Int32.Parse(dt.Rows[0][0].ToString());
        }

        public static object GetStaffIDByUserIDCnn(DpsConnection cnn, string UserID)
        {
            string sql = $"select StaffID from AccountList where UserID = {UserID}";
            DataTable dt = new DataTable();
            dt = cnn.CreateDataTable(sql);
            if (dt.Rows.Count == 0) return null;
            if (string.IsNullOrEmpty(dt.Rows[0][0].ToString())) return null;
            return dt.Rows[0][0].ToString();
        }

        public static object GetStaffIDByUsernameCnn(DpsConnection cnn, string Username)
        {
            string sql = $"select StaffID from AccountList where Username = '{Username}'";
            DataTable dt = new DataTable();
            dt = cnn.CreateDataTable(sql);
            if (dt.Rows.Count == 0) return null;
            if (string.IsNullOrEmpty(dt.Rows[0][0].ToString())) return null;
            return dt.Rows[0][0].ToString();
        }

        public static object GetUserIDByUsername(string connectionString, string Username)
        {
            using (DpsConnection cnn = new DpsConnection(connectionString))
            {
                string sql = $"select UserID from AccountList where Username = '{Username}'";
                DataTable dt = new DataTable();
                dt = cnn.CreateDataTable(sql);
                if (dt.Rows.Count == 0) return null;
                if (string.IsNullOrEmpty(dt.Rows[0][0].ToString())) return null;
                return Int32.Parse(dt.Rows[0][0].ToString());
            }
        }

        public static List<long> GetLstCustomerid(string connectionString)
        {
            using (DpsConnection cnn = new DpsConnection(connectionString))
            {
                var ListCustomerid = new List<long>();
                string sql = $"select RowID from CustomerList where RowID > 0";
                DataTable dt = new DataTable();
                dt = cnn.CreateDataTable(sql);
                if (dt.Rows.Count == 0) return null;
                foreach (DataRow dr in dt.Rows)
                {
                    ListCustomerid.Add(ConvertToLong(dr["RowID"]));
                }
                return ListCustomerid;
            }
        }

        public static List<long> GetLstCustomeridCnn(DpsConnection cnn)
        {
            var ListCustomerid = new List<long>();
            string sql = $"select RowID from CustomerList where RowID > 0";
            DataTable dt = new DataTable();
            dt = cnn.CreateDataTable(sql);
            if (dt.Rows.Count == 0) return null;
            foreach (DataRow dr in dt.Rows)
            {
                ListCustomerid.Add(ConvertToLong(dr["RowID"]));
            }
            return ListCustomerid;
        }

        public static List<long> GetLstUserIDByCustomerid(string connectionString, long customerid)
        {
            using (DpsConnection cnn = new DpsConnection(connectionString))
            {
                var ListCustomerid = new List<long>();
                string sql = $"select UserID from AccountList where CustomerID = {customerid}";
                DataTable dt = new DataTable();
                dt = cnn.CreateDataTable(sql);
                if (dt.Rows.Count == 0) return null;
                foreach (DataRow dr in dt.Rows)
                {
                    ListCustomerid.Add(ConvertToLong(dr["UserID"]));
                }
                return ListCustomerid;
            }
        }

        public static List<long> GetLstUserIDByCustomeridCnn(DpsConnection cnn, long customerid)
        {
            var ListCustomerid = new List<long>();
            string sql = $"select UserID from AccountList where CustomerID = {customerid}";
            DataTable dt = new DataTable();
            dt = cnn.CreateDataTable(sql);
            if (dt.Rows.Count == 0) return null;
            foreach (DataRow dr in dt.Rows)
            {
                ListCustomerid.Add(ConvertToLong(dr["UserID"]));
            }
            return ListCustomerid;
        }

        public static List<string> GetLstUsernameByCustomerid(string connectionString, long customerid)
        {
            using (DpsConnection cnn = new DpsConnection(connectionString))
            {
                var lst = new List<string>();
                string sql = $"select Username from AccountList where CustomerID = {customerid}";
                DataTable dt = new DataTable();
                dt = cnn.CreateDataTable(sql);
                if (dt.Rows.Count == 0) return null;
                foreach (DataRow dr in dt.Rows)
                {
                    lst.Add(dr["Username"].ToString());
                }
                return lst;
            }
        }

        public static List<string> GetLstUsernameByCustomeridCnn(DpsConnection cnn, long customerid)
        {
            var lst = new List<string>();
            string sql = $"select Username from AccountList where CustomerID = {customerid}";
            DataTable dt = new DataTable();
            dt = cnn.CreateDataTable(sql);
            if (dt.Rows.Count == 0) return null;
            foreach (DataRow dr in dt.Rows)
            {
                lst.Add(dr["Username"].ToString());
            }
            return lst;
        }

        public static bool IsUsedJeeHRCustomeridCnn(DpsConnection cnn, long customerid)
        {
            string sql = $"select * from Customer_App where AppID = 1 and CustomerID = {customerid}";
            DataTable dt = new DataTable();
            dt = cnn.CreateDataTable(sql);
            if (dt.Rows.Count == 0) return false;
            return true;
        }

        public static bool IsUsedJeeHRCustomerid(string connectionString, long customerid)
        {
            string sql = $"select * from Customer_App where AppID = 1 and CustomerID = {customerid}";
            using (DpsConnection cnn = new DpsConnection(connectionString))
            {
                DataTable dt = new DataTable();
                dt = cnn.CreateDataTable(sql);
                if (dt.Rows.Count == 0) return false;
            }
            return true;
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
            if (tentruochosau) return GetColorNameUser(getlastname(fullname).Substring(0, 1));
            return GetColorNameUser(getFirstname(fullname).Substring(0, 1));
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