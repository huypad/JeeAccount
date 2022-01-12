using DPSinfra.Logger;
using JeeAccount.Classes;
using JeeAccount.Controllers;
using JeeAccount.Models.Common;
using JeeAccount.Models.DepartmentManagement;
using JeeAccount.Models.JeeHR;
using JeeAccount.Reponsitories;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace JeeAccount.Services.DepartmentManagement
{
    public class DepartmentManagementService : IDepartmentManagementService
    {
        private readonly string _connectionString;
        private readonly IConfiguration _config;
        private readonly IDepartmentManagementReponsitory _reponsitory;
        private readonly string HOST_JEEHR_API;
        private readonly ILogger<DepartmentManagementService> _logger;
        public DepartmentManagementService(IConfiguration configuration, IDepartmentManagementReponsitory reponsitory, ILogger<DepartmentManagementService> logger)
        {
            _config = configuration;
            _connectionString = configuration.GetValue<string>("AppConfig:Connection");
            _reponsitory = reponsitory;
            HOST_JEEHR_API = configuration.GetValue<string>("Host:JeeHR_API");
            _logger = logger;
        }

        public void ChangeTinhTrang(long customerID, long RowID, string Note, long UserIdLogin)
        {
            _reponsitory.ChangeTinhTrang(customerID, RowID, Note, UserIdLogin);
        }

        public bool CheckDepartmentExist(long CustomerID, string connectionString)
        {
            return _reponsitory.CheckDepartmentExist(CustomerID, connectionString);
        }

        public void CreateDepartment(DepartmentModel departmentModel, long CustomerID, string Username)
        {
            _reponsitory.CreateDepartment(departmentModel, CustomerID, Username);
        }

        public void UpdateDepartment(DepartmentModel departmentModel, long CustomerID, string Username, bool isJeeHR)
        {
            _reponsitory.UpdateDepartment(departmentModel, CustomerID, Username, isJeeHR);
        }

        public void UpdateDepartmentManager(string UsernameModifiedBy, long customerID, string DirectManagerUsername, int departmemntID)
        {
            _reponsitory.UpdateDepartmentManager(UsernameModifiedBy, customerID, DirectManagerUsername, departmemntID);
        }

        public void DeleteDepartmentManager(string DeletedBy, long customerID, int departmemntID)
        {
            _reponsitory.DeleteDepartmentManager(DeletedBy, customerID, departmemntID);
        }

        public DepartmentModel GetDepartment(int rowid, long CustomerID)
        {
            return _reponsitory.GetDepartment(rowid, CustomerID);
        }

        public async Task<IEnumerable<DepartmentDTO>> GetListDepartmentDefaultAsync(long custormerID)
        {
            return await _reponsitory.GetListDepartmentDefaultAsync(custormerID).ConfigureAwait(false);
        }

        public async Task<IEnumerable<JeeHRCoCauToChucModelFromDB>> GetListDepartmentIsJeeHRAsync(long custormerID)
        {
            return await _reponsitory.GetListDepartmentIsJeeHRAsync(custormerID).ConfigureAwait(false);
        }

        #region GetDS Phong Ban

        public async Task<object> GetDSPhongBan(QueryParams query, long customerid, string token)
        {
            var traceLog = new GeneralLog()
            {
                name = "department",
                data = "",
                message = "GetDSPhongBan Open"
            };
            _logger.LogTrace(JsonConvert.SerializeObject(traceLog));

            query = query == null ? new QueryParams() : query;
            PageModel pageModel = new PageModel();

            string orderByStrDefault = "DepartmentList.RowID asc";
            string orderByStrJeeHR = "AccountList.DepartmentID asc";
            string whereStrDefault = " (DepartmentList.Disable != 1 or DepartmentList.Disable is null)";
            string whereStrJeeHR = " (AccountList.Disable != 1 or AccountList.Disable is null)";

            Dictionary<string, string> sortableFieldsDefault = new Dictionary<string, string>
                        {
                            { "phongbanid", "DepartmentList.RowID"},
                            { "phongban", "DepartmentList.DepartmentName"},
                            { "tinhtrang", "DepartmentList.IsActive"},
                            { "phongbanmanager", "DepartmentList.DepartmentManager"},
                        };

            Dictionary<string, string> sortableFieldsJeeHR = new Dictionary<string, string>
                        {
                            { "phongbanid", "AccountList.DepartmentID"},
                            { "phongban", "AccountList.DepartmentName"},
                        };

            var checkusedjeehr = GeneralReponsitory.IsUsedJeeHRCustomerid(_connectionString, customerid);

            if (!checkusedjeehr)
            {
                if (!string.IsNullOrEmpty(query.sortField) && sortableFieldsDefault.ContainsKey(query.sortField))
                {
                    orderByStrDefault = sortableFieldsDefault[query.sortField] + ("desc".Equals(query.sortOrder) ? " desc" : " asc");
                }

                whereStrDefault = CreateWhereStrDefault(query, whereStrDefault);
            }
            else
            {
                if (!string.IsNullOrEmpty(query.sortField) && sortableFieldsJeeHR.ContainsKey(query.sortField))
                {
                    orderByStrJeeHR = sortableFieldsJeeHR[query.sortField] + ("desc".Equals(query.sortOrder) ? " desc" : " asc");
                }

                whereStrJeeHR = CreateWhereStrJeeHR(query, whereStrJeeHR);
            }

            if (checkusedjeehr)
            {
                var traceLog2 = new GeneralLog()
                {
                    name = "department",
                    data = checkusedjeehr.ToString(),
                    message = "GetDSPhongBan checkusedjeehr"
                };
                _logger.LogTrace(JsonConvert.SerializeObject(traceLog2));

                var jeehrController = new JeeHRController(HOST_JEEHR_API);
                var list = await jeehrController.GetDSCoCauToChuc(token);
                var traceLog3 = new GeneralLog()
                {
                    name = "department",
                    data = JsonConvert.SerializeObject(list),
                    message = "GetDSPhongBan checkusedjeehr GetDSCoCauToChuc"
                };
                _logger.LogTrace(JsonConvert.SerializeObject(traceLog3));
                if (list.status == 1)
                {
                    var flat = TranferDataHelper.FlatListJeeHRCoCauToChuc(list.data);
                    flat = FilterLstFlatJeeHRCoCauToChuc(flat, query, sortableFieldsJeeHR);
                    pageModel.TotalCount = flat.Count;
                    if (flat.Count() == 0)
                    {
                        return await ReturnObjectGetListDepartmentIsJeeHRAsync(query, pageModel, customerid, whereStrJeeHR, orderByStrJeeHR, checkusedjeehr);
                    }
                    pageModel.AllPage = (int)Math.Ceiling(flat.Count / (decimal)query.record);
                    pageModel.Size = query.record;
                    pageModel.Page = query.page;
                    if (query.more)
                    {
                        query.page = 1;
                        pageModel.AllPage = 1;
                        pageModel.Size = 1;
                        query.record = pageModel.TotalCount;
                    }
                    flat = flat.Skip((query.page - 1) * query.record).Take(query.record).ToList();

                    var obj = new
                    {
                        tree = list.data,
                        flat = flat,
                        isTree = true,
                        isJeeHR = checkusedjeehr,
                    };

                    return new { data = obj, panigator = pageModel };
                }
                else
                {
                    return await ReturnObjectGetListDepartmentIsJeeHRAsync(query, pageModel, customerid, whereStrJeeHR, orderByStrJeeHR, checkusedjeehr);
                }
            }
            else
            {
                return await ReturnObjectDepartmentDefaultAsync(query, pageModel, customerid, whereStrDefault, orderByStrDefault, checkusedjeehr);
            }
        }

        private async Task<object> ReturnObjectDepartmentDefaultAsync(QueryParams query, PageModel pageModel, long customerid, string whereStrDefault, string orderByStrDefault, bool checkusedjeehr)
        {
            var traceLog3 = new GeneralLog()
            {
                name = "department",
                data = "",
                message = "ReturnObjectDepartmentDefaultAsync open"
            };
            _logger.LogTrace(JsonConvert.SerializeObject(traceLog3));

            var depart = await _reponsitory.GetListDepartmentDefaultAsync(customerid, whereStrDefault, orderByStrDefault);
            var traceLog1 = new GeneralLog()
            {
                name = "department",
                data = JsonConvert.SerializeObject(depart),
                message = "ReturnObjectDepartmentDefaultAsync open GetListDepartmentDefaultAsync"
            };
            _logger.LogTrace(JsonConvert.SerializeObject(traceLog1));


            pageModel.TotalCount = depart.Count();
            if (depart.Count() == 0) throw new KhongCoDuLieuException();
            pageModel.AllPage = (int)Math.Ceiling(depart.Count() / (decimal)query.record);
            pageModel.Size = query.record;
            pageModel.Page = query.page;
            if (query.more)
            {
                query.page = 1;
                pageModel.AllPage = 1;
                pageModel.Size = 1;
                query.record = pageModel.TotalCount;
            }
            depart = depart.Skip((query.page - 1) * query.record).Take(query.record).ToList();

            var obj = new
            {
                tree = DBNull.Value,
                flat = depart,
                isTree = false,
                isJeeHR = checkusedjeehr,
            };
            return new { data = obj, panigator = pageModel };
        }

        private async Task<object> ReturnObjectGetListDepartmentIsJeeHRAsync(QueryParams query, PageModel pageModel, long customerid, string whereStrJeeHR, string orderByStrJeeHR, bool checkusedjeehr)
        {

            var traceLog = new GeneralLog()
            {
                name = "department",
                data = "",
                message = "ReturnObjectGetListDepartmentIsJeeHRAsync Open"
            };
            _logger.LogTrace(JsonConvert.SerializeObject(traceLog));

            var depart = await _reponsitory.GetListDepartmentIsJeeHRAsync(customerid, whereStrJeeHR, orderByStrJeeHR).ConfigureAwait(false);

            var traceLog2 = new GeneralLog()
            {
                name = "department",
                data = "",
                message = "ReturnObjectGetListDepartmentIsJeeHRAsync After getdata"
            };
            _logger.LogTrace(JsonConvert.SerializeObject(traceLog2));

            pageModel.TotalCount = depart.Count();
            if (depart.Count() == 0) throw new KhongCoDuLieuException();
            pageModel.AllPage = (int)Math.Ceiling(depart.Count() / (decimal)query.record);
            pageModel.Size = query.record;
            pageModel.Page = query.page;
            if (query.more)
            {
                query.page = 1;
                pageModel.AllPage = 1;
                pageModel.Size = 1;
                query.record = pageModel.TotalCount;
            }
            depart = depart.Skip((query.page - 1) * query.record).Take(query.record).ToList();

            var objJeeHR = new
            {
                tree = DBNull.Value,
                flat = depart,
                isTree = false,
                isJeeHR = checkusedjeehr,
            };

            return new { data = objJeeHR, panigator = pageModel };
        }

        private string CreateWhereStrDefault(QueryParams query, string whereStrDefault)
        {
            if (!string.IsNullOrEmpty(query.filter["keyword"]))
            {
                whereStrDefault += $" and (AccountList.LastName + ' ' + AccountList.FirstName like N'%{query.filter["keyword"]}%' " +
 $"or DepartmentList.DepartmentName like N'%{query.filter["keyword"]}%')";
            }

            if (!string.IsNullOrEmpty(query.filter["phongban"]))
            {
                whereStrDefault += $" and (DepartmentList.DepartmentName like N'%{query.filter["phongban"]}%') ";
            }

            if (!string.IsNullOrEmpty(query.filter["phongbanid"]))
            {
                whereStrDefault += $" and (DepartmentList.RowID  in ({query.filter["phongbanid"]})) ";
            }

            if (!string.IsNullOrEmpty(query.filter["dakhoa"]))
            {
                if (Convert.ToBoolean(query.filter["dakhoa"]))
                {
                    whereStrDefault += $" and (DepartmentList.IsActive = 0) ";
                }
            }
            return whereStrDefault;
        }

        private string CreateWhereStrJeeHR(QueryParams query, string whereStrJeeHR)
        {
            if (!string.IsNullOrEmpty(query.filter["keyword"]))
            {
                whereStrJeeHR += $" and AccountList.DepartmentName like N'%{query.filter["keyword"]}%') ";
            }

            if (!string.IsNullOrEmpty(query.filter["phongban"]))
            {
                whereStrJeeHR += $" and (AccountList.DepartmentName like N'%{query.filter["phongban"]}%') ";
            }

            if (!string.IsNullOrEmpty(query.filter["phongbanid"]))
            {
                whereStrJeeHR += $" and (AccountList.DepartmentID  in ({query.filter["phongbanid"]})) ";
            }
            return whereStrJeeHR;
        }

        private List<FlatJeeHRCoCauToChucModel> FilterLstFlatJeeHRCoCauToChuc(List<FlatJeeHRCoCauToChucModel> lst, QueryParams query, Dictionary<string, string> sortableFieldsJeeHR)
        {
            if (!string.IsNullOrEmpty(query.filter["keyword"]))
            {
                lst = lst.AsEnumerable().Where(item => item.Title.Contains(query.filter["keyword"])).ToList();
            }

            if (!string.IsNullOrEmpty(query.filter["phongban"]))
            {
                lst = lst.AsEnumerable().Where(item => item.Title.Contains(query.filter["phongban"])).ToList();
            }

            if (!string.IsNullOrEmpty(query.filter["phongbanid"]))
            {
                lst = lst.AsEnumerable().Where(item => item.RowID.ToString() == query.filter["phongbanid"]).ToList();
            }
            if (!string.IsNullOrEmpty(query.sortField) && sortableFieldsJeeHR.ContainsKey(query.sortField))
            {
                if (query.sortField.Equals("phongbanid"))
                {
                    if ("desc".Equals(query.sortOrder))
                    {
                        lst = lst.AsEnumerable().OrderByDescending(item => item.RowID).ToList();
                    }
                }
                if (query.sortField.Equals("phongban"))
                {
                    if ("desc".Equals(query.sortOrder))
                    {
                        lst = lst.AsEnumerable().OrderByDescending(item => item.Title).ToList();
                    }
                    else
                    {
                        lst = lst.AsEnumerable().OrderBy(item => item.Title).ToList();
                    }
                }
            }
            return lst;
        }

        #endregion GetDS Phong Ban
    }
}