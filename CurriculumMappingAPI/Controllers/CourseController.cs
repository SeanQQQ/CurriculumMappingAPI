using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Data.SqlClient;

namespace CurriculumMappingAPI.Controllers
{
    public class CourseController : Controller
    {

        [HttpGet]
        [Route("[controller]")]
        public object GetCourses()
        {
            try
            {
                using SqlConnection con = new("Server=tcp:cmt.database.windows.net,1433;Initial Catalog=curriculumDb;Persist Security Info=False;User ID=sean;Password=getoff12!;MultipleActiveResultSets=False;Encrypt=True;TrustServerCertificate=False;Connection Timeout=30;");
                using SqlCommand cmd = new($"select * from Courses", con);
                con.Open();
                SqlDataReader reader = cmd.ExecuteReader();

                List<Course> courses = new();

                while (reader.Read())
                {
                    courses.Add(new Course
                    {
                        CourseId = reader["CourseId"].ToString(),
                        CourseName = reader["CourseName"].ToString()
                    });
                }

                return new
                {
                    courses
                };
            }
            catch (Exception ex)
            {
                return ex.Message;
            }
        }
    }

    class Course
    {
        public string CourseName { get; set; }
        public string CourseId { get; set; }
    }
}
