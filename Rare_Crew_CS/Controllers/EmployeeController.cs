using Microsoft.AspNetCore.Mvc;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

public class EmployeeController : Controller
{
    private readonly EmployeeService _employeeService;

    public EmployeeController(EmployeeService employeeService)
    {
        _employeeService = employeeService;
    }

    public async Task<IActionResult> Index(string sortOrder = "desc")
    {
        var employees = await _employeeService.GetEmployeesAsync();

        // Sort based on the provided sortOrder parameter
        if (sortOrder == "asc")
        {
            employees = employees.OrderBy(e => e.TotalTimeWorked).ToList();
        }
        else
        {
            employees = employees.OrderByDescending(e => e.TotalTimeWorked).ToList();
        }

        // Generate the pie chart
        var chartService = new ChartService();
        string outputPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "images", "EmployeeWorkPieChart.png");
        chartService.GeneratePieChart(employees, outputPath);

        ViewBag.CurrentSortOrder = sortOrder;
        return View(employees);
    }
}
