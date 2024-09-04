using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.VisualBasic;
using MVC.Models.DTO;
using NewWeb.Data;
using NewWeb.Models.Entities;

namespace NewWeb.Controllers
{
    public class StudentController : Controller
    {
        private readonly AppDbContext _dbContext;

        public StudentController(AppDbContext dbContext)
        {
            this._dbContext = dbContext;
        }

        [HttpGet]
        public IActionResult Add()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> AddStudent(AddStudentViewModel viewModel)
        {
            var student = new Student
            {
                Name = viewModel.Name,
                Address = viewModel.Address,
            };

            await _dbContext.Students.AddAsync(student);

            await _dbContext.SaveChangesAsync();

            return RedirectToAction("GetStudents");
        }

        [HttpGet]
        public async Task<IActionResult> GetStudents()
        {
            var studentView = await _dbContext.Students.ToListAsync();

            return View(studentView);
        }

        //Edit
        [HttpGet]
        public async Task<IActionResult> Edit(Guid id)
        {
            var student = await _dbContext.Students.FindAsync(id);

            return View(student);
        }

        //Actual Edit Saving
        [HttpPost]
        public async Task<IActionResult> EditStudent(Student stuModel)
        {
            var student = await _dbContext.Students.FindAsync(stuModel.Id);

            if (student is not null)
            {
                student.Name = stuModel.Name;
                student.Address = stuModel.Address;

                await _dbContext.SaveChangesAsync();
            }

            return RedirectToAction("GetStudents", "Student"); //(RedirectPage, ControllerName)
        }

        //Deleting data
        [HttpPost]
        public async Task<IActionResult> Delete(Guid id)
        {
            var student = await _dbContext.Students.FirstOrDefaultAsync(x => x.Id == id);

            if (student is not null)
            {
                _dbContext.Students.Remove(student);
                await _dbContext.SaveChangesAsync();
            }

            return RedirectToAction("GetStudents", "Student");
        }
    }
}