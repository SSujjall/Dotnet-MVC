using MVC.Models.Entities;
using NewWeb.Models.Entities;

namespace MVC.Models.DTO
{
    public class StudentAssignmentDTO
    {
        public Guid StudentId { get; set; }
        public List<Guid> StudentIds { get; set; }
        public string StudentName { get; set; }

        public int AssignmentId { get; set; }
        public string AssignmentTitle { get; set; }

        public DateOnly DueDate { get; set; }
        public DateOnly SubmissionDate { get; set; }


        // Additional properties for the add form
        public List<Student> Students { get; set; }
        public List<Assignment> Assignments { get; set; }
    }
}
