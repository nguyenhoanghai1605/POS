using OfficeOpenXml;
using POS.Models;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace POS.Controllers
{
    public class HomeController : Controller
    {
        POS_Entities db = new POS_Entities();

        public ActionResult DanhSachKCV(string size, string measure, string thongso)
        {
            var result = db.DanhSachKCVs.AsQueryable();

            if (!string.IsNullOrEmpty(size))
                result = result.Where(x => x.SIZE.Contains(size));

            // Kiểm tra và lọc MEASURE nếu nhập đúng định dạng số
            if (!string.IsNullOrEmpty(measure))
            {
                if (decimal.TryParse(measure, out decimal measureValue))
                {
                    // Lọc chính xác theo giá trị MEASURE
                    result = result.Where(x => x.MEASURE == measureValue);
                }
            }

            if (!string.IsNullOrEmpty(thongso))
                result = result.Where(x => x.Thong_So_KCV.Contains(thongso));

            // Truyền danh sách Thông số KCV để hiển thị dropdown
            ViewBag.ThongSoList = db.DanhSachKCVs
                .Select(x => x.Thong_So_KCV)
                .Distinct()
                .OrderBy(x => x)
                .ToList();

            return View(result.ToList());
        }

        public ActionResult Index(string size, string measure, string thongso, int page = 1, int pageSize = 50)
        {
            var data = db.DanhSachKCVs.AsQueryable();

            // Lọc Size
            if (!string.IsNullOrEmpty(size))
                data = data.Where(x => x.SIZE.Contains(size));

            // Lọc Measure (decimal?)
            if (!string.IsNullOrEmpty(measure) && decimal.TryParse(measure, out var measureVal))
                data = data.Where(x => x.MEASURE.HasValue && x.MEASURE.Value == measureVal);

            // Lọc Thông số
            if (!string.IsNullOrEmpty(thongso))
                data = data.Where(x => x.Thong_So_KCV.Contains(thongso));

            // Tổng số dòng
            int totalItems = data.Count();

            // Lấy danh sách phân trang
            var pagedData = data
                .OrderBy(x => x.SIZE) // bạn có thể thay đổi điều kiện sắp xếp
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToList();

            // ViewBag để phân trang và hiển thị
            ViewBag.CurrentPage = page;
            ViewBag.PageSize = pageSize;
            ViewBag.TotalItems = totalItems;
            ViewBag.ThongSoList = db.DanhSachKCVs.Select(x => x.Thong_So_KCV).Distinct().ToList();

            return View(pagedData);
        }

        [HttpGet]
        public JsonResult GetThongSoKCV(string size, string measure)
        {
            var query = db.DanhSachKCVs.AsQueryable();

            if (!string.IsNullOrEmpty(size))
                query = query.Where(x => x.SIZE.Contains(size));

            if (!string.IsNullOrEmpty(measure) && decimal.TryParse(measure, out var measureValue))
                query = query.Where(x => x.MEASURE != null && x.MEASURE.Value == measureValue);

            var thongSoList = query
                .Where(x => !string.IsNullOrEmpty(x.Thong_So_KCV))
                .Select(x => x.Thong_So_KCV)
                .Distinct()
                .OrderBy(x => x)
                .ToList();

            return Json(thongSoList, JsonRequestBehavior.AllowGet);
        }


        [HttpGet]
        public JsonResult GetKCVData(int page = 1, int pageSize = 20, string size = "", string measure = "", string thongso = "")
        {
            try
            {
                var query = db.DanhSachKCVs.AsQueryable();

                if (!string.IsNullOrEmpty(size))
                    query = query.Where(x => x.SIZE.Contains(size));

                if (!string.IsNullOrEmpty(measure) && decimal.TryParse(measure, out var measureVal))
                {
                    query = query.Where(x => x.MEASURE.HasValue && x.MEASURE.Value == measureVal);
                }


                if (!string.IsNullOrEmpty(thongso))
                    query = query.Where(x => x.Thong_So_KCV.Contains(thongso));

                var totalItems = query.Count();
                var totalPages = (int)Math.Ceiling((double)totalItems / pageSize);

                var data = query
                    .OrderBy(x => x.KCV) // Sắp xếp tùy chọn
                    .Skip((page - 1) * pageSize)
                    .Take(pageSize)
                    .Select(x => new
                    {
                        x.SIZE,
                        x.KCV_Type,
                        x.KCV,
                        x.KCV_Name,
                        x.Thong_So_KCV,
                        x.MEASURE,
                        x.MEASURE_Details,
                        x.TLSP,
                        x.SL,
                        x.Retail_Price,
                        x.Discount,
                        x.POS_Discount,
                        x.Note
                    })
                    .ToList();

                return Json(new
                {
                    data = data,
                    totalPages = totalPages
                }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(new
                {
                    error = true,
                    message = ex.Message
                }, JsonRequestBehavior.AllowGet);
            }
        }

        public ActionResult DKPhieuQuaTang()
        {
            var dsKhachHang = db.KhachHangs.ToList();
            var dsPhieu = db.PhieuQuaTangs.ToList();
            var dsKetQua = new List<KhachHang>();
            if (dsPhieu!=null)
            {
                if (dsPhieu!=null && dsPhieu.Any())
                {
                    foreach (var item in dsKhachHang)
                    {
                        var flag = false;
                        foreach (var phieu in dsPhieu)
                        {
                            if (item.MaKH == phieu.MaKH)
                            {
                                flag = true;
                                continue;
                            }
                            
                        }
                        if (!flag)
                        {
                            dsKetQua.Add(item);
                        }
                    }
                }
                else
                {
                    dsKetQua = dsKhachHang;
                }
               
            }
            ViewBag.KhachHang = dsKetQua;
            return View();
        }

        [HttpPost]
        public ActionResult XemThongTin(KhachHangModel.Input.TimKiem input)
        {
           var model = new Common.Output.CommonOutput();
            try
            {
                var thongTinPhieu = db.PhieuQuaTangs.Where(x => x.MaPhieuQuaTang == input.MaPhieuQuaTang).FirstOrDefault();
                if (thongTinPhieu!=null)
                {
                    var thongTinKH = db.KhachHangs.FirstOrDefault(x => x.MaKH == thongTinPhieu.MaKH);
                    if (thongTinKH != null)
                    {
                        model.KetQua = 1;
                        model.ThongBao = "Thành công";
                        model.DuLieu = thongTinKH;
                    }
                    else
                    {
                        model.KetQua = 2;
                        model.ThongBao = "Không tìm thấy thông tin!";
                    }
                }
                else
                {
                    model.KetQua = 2;
                    model.ThongBao = "Không tìm thấy thông tin!";
                }

            }
            catch (Exception)
            {
                model.KetQua = 0;
                model.ThongBao = "Thất bại!";
            }
            return Json(model);
        }

        [HttpPost]
        public ActionResult DangKy(KhachHangModel.Input.DangKy input)
        {
            var model = new Common.Output.CommonOutput();
            model.KetQua = 0;
            try
            {
                if (string.IsNullOrEmpty(input.MaPhieuQuaTang))
                {
                    model.ThongBao = "Vui lòng nhập mã phiếu quà tặng!";
                    return Json(model);
                }
                if (string.IsNullOrEmpty(input.MaKH))
                {
                    model.ThongBao = "Vui lòng chọn khách hàng!";
                    return Json(model);
                }
                var maKH = int.Parse(input.MaKH);
                var thongTinPhieuCoKhachHang = db.PhieuQuaTangs.Where(x => x.MaKH == maKH).FirstOrDefault();
                if (thongTinPhieuCoKhachHang !=null)
                {
                    model.ThongBao = "Mỗi khách hàng chỉ được 1 phiếu quà tặng!";
                    return Json(model);
                }
                var thongTinPhieu = db.PhieuQuaTangs.Where(x => x.MaPhieuQuaTang == input.MaPhieuQuaTang).FirstOrDefault();
                if (thongTinPhieu != null)
                {
                    model.ThongBao = "Phiếu quà tặng đã dược sử dụng vui lòng nhập mã phiếu quà tặng khác!";
                    return Json(model);
                }
                else
                {
                    POS_Entities _db = new POS_Entities();
                    var thongTin = _db.Set<PhieuQuaTang>();
                    
                    thongTin.Add(new PhieuQuaTang
                    {
                        MaPhieuQuaTang = input.MaPhieuQuaTang,
                        MaKH = maKH
                    });
                     _db.SaveChanges();
                   
                    var thongTinKH = db.KhachHangs.FirstOrDefault(x => x.MaKH == maKH);
                    if (thongTinKH != null)
                    {
                        model.KetQua = 1;
                        model.ThongBao = "Thành công";
                        model.DuLieu = thongTinKH;
                    }
                    else
                    {
                        model.KetQua = 2;
                        model.ThongBao = "Không tìm thấy thông tin!";
                    }
                }

            }
            catch (Exception ex)
            {
                model.KetQua = 0;
                model.ThongBao = "Thất bại!";
            }
            return Json(model);
        }

        [HttpGet]
        public ActionResult ImportKCV()
        {
            return View();
        }

        [HttpPost]
        public ActionResult ImportExcelKCV(HttpPostedFileBase excelFile)
        {
            if (excelFile == null || excelFile.ContentLength == 0)
            {
                ViewBag.Message = "Vui lòng chọn file Excel!";
                return View("ImportKCV");
            }

            if (!excelFile.FileName.EndsWith(".xlsx"))
            {
                ViewBag.Message = "Vui lòng chọn đúng định dạng file .xlsx!";
                return View("ImportKCV");
            }

            try
            {
                var dt = new DataTable();
                dt.Columns.Add("STT", typeof(int));
                dt.Columns.Add("KCV_Type", typeof(string));
                dt.Columns.Add("KCV", typeof(string));
                dt.Columns.Add("KCV_Name", typeof(string));
                dt.Columns.Add("SIZE", typeof(string));
                dt.Columns.Add("Thong_So_KCV", typeof(string));
                dt.Columns.Add("MEASURE", typeof(decimal));
                dt.Columns.Add("MEASURE_Details", typeof(string));
                dt.Columns.Add("TLSP", typeof(decimal));
                dt.Columns.Add("SL", typeof(int));
                dt.Columns.Add("Retail_Price", typeof(string));
                dt.Columns.Add("Discount", typeof(string));
                dt.Columns.Add("POS_Discount", typeof(string));
                dt.Columns.Add("Note", typeof(string));

                using (var package = new ExcelPackage(excelFile.InputStream))
                {
                    ExcelWorksheet worksheet = package.Workbook.Worksheets.FirstOrDefault();
                    int rowCount = worksheet.Dimension.Rows;

                    for (int row = 2; row <= rowCount; row++) // dòng tiêu đề là dòng 1
                    {
                        dt.Rows.Add(
                            int.TryParse(worksheet.Cells[row, 1].Text, out int stt) ? stt : 0,
                            worksheet.Cells[row, 2].Text,
                            worksheet.Cells[row, 3].Text,
                            worksheet.Cells[row, 4].Text,
                            worksheet.Cells[row, 5].Text,
                            worksheet.Cells[row, 6].Text,
                            decimal.TryParse(worksheet.Cells[row, 7].Text, out var measure) ? measure : 0,
                            worksheet.Cells[row, 8].Text,
                            decimal.TryParse(worksheet.Cells[row, 9].Text, out var tlsp) ? tlsp : 0,
                            int.TryParse(worksheet.Cells[row, 10].Text, out var sl) ? sl : 0,
                            worksheet.Cells[row, 11].Text.Trim(),
                            worksheet.Cells[row, 12].Text,
                            worksheet.Cells[row, 13].Text,
                            worksheet.Cells[row, 14].Text
                        );
                    }
                }

                // Chuỗi kết nối - bạn có thể lấy từ Web.config
                //string connStr = System.Configuration.ConfigurationManager.ConnectionStrings["POS_Entities"].ConnectionString;
                string connStr = System.Configuration.ConfigurationManager.ConnectionStrings["POSEntities"].ConnectionString;

                using (SqlConnection conn = new SqlConnection(connStr))
                {
                    conn.Open();
                    using (SqlBulkCopy bulkCopy = new SqlBulkCopy(conn))
                    {
                        bulkCopy.DestinationTableName = "DanhSachKCV";

                        foreach (DataColumn column in dt.Columns)
                        {
                            bulkCopy.ColumnMappings.Add(column.ColumnName, column.ColumnName);
                        }

                        bulkCopy.WriteToServer(dt);
                    }
                }

                ViewBag.Message = "Import dữ liệu thành công!";
            }
            catch (Exception ex)
            {
                ViewBag.Message = "Lỗi khi import: " + ex.Message;
            }

            return View("ImportKCV");
        }



    }
}