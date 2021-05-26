using DpsLibs.Data;
using JeeAccount.Models.Common;
using JeeAccount.Models.StructureManagement;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;

namespace JeeAccount.Reponsitories
{
    public class StructureManagementReponsitory : IStructureManagementReponsitory
    {
        private readonly string _connectionString;

        public StructureManagementReponsitory(IConfiguration configuration)
        {
            _connectionString = configuration.GetConnectionString("DefaultConnection");
        }

        public async Task<IEnumerable<StructureDTO>> GetOrgStructure([FromQuery] QueryParams query)
        {
            DataTable dt = new DataTable();
            SqlConditions Conds = new SqlConditions();
            //Conds.Add("CustomerID", custormerID);
            string sqlq = @"select dv1.[Id]
                                          , dv1.[LoaiDonVi]
                                          ,dv1.[DonVi]
                                          ,dv1.[MaDonvi]
                                          ,dv1.[MaDinhDanh]
                                          ,dv1.[Parent]
                                          ,dv1.[SDT]
                                          ,dv1.[Email]
                                          ,dv1.[DiaChi]
                                          ,dv1.[Logo]
                                          ,dv1.[Locked]
                                          ,dv1.[Priority]
                                          ,dv1.[DangKyLichLanhDao]
                                          ,dv1.[KhongCoVanThu]
                                          ,dv1.[CreatedBy]
                                          ,dv1.[CreatedDate]
                                          ,dv1.[UpdatedBy]
                                          ,dv1.[UpdatedDate]
                                          ,dv1.[Disabled]
                                            ,dv2.DonVi as ParentName
                                           from Cocautochuc dv1
                                            left
                                           join Cocautochuc dv2 on dv1.Parent = dv2.Id
                                          where dv1.[Disabled] = 0 and(dv1.Parent = @IdDV or dv1.Id = @IdDV)";
            using (DpsConnection cnn = new DpsConnection(_connectionString))
            {
                if (!string.IsNullOrEmpty(query.filter["IdDV"]))
                {
                    Conds.Add("IdDV", long.Parse(query.filter["IdDV"]));
                }
                else
                {
                    //Conds.Add("IdDV", long.Parse(_config.IdTinh));
                    Conds.Add("IdDV", 1); // gán tạm
                }
                dt = cnn.CreateDataTable(sqlq, Conds);
                var temp = dt.AsEnumerable();
                if (!string.IsNullOrEmpty(query.filter["LoaiDonVi"]))
                {
                    long keyword = long.Parse(query.filter["LoaiDonVi"]);
                    temp = temp.Where(x => x["LoaiDonVi"].Equals(keyword));
                }
                if (!string.IsNullOrEmpty(query.filter["DonVi"]))
                {
                    string keyword = query.filter["DonVi"].ToLower();
                    temp = temp.Where(x => x["DonVi"].ToString().ToLower().Contains(keyword));
                }
                if (!string.IsNullOrEmpty(query.filter["MaDinhDanh"]))
                {
                    string keyword = query.filter["MaDinhDanh"].ToLower();
                    temp = temp.Where(x => x["MaDinhDanh"].ToString().ToLower().Contains(keyword));
                }
                if (!string.IsNullOrEmpty(query.filter["MaDonvi"]))
                {
                    string keyword = query.filter["MaDonvi"].ToLower();
                    temp = temp.Where(x => x["MaDonvi"].ToString().ToLower().Contains(keyword));
                }
                if (!string.IsNullOrEmpty(query.filter["SDT"]))
                {
                    string keyword = query.filter["SDT"].ToLower();
                    temp = temp.Where(x => x["SDT"].ToString().ToLower().Contains(keyword));
                }
                if (!string.IsNullOrEmpty(query.filter["Email"]))
                {
                    string keyword = query.filter["Email"].ToLower();
                    temp = temp.Where(x => x["Email"].ToString().ToLower().Contains(keyword));
                }
                if (!string.IsNullOrEmpty(query.filter["DiaChi"]))
                {
                    string keyword = query.filter["DiaChi"].ToLower();
                    temp = temp.Where(x => x["DiaChi"].ToString().ToLower().Contains(keyword));
                }
                var result = dt.AsEnumerable().Select(row => new StructureDTO
                {
                    Id = long.Parse(row["ID"].ToString()),
                    DonVi = row["DonVi"].ToString(),
                    MaDonvi = row["MaDonvi"].ToString(),
                    MaDinhDanh = row["MaDinhDanh"].ToString(),
                    Parent = long.Parse(row["Parent"].ToString()),
                    SDT = row["SDT"].ToString(),
                    Email = row["Email"].ToString(),
                    DiaChi = row["DiaChi"].ToString(),
                    Logo = row["Logo"].ToString(),
                    DangKyLichLanhDao = Convert.ToBoolean((bool)row["DangKyLichLanhDao"]),
                    KhongCoVanThu = Convert.ToBoolean((bool)row["KhongCoVanThu"]),
                    LoaiDonVi = long.Parse(row["LoaiDonVi"].ToString()),
                    Priority = long.Parse(row["Priority"].ToString()),
                    Locked = Convert.ToBoolean((bool)row["Locked"]),
                    CreatedDate = String.Format("{0:dd\\/MM\\/yyyy HH:mm}", row["CreatedDate"]),
                    ParentName = row["ParentName"].ToString()
                });
                return await Task.FromResult(result).ConfigureAwait(false);
            }
        }

