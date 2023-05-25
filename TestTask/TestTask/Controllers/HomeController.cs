using CsvHelper;
using CsvHelper.Configuration;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Globalization;
using TestTask.Models;

namespace TestTask.Controllers
{
    public class HomeController : Controller
    {
        private readonly IEmployeeRepository _employeeRepository;

        public HomeController(IEmployeeRepository employeeRepository)
        {
            _employeeRepository = employeeRepository;
        }

        public async Task<IActionResult> Index() 
        {
            var employees = await _employeeRepository.GetAllAsync();

            return View(employees);
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
                        MissingFieldFound = null
                    };

                    using (var csvReader = new CsvReader(reader, csvConfig))
                    {
                        var employees = csvReader.GetRecords<Employee>().ToList();

                        foreach (var employee in employees)
                        {
                            await _employeeRepository.AddAsync(employee);
                        }
                    }
                }
            }

            return RedirectToAction("Index");
        }

        [HttpPost]
        public async Task<IActionResult> Update(int id, string field, string value)
        {
            var employee = await _employeeRepository.GetByIdAsync(id);
            if (employee == null)
            {
                return NotFound();
            }

            var property = typeof(Employee).GetProperty(field);

            if (property != null)
            {
                if (property.PropertyType == typeof(bool))
                {
                    property.SetValue(employee, bool.Parse(value));
                }
                else if (property.PropertyType == typeof(decimal))
                {
                    property.SetValue(employee, decimal.Parse(value));
                }
                else if (property.PropertyType == typeof(DateTime))
                {
                    property.SetValue(employee, DateTime.Parse(value));
                }
                else
                {
                    property.SetValue(employee, value);
                }

                await _employeeRepository.UpdateAsync(employee);
            }

            return RedirectToAction("Index");
        }
    }
}