using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Razor.TagHelpers;
using System.Data;
using System.Data.SqlClient;
using System.Data.SqlTypes;
using System.Xml.Linq;

namespace CurriculumMappingAPI.Controllers
{
    [ApiController]
    //[Route("[controller]")]
    public class SubjectController : ControllerBase
    {
        private readonly ILogger<SubjectController> _logger;

        public SubjectController(ILogger<SubjectController> logger)
        {
            _logger = logger;
        }


        [HttpGet]
        [Route("[controller]/x")]
        public object get()
        {
            return new { foo = "bar" };
        }

        [HttpGet]
        [Route("[controller]")]
        public object GetSubjects(string CourseId)
        {
            try
            {

                using SqlConnection con = new("Server=tcp:cmt.database.windows.net,1433;Initial Catalog=curriculumDb;Persist Security Info=False;User ID=sean;Password=getoff12!;MultipleActiveResultSets=False;Encrypt=True;TrustServerCertificate=False;Connection Timeout=30;");
                using SqlCommand cmd = new($"select s.* from CourseSubject cs join Subjects s on s.SubjectID=cs.SubjectID  where CourseId = '{CourseId}'", con);
                con.Open();
                SqlDataReader reader = cmd.ExecuteReader();

                List<Subject> subjects = new();
                List<Link> links = new();

                while (reader.Read())
                {
                    subjects.Add(new Subject
                    {
                        SubjectId = reader["SubjectId"].ToString(),
                        SubjectName = reader["SubjectTitle"].ToString(),
                        CreditPoints = int.Parse(reader["CreditPoints"].ToString()),
                        FirstSemRecomended = bool.Parse(reader["firstSemRecomended"].ToString()),
                    });

                    if (!reader.IsDBNull("Prerequsites"))
                    {
                        var preReqs = reader["Prerequsites"].ToString().Split(',');
                        foreach(string req in preReqs)
                        {
                            links.Add(new Link{
                                source = req,
                                target= reader["SubjectId"].ToString()
                            }
                            );
                        }
                    }
                }

                //Step to Clean Links (Remove Links to Subjects not included in selection) 
                foreach(var link in links.ToList())
                {
                    if(subjects.Find(s => s.SubjectId == link.source) == null)
                    {
                        links.Remove(link);
                    }
                }


                //This code may not be very good
                var rootSubjects = links.Select(l => subjects.Find(sub => sub.SubjectId == l.source && !links.Select(l => l.target).Contains(sub.SubjectId))).Distinct().Where(sub => sub != null);

                foreach (var subject in rootSubjects)
                {
                    List<Subject> visited = new();
                    subject.RootDescendenceDepth = GetRootDecendentsDepth(subjects, links, subject, visited);
                }

                return new
                {
                    subjects = subjects.OrderByDescending(s => s.RootDescendenceDepth).ToList().OrderByDescending(s => s.FirstSemRecomended),
                    links
                };
            }
            catch (Exception ex)
            {
                return ex.Message;
            }
        }

        [HttpGet]
        [Route("[controller]/GetSemesters")]
        public object GetSemesters()
        {
            return new { foo = "bar" };
        }

        private int GetRootDecendentsDepth(List<Subject> subjects, List<Link> links, Subject subject, List<Subject> visited)
        {
            if (visited.Contains(subject)){
                return 0;
            }

            visited.Add(subject);

            int maxChainLength = 0;

            var linksFromNode = links.Where(l => l.source == subject.SubjectId).ToList();
            var children = linksFromNode.Select(l => subjects.Find(s => s.SubjectId == l.target));

            foreach(var child in children)
            {
                maxChainLength = Math.Max(maxChainLength, 1 + GetRootDecendentsDepth(subjects, links, child, visited));
            }

            visited.Remove(subject);

            return maxChainLength;
        }
    }



    class Subject
    {
        public string SubjectName { get; set; }
        public string SubjectId { get; set; }
        public int CreditPoints { get; set; }
        public int RootDescendenceDepth { get; set; }
        public bool FirstSemRecomended { get; set; }
    }

    class Link
    {
        public string source { get; set; }
        public string target { get; set; }
    }

    
}
