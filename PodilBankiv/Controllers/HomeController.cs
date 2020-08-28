using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using PodilBankiv.Models;
using PodilBankiv.MyClasses;
using DotNetDBF;

namespace PodilBankiv.Controllers
{
    public class HomeController : Controller
    {
        private IWebHostEnvironment appEnv;

        public HomeController(IWebHostEnvironment appEnviroment)
        {
            appEnv = appEnviroment;
        }

        public IActionResult Index()
        {
            return View();
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [HttpPost]
        public ActionResult LoadFile(IFormFile formFileES, IFormFile formFileE)
        {
            Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
            string period = formFileES.FileName.Trim().Substring(0, 4);
            string filePath = "\\Files\\" + period + "\\";
            string fullPathES = appEnv.WebRootPath + filePath + formFileES.FileName;
            string fullPathE = appEnv.WebRootPath + filePath + formFileE.FileName;

            if (Directory.Exists(appEnv.WebRootPath + "\\Files\\"))
                Directory.Delete(appEnv.WebRootPath + "\\Files\\", true);

            if (!Directory.Exists(appEnv.WebRootPath + filePath))
                Directory.CreateDirectory(appEnv.WebRootPath + filePath);

            string[] partsES = formFileES.FileName.Split("_");
            string[] partsE = formFileE.FileName.Split("_");
            string startES = partsES[0];
            string endES = partsES[1];
            string startE = partsE[0];
            string endE = partsE[1];

            bool EqualPeriods = String.Equals(startES, startE);
            bool EqualE = endE.ToLower() == "e.dbf";
            bool EqualES = endES.ToLower() == "es.dbf";
            
            if (EqualE && EqualES && EqualPeriods)
            {
                using (var fileStreamES = new FileStream(fullPathES, FileMode.Create))
                        formFileES.CopyTo(fileStreamES);

                using (var fileStreamE = new FileStream(fullPathE, FileMode.Create))
                    formFileE.CopyTo(fileStreamE);
            }
            else
            {
                ViewBag.error = "BadFile";
                return View("Index");
            }

            List<Banks> ES = new List<Banks>();
            List<Abons> E = new List<Abons>();


            using (var dbfDataReader = NDbfReader.Table.Open(fullPathE))
            {
                var reader = dbfDataReader.OpenReader(Encoding.GetEncoding(866));
                while (reader.Read())
                {
                    var row = new Abons();
                    row.Dat = DateTime.Parse(reader.GetValue("DAT").ToString());
                    row.Numbpers = int.Parse(reader.GetValue("NUMBPERS").ToString());
                    row.Suma = decimal.Parse(reader.GetValue("SUMA").ToString());
                    row.Poppok = reader.GetValue("POPPOK")?.ToString();
                    row.kb = int.Parse(reader.GetValue("KB").ToString());
                    E.Add(row);
                }
            }

            using (var dbfDataReader = NDbfReader.Table.Open(fullPathES))
            {
                var reader = dbfDataReader.OpenReader(Encoding.GetEncoding(866));
                while (reader.Read())
                {
                    var row = new Banks();
                    row.FileNameDate = period;
                    row.Dat = DateTime.Parse(reader.GetValue("DAT").ToString());
                    row.Kb = int.Parse(reader.GetValue("KB").ToString());
                    row.Bank = reader.GetValue("BANK").ToString();
                    row.Kil = int.Parse(reader.GetValue("KIL").ToString());
                    row.Suma = decimal.Parse(reader.GetValue("SUMA").ToString());
                    ES.Add(row);
                    
                    string resultFilePath = appEnv.WebRootPath + filePath + period + "_" + row.Kb + ".dbf";
                    using (Stream fos = System.IO.File.Open(resultFilePath, FileMode.OpenOrCreate, FileAccess.ReadWrite))
                    {
                        using (var writer = new DBFWriter())
                        {
                            writer.CharEncoding = Encoding.Default;
                            writer.Signature = DBFSignature.DBase3;
                            writer.LanguageDriver = 0x26; // кодировка 866
                            var field1 = new DBFField("DAT", NativeDbType.Date);
                            var field2 = new DBFField("KB", NativeDbType.Numeric, 10);
                            var field3 = new DBFField("NUMBPERS", NativeDbType.Numeric, 10);
                            var field4 = new DBFField("SUMMA", NativeDbType.Numeric, 10, 2);
                            var field5 = new DBFField("POPPOK", NativeDbType.Char, 10);
                            writer.Fields = new[] { field1, field2, field3, field4, field5 };
                            foreach (Abons abon in E)
                            {
                                if(abon.kb == row.Kb)
                                {
                                    writer.AddRecord(
                                        abon.Dat,
                                        abon.kb,
                                        abon.Numbpers,
                                        abon.Suma,
                                        abon.Poppok
                                    );
                                }
                            }
                            writer.Write(fos);
                        }
                    }
                }
            }
           
            UploadedBankAbons ba = new UploadedBankAbons();
            ba.banks = ES;
            ba.abons = E;
            string userKey = ((Int32)(DateTime.UtcNow.Subtract(new DateTime(1970, 1, 1))).TotalSeconds).ToString();
            GavnoVopros.UserLoaded[userKey] = ba;
            HttpContext.Response.Cookies.Append("userKey", userKey);
            return View(ES);
        }

        public FileResult UnloadFile(int kodBank)
        {
            string userKey = HttpContext.Request.Cookies["userKey"];
            UploadedBankAbons ba = GavnoVopros.UserLoaded[userKey];
            Banks curBank = new Banks();
            foreach(Banks bank in ba.banks)
            {
                if(bank.Kb == kodBank)
                {
                    string fileName = bank.FileNameDate + "_" + bank.Kb + ".dbf";
                    curBank = bank;
                    byte[] fileBytes = System.IO.File.ReadAllBytes(
                        appEnv.WebRootPath + 
                        "\\Files\\" + 
                       bank.FileNameDate +
                        "\\" +
                        fileName
                    );
                    return File(fileBytes, System.Net.Mime.MediaTypeNames.Application.Octet, fileName);
                }
            }
            return File(new byte[1], System.Net.Mime.MediaTypeNames.Application.Octet, "Файл не знайдено!");
        }

[ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