        public async Task<IEnumerable<StructureDTO>> Sysn_Structure(long CustomerID)
        {
            DataTable dt = new DataTable();
            SqlConditions Conds = new SqlConditions();
            //Conds.Add("CustomerID", custormerID);
            string sqlq = @"select dv1.[Id]
                                          , dv1.[LoaiDonVi]
                                          ,dv1.[DonVi]
                                          ,dv1.[MaDonvi]
                                          ,dv1.[MaDinhDanh]
                                          ,dv1.[Parent]
                                          ,dv1.[SDT]
                                          ,dv1.[Email]
                                          ,dv1.[DiaChi]
                                          ,dv1.[Logo]
                                          ,dv1.[Locked]
                                          ,dv1.[Priority]
                                          ,dv1.[DangKyLichLanhDao]
                                          ,dv1.[KhongCoVanThu]
                                          ,dv1.[CreatedBy]
                                          ,dv1.[CreatedDate]
                                          ,dv1.[UpdatedBy]
                                          ,dv1.[UpdatedDate]
                                          ,dv1.[Disabled]
                                            ,dv2.DonVi as ParentName
                                           from Cocautochuc dv1
                                            left
                                           join Cocautochuc dv2 on dv1.Parent = dv2.Id
                                          where dv1.[Disabled] = 0 and(dv1.Parent = @IdDV or dv1.Id = @IdDV)";
            using (DpsConnection cnn = new DpsConnection(_connectionString))
            {
                dt = cnn.CreateDataTable(sqlq, Conds);
                var temp = dt.AsEnumerable();
                var result = dt.AsEnumerable().Select(row => new StructureDTO
                {
                    Id = long.Parse(row["ID"].ToString()),
                    DonVi = row["DonVi"].ToString(),
                    MaDonvi = row["MaDonvi"].ToString(),
                    MaDinhDanh = row["MaDinhDanh"].ToString(),
                    Parent = long.Parse(row["Parent"].ToString()),
                    SDT = row["SDT"].ToString(),
                    Email = row["Email"].ToString(),
                    DiaChi = row["DiaChi"].ToString(),
                    Logo = row["Logo"].ToString(),
                    DangKyLichLanhDao = Convert.ToBoolean((bool)row["DangKyLichLanhDao"]),
                    KhongCoVanThu = Convert.ToBoolean((bool)row["KhongCoVanThu"]),
                    LoaiDonVi = long.Parse(row["LoaiDonVi"].ToString()),
                    Priority = long.Parse(row["Priority"].ToString()),
                    Locked = Convert.ToBoolean((bool)row["Locked"]),
                    CreatedDate = String.Format("{0:dd\\/MM\\/yyyy HH:mm}", row["CreatedDate"]),
                    ParentName = row["ParentName"].ToString()
                });
                return await Task.FromResult(result).ConfigureAwait(false);
            }
        }
    }
}