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

        public async Task<ReturnJeeHR<NhanVienJeeHR>> GetDSNhanVien(string access_token)
        {
            string url = $"{_HOST_API_JEEHR}/{GET_DSNHANVIEN}";

            using (var client = new HttpClient())
            {
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue(access_token);
                var reponse = await client.GetAsync(url);
                string returnValue = reponse.Content.ReadAsStringAsync().Result;
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
                string returnValue = reponse.Content.ReadAsStringAsync().Result;
                var res = JsonConvert.DeserializeObject<ReturnJeeHR<NhanVienDuocQuanLyTrucTiep>>(returnValue);
                return res;
            }
        }

        public List<NhanVienDuocQuanLyTrucTiep_> ConverNhanVienDuocQuanLyTrucTiep_(List<NhanVienDuocQuanLyTrucTiep> lst)
        {
            var lstNhanVienDuocQuanLyTrucTiep_ = new List<NhanVienDuocQuanLyTrucTiep_>();

            foreach (var nv in lst)
            {
                var obj = new NhanVienDuocQuanLyTrucTiep_(nv);
                lstNhanVienDuocQuanLyTrucTiep_.Add(obj);
            }
            return lstNhanVienDuocQuanLyTrucTiep_;
        }
    }
}