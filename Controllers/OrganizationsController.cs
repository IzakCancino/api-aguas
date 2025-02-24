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
    public class OrganizationsController : ApiController
    {
        private model_db db = new model_db();

        // GET: api/Organizations/Read
        [HttpGet]
        [Route("api/Organizations/Read")]
        public IHttpActionResult ReadOrganizations()
        {
            return Ok(db.Organizations.Select(x => new {
                x.IdOrganization,
                x.Name,
                x.Phone,
                x.Color,
                x.IsMessageable,
                ReportTypes = x.ReportTypes.Select(r => new 
                {
                    r.IdReportType,
                    r.Name
                })
            }));
        }

        // GET: api/Organizations/Read/5
        [HttpGet]
        [Route("api/Organizations/Read/{id}")]
        public IHttpActionResult GetOrganization(int id)
        {
            Organization organization = db.Organizations.Find(id);
            if (organization == null)
            {
                return NotFound();
            }

            return Ok(new {
                organization.IdOrganization,
                organization.Name,
                organization.Phone,
                organization.Color,
                organization.IsMessageable,
                ReportTypes = organization.ReportTypes.Select(r => new
                {
                    r.IdReportType,
                    r.Name
                })
            });
        }

        // POST: api/Organizations/Update
        [HttpPost]
        [Route("api/Organizations/Update")]
        public IHttpActionResult UpdateOrganization(Organization organization, string adminPassword)
        {
            if (!IsAdmin(adminPassword))
            {
                return Unauthorized();
            }

            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            if (!db.Organizations.Any(x => x.IdOrganization == organization.IdOrganization))
            {
                return BadRequest();
            }

            db.Entry(organization).State = EntityState.Modified;

            try
            {
                db.SaveChanges();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!OrganizationExists(organization.IdOrganization))
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

        // POST: api/Organizations/Create
        [HttpPost]
        [Route("api/Organizations/Create")]
        public IHttpActionResult CreateOrganization(Organization organization, string adminPassword)
        {
            if (!IsAdmin(adminPassword))
            {
                return Unauthorized();
            }

            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            db.Organizations.Add(organization);
            db.SaveChanges();

            return Created($"api/Organizations/{organization.IdOrganization}", organization);
        }

        // POST: api/Organizations/Delete
        [HttpPost]
        [Route("api/Organizations/Delete")]
        public IHttpActionResult DeleteOrganization(int id, string adminPassword)
        {
            if (!IsAdmin(adminPassword))
            {
                return Unauthorized();
            }

            Organization organization = db.Organizations.Find(id);
            if (organization == null)
            {
                return NotFound();
            }

            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            db.Organizations.Remove(organization);
            db.SaveChanges();

            return Ok(organization);
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                db.Dispose();
            }
            base.Dispose(disposing);
        }

        private bool OrganizationExists(int id)
        {
            return db.Organizations.Count(e => e.IdOrganization == id) > 0;
        }
        
        private bool IsAdmin(string password)
        {
            return db.Users.Find(10).Password == password;
        }
    }
}