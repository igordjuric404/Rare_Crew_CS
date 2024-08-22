using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

public class EmployeeController : Controller
{
    private readonly EmployeeService _employeeService;

    public EmployeeController(EmployeeService employeeService)
    {
        _employeeService = employeeService;
    }

    public async Task<IActionResult> Index()
    {
        var employees = await _employeeService.GetEmployeesAsync();

        var chartService = new ChartService();
        string outputPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "images", "EmployeeWorkPieChart.png");
        chartService.GeneratePieChart(employees, outputPath);

        return View(employees);
    }

}
