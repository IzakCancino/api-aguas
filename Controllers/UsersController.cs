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
    public class UsersController : ApiController
    {
        private model_db db = new model_db();

        // GET: api/Users/Read
        [HttpGet]
        [Route("api/Users/Read")]
        public IQueryable<User> ReadUsers()
        {
            return db.Users;
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

            return Ok(user);
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
        public IHttpActionResult PostUser(User user)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            db.Users.Add(user);
            db.SaveChanges();

            return CreatedAtRoute("DefaultApi", new { id = user.IdUser }, user);
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
        public bool Login(User user)
        {
            return db.Users.Any(x => x.Email == user.Email && x.Password == user.Password && x.IsEnabled);
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