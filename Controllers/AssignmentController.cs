using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.VisualBasic;
using MVC.Models.DTO;
using MVC.Models.Entities;
using NewWeb.Data;

namespace MVC.Controllers
{
    public class AssignmentController : Controller
    {
        private readonly AppDbContext _dbContext;

        public AssignmentController(AppDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<IActionResult> Index()
        {
            var studentView = await _dbContext.Assignments.ToListAsync();

            return View(studentView);
        }

        [HttpPost]
        public async Task<IActionResult> AddAssignment(AssignmentDTO dto)
        {
            var assignment = new Assignment
            {
                Title = dto.Title,
                DueDate = dto.DueDate
            };

            await _dbContext.Assignments.AddAsync(assignment);
            await _dbContext.SaveChangesAsync();
            return Json(new
            {
                id = assignment.Id,
                title = assignment.Title,
                dueDate = assignment.DueDate.ToString("M/d/yyyy")
            });
        }

        public async Task<IActionResult> Edit(int id)
        {
            var student = await _dbContext.Assignments.FindAsync(id);

            return View(student);
        }

        [HttpPost]
        public async Task<IActionResult> EditAssignment(Assignment model)
        {
            var assignment = await _dbContext.Assignments.FindAsync(model.Id);

            if (assignment is not null)
            {
                assignment.Title = model.Title;
                assignment.DueDate = model.DueDate;

                await _dbContext.SaveChangesAsync();
            }

            return RedirectToAction("Index", "Assignment");
        }

        [HttpPost]
        public async Task<IActionResult> DeleteAssignment(int id)
        {
            var assignment = await _dbContext.Assignments.FirstOrDefaultAsync(x => x.Id == id);

            if (assignment is not null)
            {
                _dbContext.Assignments.Remove(assignment);
                await _dbContext.SaveChangesAsync();
            }

            return RedirectToAction("Index", "Assignment");
        }
    }
}
