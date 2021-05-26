using JeeAccount.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.IdentityModel.Tokens;
using Microsoft.Net.Http.Headers;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace JeeAccount.Classes
{
    public static class Ulities
    {
        public static string Remove_Multiple_Space(string p_keyword)
        {
            try
            {
                return Regex.Replace(p_keyword.Trim(), @"\s+", " ", RegexOptions.Multiline);
            }
            catch (Exception ex)
            {
                return p_keyword;
            }
        }

        public static string Remove_All_Space(string p_keyword)
        {
            try
            {
                return Regex.Replace(p_keyword.Trim(), @"\s+", "", RegexOptions.Multiline);
            }
            catch (Exception ex)
            {
                return p_keyword;
            }
        }

        public static string RandomString(int length)
        {
            Random random = new Random();

            string chars1 = "ABCDEFGHIJKLMNOPQRSTUVWXYZ";
            var Str1 = new string(Enumerable.Repeat(chars1, 1)
              .Select(s => s[random.Next(s.Length)]).ToArray());

            string chars2 = "0123456789";
            var Str2 = new string(Enumerable.Repeat(chars2, 1)
              .Select(s => s[random.Next(s.Length)]).ToArray());

            string chars3 = "!@#$%";
            var Str3 = new string(Enumerable.Repeat(chars3, 1)
              .Select(s => s[random.Next(s.Length)]).ToArray());

            string chars4 = "abcdefghijklmnopqrstvwxyz";
            var Str4 = new string(Enumerable.Repeat(chars4, 5)
              .Select(s => s[random.Next(s.Length)]).ToArray());

            return Str1 + Str2 + Str3 + Str4;
        }

        /// <summary>
        /// Format string follow length input and concat '...'
        /// sample: 123456789
        /// lenght input = 5
        /// output: 12345...
        /// </summary>
        /// <param name="k"></param>
        /// <param name="pLength"></param>
        /// <returns></returns>
        public static string TruncateString_ConcatDot(string k, int pLength = 100)
        {
            try
            {
                if (k.Length > pLength)
                    k = k.Substring(0, pLength) + "...";
                return k;
            }
            catch (Exception ex)
            {
                return k;
            }
        }

        /// <summary>
        /// Format string follow length input
        /// sample: 123456789
        /// length input = 7 , length in database is 7
        /// output: 1234... => length = 3
        /// </summary>
        /// <param name="k"></param>
        /// <param name="pLength"></param>
        /// <returns></returns>
        public static string TruncateString(string k, int pLength = 100)
        {
            try
            {
                if (k.Length > pLength)
                    k = k.Substring(0, pLength > 3 ? pLength - 3 : pLength) + "...";
                return k;
            }
            catch (Exception ex)
            {
                return k;
            }
        }

        /// <summary>
        /// Remove all unicode of string
        /// Sample:
        /// Input = "Thế giới hòa binh"
        /// Output = "The gioi hoa binh"
        /// </summary>
        /// <param name="text">Input string</param>
        /// <returns></returns>
        public static string RemoveUnicodeFromStr(string text)
        {
            if (string.IsNullOrWhiteSpace(text))
                return text;

            text = text.Normalize(NormalizationForm.FormD);
            var chars = text.Where(c => CharUnicodeInfo.GetUnicodeCategory(c) != UnicodeCategory.NonSpacingMark).ToArray();
            return new string(chars).Normalize(NormalizationForm.FormC);
        }

        //public static List<string> GET_EXTENSION_UPLOAD_FILE()
        //{
        //    List<string> v_str = new List<string>();
        //    string s = ConfigurationManager_HT.AppSettings.EXTENSION_UPLOAD_FILE;
        //    if (s != null)
        //        if (s.Length > 0)
        //            v_str = s.Split(',').ToList();
        //    v_str.ForEach(x => x.Trim());
        //    return v_str;
        //}

        /// <summary>
        /// Remove all special Character in a string
        /// </summary>
        /// <param name="input"></param>
        /// <param name="char_replTo"></param>
        /// <returns></returns>
        public static string RemoveAllSpecialChar(string input, string char_replTo = "")
        {
            try
            {
                return Regex.Replace(input, @"[^0-9a-zA-Z]+", char_replTo);
            }
            catch (Exception ex)
            {
                return input;
            }
        }

        /// <summary>
        /// Get user by header
        /// </summary>
        /// <param name="pHeader"></param>
        /// <returns></returns>
        public static CustomData GetUserByHeader(IHeaderDictionary pHeader)
        {
            try
            {
                if (pHeader == null) return null;
                if (!pHeader.ContainsKey(HeaderNames.Authorization)) return null;

                IHeaderDictionary _d = pHeader;
                string _bearer_token, _user;
                _bearer_token = _d[HeaderNames.Authorization].ToString().Replace("Bearer ", "");
                var handler = new JwtSecurityTokenHandler();
                var tokenS = handler.ReadToken(_bearer_token) as JwtSecurityToken;

                _user = tokenS.Claims.Where(x => x.Type == Constant._User).FirstOrDefault().Value;
                if (string.IsNullOrEmpty(_user))
                    return null;

                var customData = JsonConvert.DeserializeObject<CustomData>(_user);
                return customData;
            }
            catch (Exception ex)
            {
                return null;
            }
        }

        /// <summary>
        /// Get user by header
        /// </summary>
        /// <param name="pHeader"></param>
        /// <returns></returns>
        public static object GetProjectnameByHeader(IHeaderDictionary pHeader, string signingKey)
        {
            try
            {
                if (pHeader == null) return null;
                if (!pHeader.ContainsKey(HeaderNames.Authorization)) return null;

                IHeaderDictionary _d = pHeader;
                string _bearer_token, _projectname;
                _bearer_token = _d[HeaderNames.Authorization].ToString().Replace("Bearer ", "");

                SecurityToken validatedToken;
                JwtSecurityTokenHandler handler = new JwtSecurityTokenHandler();
                TokenValidationParameters validationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = false,
                    ValidateAudience = false,
                    ValidateLifetime = false,
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(signingKey)),
                };

                var tokenS = handler.ValidateToken(_bearer_token, validationParameters, out validatedToken);
                _projectname = tokenS.Claims.Where(x => x.Type == "projectName").FirstOrDefault().Value;
                if (string.IsNullOrEmpty(_projectname))
                    return null;

                return _projectname;
            }
            catch (Exception ex)
            {
                return null;
            }
        }

        public static SecurityToken validationToken(string token, string signingKey)
        {
            SecurityToken validatedToken;
            JwtSecurityTokenHandler handler = new JwtSecurityTokenHandler();
            TokenValidationParameters validationParameters = new TokenValidationParameters
            {
                ValidateIssuer = false,
                ValidateAudience = false,
                ValidateLifetime = false,
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(signingKey)),
            };
            var user = handler.ValidateToken(token, validationParameters, out validatedToken);
            return validatedToken;
        }

        public static string GetAccessTokenByHeader(IHeaderDictionary pHeader)
        {
            try
            {
                if (pHeader == null) return null;
                if (!pHeader.ContainsKey(HeaderNames.Authorization)) return null;

                IHeaderDictionary _d = pHeader;
                string _bearer_token, _user;
                _bearer_token = _d[HeaderNames.Authorization].ToString().Replace("Bearer ", "");

                return _bearer_token;
            }
            catch (Exception ex)
            {
                return null;
            }
        }
    }
}