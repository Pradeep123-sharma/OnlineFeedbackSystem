using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OnlineFeedbackSystem.Models.Entities;
using OnlineFeedbackSystem.Repositories;
using OnlineFeedbackSystem.Models.ViewModels;

namespace OnlineFeedbackSystem.Controllers
{
    [Authorize(Roles = "SuperAdmin")]
    public class DepartmentController : Controller
    {
        private readonly IDepartmentRepository _departmentRepository;

        public DepartmentController(IDepartmentRepository departmentRepository)
        {
            _departmentRepository = departmentRepository;
        }

        public IActionResult Index()
        {
            ViewData["ActiveNav"] = "Departments";
            ViewData["PageTitle"] = "Departments";
            ViewData["Title"] = "Departments — Pulse";

            var departments = _departmentRepository.GetAll()
                .Select(d => new DepartmentViewModel
                {
                    DepartmentId = d.DepartmentId,
                    DepartmentName = d.DepartmentName,
                    IsActive = d.IsActive
                })
                .ToList();

            return View(departments);
        }

        [HttpGet]
        public IActionResult Create()
        {
            ViewData["ActiveNav"] = "Departments";
            ViewData["PageTitle"] = "Add department";
            ViewData["Title"] = "Add Department — Pulse";
            return View(new DepartmentViewModel());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Create(DepartmentViewModel model)
        {
            if (!ModelState.IsValid)
            {
                TempData["Error"] = "Department name is required.";
                return View(model);
            }

            var department = new Departments
            {
                DepartmentName = model.DepartmentName.Trim(),
                IsActive = model.IsActive
            };

            int newId = _departmentRepository.Create(department);

            if (newId <= 0)
            {
                TempData["Error"] = "Department already exists.";
                return View(model);
            }

            TempData["Success"] = "Department created successfully.";
            return RedirectToAction(nameof(Index));
        }

        [HttpGet]
        public IActionResult Edit(int id)
        {
            ViewData["ActiveNav"] = "Departments";
            ViewData["PageTitle"] = "Edit department";
            ViewData["Title"] = "Edit Department — Pulse";

            var dept = _departmentRepository.GetById(id);
            if (dept == null)
            {
                TempData["Error"] = "Department not found.";
                return RedirectToAction(nameof(Index));
            }

            var model = new DepartmentViewModel
            {
                DepartmentId = dept.DepartmentId,
                DepartmentName = dept.DepartmentName,
                IsActive = dept.IsActive
            };

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Edit(int id, DepartmentViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var dept = new Departments
            {
                DepartmentId = id,
                DepartmentName = model.DepartmentName.Trim(),
                IsActive = model.IsActive
            };

            bool updated = _departmentRepository.Update(dept);
            if (!updated)
            {
                TempData["Error"] = "Could not update department.";
                return View(model);
            }

            TempData["Success"] = "Department updated successfully.";
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult ToggleStatus(int id)
        {
            bool toggled = _departmentRepository.ToggleStatus(id);
            if (toggled)
            {
                TempData["Success"] = "Department status updated successfully.";
            }
            else
            {
                TempData["Error"] = "Could not update department status.";
            }
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Deactivate(int id) => ToggleStatus(id);

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Activate(int id) => ToggleStatus(id);
    }
}
