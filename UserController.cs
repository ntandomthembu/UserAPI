using System.Web.Http.Cors;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Dynamic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Web;
using System.Web.Http;
using System.Web.Mvc;
using User_API.Models;
using System.Globalization;
using User_API.ViewModels;

namespace User_API.Controllers
{
    [EnableCors(origins: "*", headers: "*", methods: "*")]
    public class UserController : ApiController
    {
        Tuks_Athletics_SystemsEntities db = new Tuks_Athletics_SystemsEntities();
        //
        //Register
        //
        [System.Web.Mvc.Route("api/User/RegisterUser")]
        [System.Web.Http.HttpPost]

        public object RegisterUser(User user)
        {
            
                User usr = new User();
                db.Configuration.ProxyCreationEnabled = false;

                if (usr.User_ID == 0)
                {
                    var hash = GenerateHash(Spice(user.Password));
                    usr.Password = hash;
                    usr.Email = user.Email;
                usr.Name = user.Name;
                usr.Surname = user.Surname;
                usr.Phone_Number = user.Phone_Number;
                    Guid g = Guid.NewGuid();
                   // usr.Session = g.ToString();
                    ///usr.User_Type = user.User_Type;
                   

                    db.Users.Add(usr);
                    db.SaveChanges();
                    return usr;
                   
                }
            return usr;
          

        }

        [System.Web.Http.Route("api/User/GetAllUsers")]
        [System.Web.Mvc.HttpPost]
        [System.Web.Http.Authorize(Roles = "Admin")]
        public List<dynamic> GetAllUsers()
        {
            //var Token = Request.Headers.FirstOrDefault(k => k.Key.Equals("Authorization")).Value;

            //var identity = (ClaimsIdentity)User.Identity;
            //IEnumerable<Claim> claims = identity.Claims;

            db.Configuration.ProxyCreationEnabled = false;


            return GetUserReturnList(db.Users.ToList());
        }

        private List<dynamic> GetUserReturnList(List<User> ForClent)
        {
            List<dynamic> DynamicUsers = new List<dynamic>();

            foreach (User user in ForClent)
            {
                dynamic dynamicUser = new ExpandoObject();

                dynamicUser.User_ID = user.User_ID;
                dynamicUser.Name = user.Name;
                dynamicUser.Surname = user.Surname;
                dynamicUser.Email = user.Email;
                dynamicUser.User_role = user.User_role;
                dynamicUser.Phone_Number = user.Phone_Number;


                DynamicUsers.Add(dynamicUser);
            }

            return DynamicUsers;
        }

        //
        //LOGIN

        //----------------------------- User LogIn ------------------------//

        [System.Web.Http.AllowAnonymous]
        [System.Web.Http.Route("api/User/LogIn")]
        [System.Web.Mvc.HttpPost]
        public string LogIn([FromBody] LoginViewModel LogIn)
        {

            Tuks_Athletics_SystemsEntities db = new Tuks_Athletics_SystemsEntities();
            db.Configuration.ProxyCreationEnabled = false;

            User CurrentUser = db.Users.Where(u => u.Email == LogIn.Email && u.Password == ComputeSha256Hash(LogIn.Password)).FirstOrDefault();

            if (CurrentUser == null)
            {
                return "Not Found";
            }
            else
            {
                return "Found";
            }


        }

        [System.Web.Http.AllowAnonymous]
        [System.Web.Http.Route("api/User/GetUserRole")]
        [System.Web.Mvc.HttpGet]
        public string GetUserRole(string UserEmail)
        {

            Tuks_Athletics_SystemsEntities db = new Tuks_Athletics_SystemsEntities();
            db.Configuration.ProxyCreationEnabled = false;

            string UserRole = db.Users.Where(usr => usr.Email == UserEmail).FirstOrDefault().User_role;

            return UserRole;


        }
        //----------------------------- Add User ------------------------//

        [System.Web.Http.AllowAnonymous]
        [System.Web.Http.Route("api/User/AddUser")]
        [System.Web.Mvc.HttpPost]
        public bool AddUser([FromBody] User user)
        {

            if (user != null)
            {
                db.Configuration.ProxyCreationEnabled = false;

                user.Name = user.Name;
                var HashedPassword = ComputeSha256Hash(user.Password);
                user.Password = HashedPassword;

                db.Users.Add(user);
                db.SaveChanges();

                return (true);
            }
            return (false);
        }

        string ComputeSha256Hash(string RawData) // I prefer my data RAW, no salt for me please
        {
            using (SHA256 Sha256Hash = SHA256.Create())
            {
                byte[] pBytes = Sha256Hash.ComputeHash(Encoding.UTF8.GetBytes(RawData));

                StringBuilder MyPassword = new StringBuilder();
                for (int i = 0; i < RawData.Length; i++)
                {
                    MyPassword.Append(pBytes[i].ToString("x2"));
                }
                return MyPassword.ToString();
            }
        }

        //---------------Update User --------------------------//

        [System.Web.Http.Route("api/Values/UpdateUser")]
        [System.Web.Mvc.HttpPost]
        [System.Web.Http.Authorize(Roles = "Admin")]
        public bool UpdateUser([FromBody] User user)
        {
            db.Configuration.ProxyCreationEnabled = false;
            db.Entry(user).State = System.Data.Entity.EntityState.Modified;
            db.SaveChanges();

            return true;
        }

        //------ Delet User ----------------- //

        [System.Web.Http.Route("api/Values/DeleteUser")]
        [System.Web.Mvc.HttpDelete]
        [System.Web.Http.Authorize(Roles = "Admin")]
        public bool DeleteUser(int Id)
        {
            db.Configuration.ProxyCreationEnabled = false;

            User UserToDelete = db.Users.Where(u => u.User_ID == Id).FirstOrDefault();

            db.Users.Remove(UserToDelete);
            db.SaveChanges();

            return true;
        }



        //Hashing
        public static string Spice(string input)
        {
            return input + "we678etyui7723drthml";
        }

        public static string GenerateHash(string input)
        {
            SHA256 sHA = SHA256Managed.Create();
            byte[] bytes = Encoding.UTF8.GetBytes(input);
            byte[] hash = sHA.ComputeHash(bytes);
            return GetFromHash(hash);
        }
        private static string GetFromHash(byte[] hash)
        {
            StringBuilder result = new StringBuilder();
            for (int i = 0; i < hash.Length; i++)
            {
                result.Append(hash[i].ToString("X2"));
            }
            return result.ToString();
        }
        public static string RandomString(int length)
        {
            Random myRandom = new Random();
            const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
            return new string(Enumerable.Repeat(chars, length).Select(s => s[myRandom.Next(s.Length)]).ToArray());
        }
      
    }
}