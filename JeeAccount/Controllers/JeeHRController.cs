using DPSinfra.Logger;
using JeeAccount.Models.JeeHR;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using RestSharp;
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
        public JeeHRController(string HOST_API_JEEHR, ILogger<JeeHRController> logger)
        {
            _HOST_API_JEEHR = HOST_API_JEEHR;
            _logger = logger;
        }

        private readonly string _HOST_API_JEEHR;
        private const string GET_DSNHANVIEN = "api/interaction/Get_DSNhanVien?more=true";
        private const string GET_DSNHANVIEN_THEOQUANLYTRUCTIEP = "api/interaction/getDSNhanVienTheoQLTT";
        private const string GET_COCAUTOCHUC = "api/interaction/getCoCauToChuc";
        private const string GET_CHUCVUCHUCDANH = "api/interaction/getDSChucVuTheoChucDanh";
        private readonly ILogger<JeeHRController> _logger;

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

        public List<NhanVienJeeHR> GetDSNhanVienNotAysnc(string access_token)
        {
            string url = $"{_HOST_API_JEEHR}/{GET_DSNHANVIEN}";

            var client = new RestClient(url);
            var request = new RestRequest(Method.GET);

            request.AddHeader("Authorization", access_token);
            IRestResponse response = client.Execute(request);
            var res = JsonConvert.DeserializeObject<ReturnJeeHR<NhanVienJeeHR>>(response.Content);
            if (res.status == 0) throw new Exception("Trong kafka không thể lấy được danh sách nhân viên JeeHR");
            return res.data;
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
            var traceLog2 = new GeneralLog()
            {
                name = "department",
                data = "",
                message = "GetDSPhongBan checkusedjeehr GetDSCoCauToChuc"
            };
            _logger.LogTrace(JsonConvert.SerializeObject(traceLog2));

            string url = $"{_HOST_API_JEEHR}/{GET_COCAUTOCHUC}";

            using (var client = new HttpClient())
            {
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue(access_token);
                var reponse = await client.GetAsync(url);
                string returnValue = await reponse.Content.ReadAsStringAsync();
                var traceLog3 = new GeneralLog()
                {
                    name = "department",
                    data = JsonConvert.SerializeObject(returnValue),
                    message = "GetDSPhongBan checkusedjeehr GetDSCoCauToChuc returnValue"
                };
                _logger.LogTrace(JsonConvert.SerializeObject(traceLog3));
                var res = JsonConvert.DeserializeObject<ReturnJeeHR<JeeHRCoCauToChuc>>(returnValue);
                return res;
            }
        }

        public async Task<ReturnJeeHR<JeeHRChucVu>> GetDSChucVu(string access_token, string cocauid, string chucdanh)
        {
            string url = $"{_HOST_API_JEEHR}/{GET_CHUCVUCHUCDANH}?cocauid={cocauid}&chucdanhid={chucdanh}";

            using (var client = new HttpClient())
            {
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue(access_token);
                var reponse = await client.GetAsync(url);
                string returnValue = await reponse.Content.ReadAsStringAsync();
                var res = JsonConvert.DeserializeObject<ReturnJeeHR<JeeHRChucVu>>(returnValue);
                return res;
            }
        }
    }
}