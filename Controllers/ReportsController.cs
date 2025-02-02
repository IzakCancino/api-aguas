using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Data.Entity.Infrastructure;
using System.Linq;
using System.Net;
using System.Net.Http;
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
        public IQueryable<Report> ReadReports()
        {
            return db.Reports;
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

            return Ok(report);
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

            db.Reports.Add(report);
            db.SaveChanges();

            return CreatedAtRoute("DefaultApi", new { id = report.IdReport }, report);
        }

        // POST: api/Reports/Delete
        [HttpPost]
        [Route("api/Reports/Delete")]
        public IHttpActionResult DeleteReport(Report report)
        {
            if (report == null)
            {
                return NotFound();
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
    }
}