using CsvHelper;
using CsvHelper.Configuration;
using FluentValidation;
using Microsoft.AspNetCore.Mvc;
using System.Globalization;
using TestTask.Models;

namespace TestTask.Controllers
{
    public class HomeController : Controller
    {
        private readonly IEmployeeRepository _employeeRepository;
        private readonly IValidator<Employee> _validator;

        public HomeController(IEmployeeRepository employeeRepository, IValidator<Employee> validator)
        {
            _employeeRepository = employeeRepository;
            _validator = validator;
        }

        public async Task<IActionResult> Index(int page = 1)
        {
            var employees = await _employeeRepository.GetAllAsync();

            var paginationData = Pagination(employees, page);

            return View(paginationData);
        }

        [HttpPost]
        public async Task<IActionResult> Upload(IFormFile file)
        {
            if (file != null && file.Length > 0)
            {
                using (var reader = new StreamReader(file.OpenReadStream()))
                {
                    var csvConfig = new CsvConfiguration(CultureInfo.InvariantCulture)
                    {
                        Delimiter = ";",
                        HasHeaderRecord = true,
                        HeaderValidated = null,
                        MissingFieldFound = null,
                        TrimOptions = TrimOptions.Trim
                    };

                    using (var csvReader = new CsvReader(reader, csvConfig))
                    {
                        var employees = csvReader.GetRecords<Employee>().ToList();

                        int counter = 0;

                        foreach (var employee in employees)
                        {
                            counter++;
                            var validationResult = await _validator.ValidateAsync(employee);

                            if (validationResult.IsValid)
                            {
                                await _employeeRepository.AddAsync(employee);
                            }
                            else
                            {
                                var validationErrors = validationResult.Errors.Select(e => e.ErrorMessage + $" Line {counter}.").ToList();
                                return Json(new { success = false, errors = validationErrors });
                            }
                        }
                    }

                    return Json(new { success = true });
                }
            }

            return Json(new { success = false });
        }

        [HttpPost]
        public async Task<IActionResult> Update(int id, string field, string value)
        {
            var employee = await _employeeRepository.GetByIdAsync(id);
            if (employee == null)
            {
                return NotFound();
            }

            switch (field)
            {
                case "Name":
                    employee.Name = value;
                    break;
                case "DateOfBirth":
                    if (DateTime.TryParse(value, out DateTime dateOfBirth))
                    {
                        employee.DateOfBirth = dateOfBirth;
                    }
                    else
                    {
                        ModelState.AddModelError(field, "Invalid Date of Birth");
                    }
                    break;
                case "Married":
                    if (bool.TryParse(value, out bool married))
                    {
                        employee.Married = married;
                    }
                    else
                    {
                        ModelState.AddModelError(field, "Invalid Married value");
                    }
                    break;
                case "Phone":
                    employee.Phone = value;
                    break;
                case "Salary":
                    if (decimal.TryParse(value, out decimal salary))
                    {
                        employee.Salary = salary;
                    }
                    else
                    {
                        ModelState.AddModelError(field, "Invalid Salary value");
                    }
                    break;
            }

            if (ModelState[field] == null)
            {
                var validationResult = await _validator.ValidateAsync(employee);

                if (!validationResult.IsValid) 
                {
                    var validationErrors = new List<string>();

                    foreach (var error in validationResult.Errors)
                    {
                        validationErrors.Add(error.ErrorMessage);
                    }

                    return Json(new { success = false, errors = validationErrors });
                }

                await _employeeRepository.UpdateAsync(employee);
                return Json(new { success = true });
            }

            var errors = ModelState[field].Errors.Select(e => e.ErrorMessage).ToList();
            return Json(new { success = false, errors });
        }

        [HttpPost]
        public async Task<IActionResult> Delete(int id)
        {
            var employee = await _employeeRepository.GetByIdAsync(id);

            if (employee == null)
            {
                return Json(new { success = false, errors = "Employee with this id doesn't exist." });
            }

            await _employeeRepository.DeleteAsync(employee);

            return Json(new { success = true });
        }

        private IEnumerable<Employee> Pagination(IEnumerable<Employee> data, int page)
        {
            const int pageSize = 10;

            if (page < 1)
            {
                page = 1;
            }

            int recsCount = data.Count();

            var pager = new Pager(recsCount, page, pageSize);

            int recSkip = (page - 1) * pageSize;

            data = data.Skip(recSkip).Take(pager.PageSize).ToList();

            ViewBag.Pager = pager;

            return data;
        }
    }
}