using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using StudentManagement.Models;
using System.Collections.Generic;
using System.Data;

namespace StudentManagement.Controllers
{
    public class StudentController : Controller
    {
        string cs;

        public StudentController(IConfiguration config)
        {
            cs = config.GetConnectionString("DefaultConnection");
        }

        // ================= INDEX =================
        public IActionResult Index()
        {
            List<dynamic> list = new();

            using SqlConnection con = new(cs);
            SqlCommand cmd = new("sp_GetStudentList", con);
            cmd.CommandType = CommandType.StoredProcedure;

            con.Open();
            SqlDataReader dr = cmd.ExecuteReader();
            while (dr.Read())
            {
                list.Add(new
                {
                    StudentId = dr["StudentId"],
                    Name = dr["FirstName"] + " " + dr["LastName"],
                    Age = dr["Age"],
                    Email = dr["Email"],
                    Country = dr["CountryName"],
                    State = dr["StateName"],
                    City = dr["CityName"]
                });

            }

            return View(list);
        }

        // ================= CREATE =================
        public IActionResult Create()
        {
            ViewBag.Country = GetCountry();
            return View();
        }

        [HttpPost]
        public IActionResult Create(Student s)
        {
            using SqlConnection con = new(cs);
            SqlCommand cmd = new("sp_InsertStudent", con);
            cmd.CommandType = CommandType.StoredProcedure;

            cmd.Parameters.AddWithValue("@FirstName", s.FirstName);
            cmd.Parameters.AddWithValue("@LastName", s.LastName);
            cmd.Parameters.AddWithValue("@Age", s.Age);
            cmd.Parameters.AddWithValue("@Email", s.Email);
            cmd.Parameters.AddWithValue("@CountryId", s.CountryId);
            cmd.Parameters.AddWithValue("@StateId", s.StateId);
            cmd.Parameters.AddWithValue("@CityId", s.CityId);

            con.Open();
            cmd.ExecuteNonQuery();

            return RedirectToAction("Index");
        }

        public IActionResult Edit(int id)
        {
            Student s = new();

            using SqlConnection con = new(cs); 
            SqlCommand cmd = new("sp_GetStudentById", con);
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.Parameters.AddWithValue("@StudentId", id);

            con.Open();
            SqlDataReader dr = cmd.ExecuteReader();
            if (dr.Read())
            {
                s.StudentId = Convert.ToInt32(dr["StudentId"]);
                s.FirstName = dr["FirstName"].ToString();
                s.LastName = dr["LastName"].ToString();
                s.Age = Convert.ToInt32(dr["Age"]);
                s.Email = dr["Email"].ToString();
                s.CountryId = Convert.ToInt32(dr["CountryId"]);
                s.StateId = Convert.ToInt32(dr["StateId"]);
                s.CityId = Convert.ToInt32(dr["CityId"]);
            }
            dr.Close();

            // 🔹 Load dropdowns
            ViewBag.Country = GetCountry();
            ViewBag.State = GetStateByCountry(s.CountryId);
            ViewBag.City = GetCityByState(s.StateId);

            return View(s);
        }


        [HttpPost]
        public IActionResult Edit(Student s)
        {
            using SqlConnection con = new(cs);
            SqlCommand cmd = new("sp_UpdateStudent", con);
            cmd.CommandType = CommandType.StoredProcedure;

            cmd.Parameters.AddWithValue("@StudentId", s.StudentId);
            cmd.Parameters.AddWithValue("@FirstName", s.FirstName);
            cmd.Parameters.AddWithValue("@LastName", s.LastName);
            cmd.Parameters.AddWithValue("@Age", s.Age);
            cmd.Parameters.AddWithValue("@Email", s.Email);
            cmd.Parameters.AddWithValue("@CountryId", s.CountryId);
            cmd.Parameters.AddWithValue("@StateId", s.StateId);
            cmd.Parameters.AddWithValue("@CityId", s.CityId);

            con.Open();
            cmd.ExecuteNonQuery();   // ✅ now no error

            return RedirectToAction("Index");
        }


        public IActionResult Delete(int id)
        {
            using SqlConnection con = new(cs);
            SqlCommand cmd = new("sp_DeleteStudent", con);
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.Parameters.AddWithValue("@StudentId", id);

            con.Open();
            cmd.ExecuteNonQuery();

            return RedirectToAction("Index");
        }



        // ================= AJAX =================
        public JsonResult GetState(int id)
        {
            List<SelectListItem> list = new();

            using SqlConnection con = new(cs);
            SqlCommand cmd = new("sp_GetStateByCountry", con);
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.Parameters.AddWithValue("@CountryId", id);

            con.Open();
            SqlDataReader dr = cmd.ExecuteReader();
            while (dr.Read())
            {
                list.Add(new SelectListItem
                {
                    Text = dr["StateName"].ToString(),
                    Value = dr["StateId"].ToString()
                });
            }

            return Json(list);
        }

        public JsonResult GetCity(int id)
        {
            List<SelectListItem> list = new();

            using SqlConnection con = new(cs);
            SqlCommand cmd = new("sp_GetCityByState", con);
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.Parameters.AddWithValue("@StateId", id);

            con.Open();
            SqlDataReader dr = cmd.ExecuteReader();
            while (dr.Read())
            {
                list.Add(new SelectListItem
                {
                    Text = dr["CityName"].ToString(),
                    Value = dr["CityId"].ToString()
                });
            }

            return Json(list);
        }

        // ================= HELPER =================
        List<SelectListItem> GetCountry()
        {
            List<SelectListItem> list = new();

            using SqlConnection con = new(cs);
            SqlCommand cmd = new("sp_GetCountry", con);
            cmd.CommandType = CommandType.StoredProcedure;

            con.Open();
            SqlDataReader dr = cmd.ExecuteReader();
            while (dr.Read())
            {
                list.Add(new SelectListItem
                {
                    Text = dr["CountryName"].ToString(),
                    Value = dr["CountryId"].ToString()
                });
            }

            return list;
        }
        List<SelectListItem> GetStateByCountry(int countryId)
        {
            List<SelectListItem> list = new();

            using SqlConnection con = new(cs);
            SqlCommand cmd = new("sp_GetStateByCountry", con);
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.Parameters.AddWithValue("@CountryId", countryId);

            con.Open();
            SqlDataReader dr = cmd.ExecuteReader();
            while (dr.Read())
            {
                list.Add(new SelectListItem
                {
                    Text = dr["StateName"].ToString(),
                    Value = dr["StateId"].ToString()
                });
            }
            return list;
        }

        List<SelectListItem> GetCityByState(int stateId)
        {
            List<SelectListItem> list = new();

            using SqlConnection con = new(cs);
            SqlCommand cmd = new("sp_GetCityByState", con);
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.Parameters.AddWithValue("@StateId", stateId);

            con.Open();
            SqlDataReader dr = cmd.ExecuteReader();
            while (dr.Read())
            {
                list.Add(new SelectListItem
                {
                    Text = dr["CityName"].ToString(),
                    Value = dr["CityId"].ToString()
                });
            }
            return list;
        }

    }
}
