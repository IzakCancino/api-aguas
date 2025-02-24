using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Data.Entity.Infrastructure;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Security.Cryptography;
using System.Web.Http;
using System.Web.Http.Description;
using api_aguas.Models;

namespace api_aguas.Controllers
{
    public class UsersController : ApiController
    {
        private model_db db = new model_db();

        // GET: api/Users/Read
        [HttpGet]
        [Route("api/Users/Read")]
        public IHttpActionResult ReadUsers()
        {
            return Ok(db.Users.Select(x => new {
                x.IdUser,
                x.Name,
                x.LastName,
                x.IsEnabled
            }));
        }

        // GET: api/Users/5
        [HttpGet]
        [Route("api/Users/Read/{id}")]
        public IHttpActionResult ReadUser(int id)
        {
            User user = db.Users.Find(id);
            if (user == null)
            {
                return NotFound();
            }

            return Ok(new {
                user.IdUser,
                user.Name,
                user.LastName,
                user.IsEnabled
            });
        }

        // POST: api/Users/Update
        [HttpPost]
        [Route("api/Users/Update")]
        public IHttpActionResult UpdateUser(User user)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            if (!db.Users.Any(x => x.IdUser == user.IdUser))
            {
                return BadRequest();
            }

            db.Entry(user).State = EntityState.Modified;

            try
            {
                db.SaveChanges();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!UserExists(user.IdUser))
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

        // POST: api/Users/Create
        [HttpPost]
        [Route("api/Users/Create")]
        public IHttpActionResult CreateUser(User user)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            db.Users.Add(user);
            db.SaveChanges();

            return Created($"api/Users/{user.IdUser}", user);
        }

        // POST: api/Users/Delete
        [HttpPost]
        [Route("api/Users/Delete")]
        public IHttpActionResult DeleteUser(User user)
        {
            if (user == null)
            {
                return NotFound();
            }

            db.Users.Remove(user);
            db.SaveChanges();

            return Ok(user);
        }

        // POST: api/Users/Login
        [HttpPost]
        [Route("api/Users/Login")]
        public IHttpActionResult Login(User user)
        {
            if (!db.Users.Any(x => x.Email == user.Email && x.Password == user.Password))
            {
                return Json(new { 
                    Success = false, 
                    Value = user.Email, 
                    Message = "Las credenciales enviadas son incorrectas." 
                });
            }

            User userFound = db.Users.FirstOrDefault(x => x.Email == user.Email && x.Password == user.Password);

            if (!userFound.IsEnabled)
            {
                return Json(new { 
                    Success = false, 
                    Value = user.Email, 
                    Message = "Las credenciales enviadas son correctas, pero el usuario se encuentra desactivado." 
                });
            }

            try
            {
                // Generate a random 32-bytes session token
                RandomNumberGenerator rng = RandomNumberGenerator.Create();
                byte[] tokenBytes = new byte[32];
                rng.GetBytes(tokenBytes);
                string sessionToken = Convert.ToBase64String(tokenBytes).TrimEnd('=').Replace('+', '-').Replace('/', '_');

                // Save token
                userFound.SessionToken = sessionToken;
                db.Entry(userFound).State = EntityState.Modified;
                db.SaveChanges();
            }
            catch (DbUpdateConcurrencyException)
            {
                return Json(new { 
                    Success = false, 
                    Value = user.Email, 
                    Message = "No fue posible iniciar sesión." 
                });
            }

            return Json(new { 
                Success = true, 
                Value = userFound.IdUser,
                Message = "Inicio de sesión exitoso." 
            });
        }

        // POST: api/Users/Historial
        [HttpPost]
        [Route("api/Users/Historial")]
        public IHttpActionResult HistorialUser(User user)
        {
            if (!db.Users.Any(x => x.IdUser == user.IdUser && x.SessionToken == user.SessionToken))
            {
                return Json(new
                {
                    Success = false,
                    Value = new List<Report> { },
                    Message = "Historial de reportes no encontrado."
                });
            }

            User userFound = db.Users.Find(user.IdUser);

            return Ok(new {
                userFound.IdUser,
                Reports = userFound.Reports.Select(x => new {
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
                })
            });
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                db.Dispose();
            }
            base.Dispose(disposing);
        }

        private bool UserExists(int id)
        {
            return db.Users.Count(e => e.IdUser == id) > 0;
        }
    }
}