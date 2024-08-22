using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Rare_Crew_CS.Models;

public class EmployeeService
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<EmployeeService> _logger;

    public EmployeeService(HttpClient httpClient, ILogger<EmployeeService> logger)
    {
        _httpClient = httpClient;
        _logger = logger;
    }
    public async Task<List<Employee>> GetEmployeesAsync()
    {
        string apiKey = "vO17RnE8vuzXzPJo5eaLLjXjmRW07law99QTD90zat9FfOQJKKUcgQ==";
        string url = $"https://rc-vault-fap-live-1.azurewebsites.net/api/gettimeentries?code={apiKey}";

        _logger.LogInformation("Fetching data from API...");
        var response = await _httpClient.GetStringAsync(url);
        _logger.LogInformation("Raw JSON response: {response}", response);

        var timeEntries = JsonConvert.DeserializeObject<List<dynamic>>(response);

        var employees = timeEntries
            .GroupBy(e => (string.IsNullOrEmpty((string)e.EmployeeName) ? "Unknown Employee" : (string)e.EmployeeName))
            .Select(g => new Employee
            {
                Name = g.Key,
                TotalTimeWorked = Math.Round(g.Sum(e =>
                {
                    DateTime start = DateTime.Parse((string)e.StarTimeUtc);
                    DateTime end = DateTime.Parse((string)e.EndTimeUtc);
                    return (end - start).TotalHours;
                }), 2),
                EntryNotes = string.Join("; ", g.Select(e => (string)e.EntryNotes))
            })
            .OrderByDescending(e => e.TotalTimeWorked)
            .ToList();

        _logger.LogInformation("Processed employees: {@employees}", employees);

        return employees;
    }



}
