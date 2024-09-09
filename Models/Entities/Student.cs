using MVC.Models.Entities;

namespace NewWeb.Models.Entities
{
    public class Student
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Address { get; set; }

        public ICollection<StudentAssignment> StudentAssignments { get; set; }
    }
}