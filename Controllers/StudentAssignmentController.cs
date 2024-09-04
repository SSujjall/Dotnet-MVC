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
            var submission = new StudentAssignment
            {
                StudentId = model.StudentId,
                AssignmentId = model.AssignmentId,
                SubmissionDate = model.SubmissionDate,
            };

            await _dbContext.StudentAssignments.AddAsync(submission);
            await _dbContext.SaveChangesAsync();

            var result = new StudentAssignmentDTO
            {
                StudentId = model.StudentId,
                StudentName = (await _dbContext.Students.FindAsync(model.StudentId)).Name,
                AssignmentId = model.AssignmentId,
                AssignmentTitle = (await _dbContext.Assignments.FindAsync(model.AssignmentId)).Title,
                DueDate = (await _dbContext.Assignments.FindAsync(model.AssignmentId)).DueDate,
                SubmissionDate = model.SubmissionDate,
            };

            return Json(new
            {
                studentId = result.StudentId,
                studentName = result.StudentName,
                assignmentId = result.AssignmentId,
                assignmentTitle = result.AssignmentTitle,
                dueDate = result.DueDate.ToString("M/d/yyyy"),
                submissionDate = result.SubmissionDate.ToString("M/d/yyyy")
            });
        }

        [HttpPost]
        public async Task<IActionResult> Delete(Guid studentId, int assignmentId)
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