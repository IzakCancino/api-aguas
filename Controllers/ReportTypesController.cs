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
    public class ReportTypesController : ApiController
    {
        private model_db db = new model_db();

        // GET: api/ReportTypes/Read
        [HttpGet]
        [Route("api/ReportTypes/Read")]
        public IHttpActionResult ReadReportTypes()
        {
            return Ok(db.ReportTypes.Select(x => new {
                x.IdReportType,
                x.Name,
                Organization = new {
                    x.Organization.IdOrganization, 
                    x.Organization.Name
                }
            }));
        }

        // GET: api/ReportTypes/Read/5
        [HttpGet]
        [Route("api/ReportTypes/Read/{id}")]
        public IHttpActionResult ReadReportType(int id)
        {
            ReportType reportType = db.ReportTypes.Find(id);
            if (reportType == null)
            {
                return NotFound();
            }

            return Ok(new
            {
                reportType.IdReportType,
                reportType.Name,
                Organization = new
                {
                    reportType.Organization.IdOrganization,
                    reportType.Organization.Name
                }
            });
        }

        // POST: api/ReportTypes/Update
        [HttpPost]
        [Route("api/ReportTypes/Update")]
        public IHttpActionResult UpdateReportType(ReportType reportType, string adminPassword)
        {
            if (!IsAdmin(adminPassword))
            {
                return Unauthorized();
            }

            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            if (!db.ReportTypes.Any(x => x.IdReportType == reportType.IdReportType))
            {
                return BadRequest();
            }

            db.Entry(reportType).State = EntityState.Modified;

            try
            {
                db.SaveChanges();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!ReportTypeExists(reportType.IdReportType))
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

        // POST: api/ReportTypes/Create
        [HttpPost]
        [Route("api/ReportTypes/Create")]
        public IHttpActionResult CreateReportType(ReportType reportType, string adminPassword)
        {
            if (!IsAdmin(adminPassword))
            {
                return Unauthorized();
            }

            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            db.ReportTypes.Add(reportType);
            db.SaveChanges();

            return Created($"api/ReportTypes/{reportType.IdReportType}", reportType);
        }

        // POST: api/ReportTypes/Delete
        [HttpPost]
        [Route("api/ReportTypes/Delete")]
        public IHttpActionResult DeleteReportType(int id, string adminPassword)
        {
            if (!IsAdmin(adminPassword))
            {
                return Unauthorized();
            }

            ReportType reportType = db.ReportTypes.Find(id);
            if (reportType == null)
            {
                return NotFound();
            }

            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            db.ReportTypes.Remove(reportType);
            db.SaveChanges();

            return Ok(reportType);
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                db.Dispose();
            }
            base.Dispose(disposing);
        }

        private bool ReportTypeExists(int id)
        {
            return db.ReportTypes.Count(e => e.IdReportType == id) > 0;
        }

        private bool IsAdmin(string password)
        {
            return db.Users.Find(10).Password == password;
        }
    }
}