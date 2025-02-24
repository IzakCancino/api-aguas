using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Data.Entity.Infrastructure;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Security.Cryptography;
using System.Text;
using System.Web.Http;
using System.Web.Http.Description;
using api_aguas.Models;

namespace api_aguas.Controllers
{
    public static class HashUtil
    {
        public static string Generate(string password)
        {
            // Generate a 128-bit salt using a secure PRNG
            byte[] salt = new byte[16];
            using (var rng = RandomNumberGenerator.Create())
            {
                rng.GetBytes(salt);
            }

            // Create the Rfc2898DeriveBytes and get the hash value
            var pbkdf2 = new Rfc2898DeriveBytes(password, salt, 100000, HashAlgorithmName.SHA256);
            byte[] hash = pbkdf2.GetBytes(32); // 256-bit hash

            // Combine salt + hash
            byte[] hashBytes = new byte[48];
            Array.Copy(salt, 0, hashBytes, 0, 16);
            Array.Copy(hash, 0, hashBytes, 16, 32);

            // Convert to base64 for storage
            return Convert.ToBase64String(hashBytes);
        }

        public static bool Verification(string password, string storedHash)
        {
            byte[] hashBytes = Convert.FromBase64String(storedHash);

            // Extract the salt
            byte[] salt = new byte[16];
            Array.Copy(hashBytes, 0, salt, 0, 16);

            // Hash the entered password with the extracted salt
            var pbkdf2 = new Rfc2898DeriveBytes(password, salt, 100000, HashAlgorithmName.SHA256);
            byte[] hash = pbkdf2.GetBytes(32);

            // Compare the results
            for (int i = 0; i < 32; i++)
            {
                if (hashBytes[i + 16] != hash[i])
                {
                    return false; // Password doesn't match
                }
            }
            return true; // Password matches
        }
    }

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
        public IHttpActionResult UpdateUser(User user, string adminPassword)
        {
            if (!IsAdmin(adminPassword))
            {
                return Unauthorized();
            }

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

            if (db.Users.Any(x => x.Email == user.Email))
            {
                return Json(new
                {
                    Success = false,
                    Value = user.Email,
                    Message = "El correo electrónico enviado ya esta registrado."
                });
            }

            user.Password = HashUtil.Generate(user.Password);

            db.Users.Add(user);
            db.SaveChanges();

            return Json(new
            {
                Success = true,
                Value = user.IdUser,
                Message = "Cuenta creada exitosamente."
            }); ;
        }

        // POST: api/Users/Delete
        [HttpPost]
        [Route("api/Users/Delete")]
        public IHttpActionResult DeleteUser(int id, string adminPassword)
        {
            if (!IsAdmin(adminPassword))
            {
                return Unauthorized();
            }

            User user = db.Users.Find(id);
            if (user == null)
            {
                return NotFound();
            }

            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
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
            var possibleUser = db.Users.FirstOrDefault(x => x.Email == user.Email);

            if (possibleUser == null || !HashUtil.Verification(user.Password, possibleUser.Password))
            {
                return Json(new { 
                    Success = false, 
                    Value = user.Email, 
                    Message = "Las credenciales enviadas son incorrectas." 
                });
            }

            User userFound = possibleUser;

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
                Value = new { 
                    userFound.IdUser, 
                    userFound.SessionToken 
                },
                Message = "Inicio de sesión exitoso." 
            });
        }

        // POST: api/Users/Historial
        [HttpPost]
        [Route("api/Users/Historial")]
        public IHttpActionResult UserHistorial(User user)
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

        public bool IsAdmin(string password)
        {
            return HashUtil.Verification(password, db.Users.Find(10).Password);
        }
    }
}