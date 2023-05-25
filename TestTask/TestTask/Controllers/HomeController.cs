using CsvHelper;
using CsvHelper.Configuration;
using Microsoft.AspNetCore.Mvc;
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
    }
}