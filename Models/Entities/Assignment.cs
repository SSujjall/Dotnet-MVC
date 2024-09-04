using System.ComponentModel.DataAnnotations;

namespace MVC.Models.Entities
{
    public class Assignment
    {
        [Key]
        public int Id { get; set; }
        public string Title { get; set; }
        public DateOnly DueDate { get; set; }

        public ICollection<StudentAssignment> StudentAssignments { get; set; }
    }
}
