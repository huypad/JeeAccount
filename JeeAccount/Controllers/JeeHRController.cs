using JeeAccount.Models.JeeHR;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;

namespace JeeAccount.Controllers
{
    public class JeeHRController
    {
        public JeeHRController(string HOST_API_JEEHR)
        {
            _HOST_API_JEEHR = HOST_API_JEEHR;
        }

        private readonly string _HOST_API_JEEHR;
        private const string GET_DSNHANVIEN = "api/interaction/Get_DSNhanVien?more=true";
        private const string GET_DSNHANVIEN_THEOQUANLYTRUCTIEP = "api/interaction/getDSNhanVienTheoQLTT";
        private const string GET_COCAUTOCHUC = "api/interaction/getCoCauToChuc";

        public async Task<ReturnJeeHR<NhanVienJeeHR>> GetDSNhanVien(string access_token)
        {
            string url = $"{_HOST_API_JEEHR}/{GET_DSNHANVIEN}";

            using (var client = new HttpClient())
            {
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue(access_token);
                var reponse = await client.GetAsync(url);
                string returnValue = await reponse.Content.ReadAsStringAsync();
                var res = JsonConvert.DeserializeObject<ReturnJeeHR<NhanVienJeeHR>>(returnValue);
                return res;
            }
        }

        public async Task<ReturnJeeHR<NhanVienDuocQuanLyTrucTiep>> GetDSNhanVienTheoQuanLyTrucTiep(string access_token, long staffiD)
        {
            string url = $"{_HOST_API_JEEHR}/{GET_DSNHANVIEN_THEOQUANLYTRUCTIEP}?id={staffiD}";

            using (var client = new HttpClient())
            {
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue(access_token);
                var reponse = await client.GetAsync(url);
                string returnValue = await reponse.Content.ReadAsStringAsync();
                var res = JsonConvert.DeserializeObject<ReturnJeeHR<NhanVienDuocQuanLyTrucTiep>>(returnValue);
                return res;
            }
        }

        public async Task<ReturnJeeHR<JeeHRCoCauToChuc>> GetDSCoCauToChuc(string access_token)
        {
            string url = $"{_HOST_API_JEEHR}/{GET_COCAUTOCHUC}";

            using (var client = new HttpClient())
            {
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue(access_token);
                var reponse = await client.GetAsync(url);
                string returnValue = await reponse.Content.ReadAsStringAsync();
                var res = JsonConvert.DeserializeObject<ReturnJeeHR<JeeHRCoCauToChuc>>(returnValue);
                return res;
            }
        }
    }
}