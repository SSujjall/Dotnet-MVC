using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.EntityFrameworkCore;
using MVC.Models.DTO;
using MVC.Models.Entities;
using NewWeb.Data;
using NewWeb.Models.Entities;
using System.Data.Common;

namespace MVC.Controllers
{
    public class StudentAssignmentController : Controller
    {
        private readonly AppDbContext _dbContext;

        public StudentAssignmentController(AppDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<IActionResult> Index()
        {
            var data = await (from sa in _dbContext.StudentAssignments
                              join s in _dbContext.Students on sa.StudentId equals s.Id
                              join a in _dbContext.Assignments on sa.AssignmentId equals a.Id
                              select new StudentAssignmentDTO
                              {
                                  StudentId = s.Id,
                                  StudentName = s.Name,
                                  AssignmentId = a.Id,
                                  AssignmentTitle = a.Title,
                                  DueDate = a.DueDate,
                                  SubmissionDate = sa.SubmissionDate
                              }).ToListAsync();

            var formData = new StudentAssignmentDTO
            {
                Students = await _dbContext.Students.ToListAsync(),
                Assignments = await _dbContext.Assignments.ToListAsync()
            };

            var model = new Tuple<List<StudentAssignmentDTO>, StudentAssignmentDTO>(data, formData);
            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> Add(StudentAssignmentDTO model)
        {
            foreach(var studentId in model.StudentIds)
            {
                var submission = new StudentAssignment
                {
                    StudentId = studentId,
                    AssignmentId = model.AssignmentId,
                    SubmissionDate = model.SubmissionDate,
                };

                await _dbContext.StudentAssignments.AddAsync(submission);
            }    

            await _dbContext.SaveChangesAsync();

            // Return the added submissions to the view.
            var addedSubmissions = from studentId in model.StudentIds
                                   let student = _dbContext.Students.Find(studentId)
                                   let assignment = _dbContext.Assignments.Find(model.AssignmentId)
                                   select new
                                   {
                                       studentId,
                                       studentName = student.Name,
                                       assignmentId = model.AssignmentId,
                                       assignmentTitle = assignment.Title,
                                       dueDate = assignment.DueDate.ToString("M/d/yyyy"),
                                       submissionDate = model.SubmissionDate.ToString("M/d/yyyy")
                                   };

            return Json(addedSubmissions);
        }

        [HttpPost]
        public async Task<IActionResult> Delete(int studentId, int assignmentId)
        {
            var submission = await _dbContext.StudentAssignments
               .FirstOrDefaultAsync(sa => sa.StudentId == studentId && sa.AssignmentId == assignmentId);

            if (submission != null)
            {
                _dbContext.StudentAssignments.Remove(submission);
                await _dbContext.SaveChangesAsync();
                return Ok();
            }

            return NotFound();
        }
    }
}