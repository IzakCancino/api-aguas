using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Data;
using System.Data.Entity;
using System.Data.Entity.Infrastructure;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Reflection.Emit;
using System.Web.Http;
using System.Web.Http.Description;
using api_aguas.Models;

namespace api_aguas.Controllers
{
    public class ReportsController : ApiController
    {
        private model_db db = new model_db();

        // GET: api/Reports/Read
        [HttpGet]
        [Route("api/Reports/Read")]
        public IHttpActionResult ReadReports()
        {
            return Ok(db.Reports.Select(x => new {
                x.IdReport,
                x.Latitude,
                x.Longitude,
                x.Description,
                x.Status,
                x.CreationDate,
                x.ModificationDate,
                ReportType = new {
                    x.ReportType.IdReportType,
                    x.ReportType.Name,
                    Organization = new
                    {
                        x.ReportType.Organization.IdOrganization,
                        x.ReportType.Organization.Name
                    }
                }
            }));
        }

        // GET: api/Reports/Read
        [HttpGet]
        [Route("api/Reports/Read")]
        public IHttpActionResult ReadReports(int idReportType, int status, int daysAgo)
        {
            IQueryable<Report> reports = db.Reports;

            if (idReportType > 0)
            {
                reports = reports.Where(x => x.IdReportType == idReportType);
            }

            if (status > 0)
            {
                reports = reports.Where(x => x.Status == status);
            }

            if (daysAgo > 0)
            {               
                DateTime thresholdDate = DateTime.Now.AddDays(-daysAgo);
                reports = reports.Where(x => x.CreationDate >= thresholdDate);
            }

            return Ok(reports.Select(x => new {
                x.IdReport,
                x.Latitude,
                x.Longitude,
                x.Description,
                x.Status,
                x.CreationDate,
                x.ModificationDate,
                ReportType = new
                {
                    x.ReportType.IdReportType,
                    x.ReportType.Name,
                    Organization = new
                    {
                        x.ReportType.Organization.IdOrganization,
                        x.ReportType.Organization.Name
                    }
                }
            }));
        }

        // GET: api/Reports/Read/5
        [HttpGet]
        [Route("api/Reports/Read/{id}")]
        public IHttpActionResult ReadReport(int id)
        {
            Report report = db.Reports.Find(id);
            if (report == null)
            {
                return NotFound();
            }

            return Ok(new {
                report.IdReport,
                report.Latitude,
                report.Longitude,
                report.Description,
                report.Status,
                report.CreationDate,
                report.ModificationDate,
                ReportType = new
                {
                    report.ReportType.IdReportType,
                    report.ReportType.Name,
                    Organization = new
                    {
                        report.ReportType.Organization.IdOrganization,
                        report.ReportType.Organization.Name
                    }
                }
            });
        }

        // POST: api/Reports/Update
        [Route("api/Reports/Update")]
        [HttpPost]
        public IHttpActionResult UpdateReport(Report report)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            if (!IsLoggedIn(report.IdUser))
            {
                return Unauthorized();
            }

            if (!db.Reports.Any(x => x.IdReport == report.IdReport))
            {
                return BadRequest();
            }

            db.Entry(report).State = EntityState.Modified;

            try
            {
                db.SaveChanges();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!ReportExists(report.IdReport))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }

            return StatusCode(HttpStatusCode.NoContent);
        }

        // POST: api/Reports/Create
        [HttpPost]
        [Route("api/Reports/Create")]
        public IHttpActionResult CreateReport(Report report)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            if (!IsLoggedIn(report.IdUser))
            {
                return Unauthorized();
            }

            db.Reports.Add(report);
            db.SaveChanges();

            return Ok(report.IdReport);
        }

        // POST: api/Reports/Delete
        [HttpPost]
        [Route("api/Reports/Delete")]
        public IHttpActionResult DeleteReport(int id)
        {
            Report report = db.Reports.Find(id);
            if (report == null)
            {
                return NotFound();
            }

            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            if (!IsLoggedIn(report.IdUser))
            {
                return Unauthorized();
            }

            db.Reports.Remove(report);
            db.SaveChanges();

            return Ok(report);
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                db.Dispose();
            }
            base.Dispose(disposing);
        }

        private bool ReportExists(int id)
        {
            return db.Reports.Count(e => e.IdReport == id) > 0;
        }

        private bool IsLoggedIn(int? checkIdUser = null)
        {
            try
            {
                // Token is in request header and formated as: IdUser + '/' + SessionToken
                if (Request.Headers.GetValues("Authorization") == null)
                {
                    return false;
                }

                string token = Request.Headers.Authorization.Parameter ?? "0/0";
                string[] s = token.Split('/');

                if (!int.TryParse(s[0], out int id) || (checkIdUser != null && checkIdUser != id))
                {
                    return false;
                }

                User user = db.Users.Find(id);
                return user.IsEnabled && user.SessionToken == s[1];
            }
            catch (Exception)
            {
                return false;
            }
        }
    }
}