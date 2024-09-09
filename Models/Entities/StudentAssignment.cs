using NewWeb.Models.Entities;
using System.ComponentModel.DataAnnotations.Schema;

namespace MVC.Models.Entities
{
    public class StudentAssignment
    {
        public int StudentId { get; set; }
        public virtual Student Student { get; set; } // Navigation property

        public int AssignmentId { get; set; }
        public virtual Assignment Assignment { get; set; } // Navigation property

        public DateOnly SubmissionDate { get; set; }
    }
}
