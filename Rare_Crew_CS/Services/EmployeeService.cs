using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using Rare_Crew_CS.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;

public class EmployeeService
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<EmployeeService> _logger;
    private readonly string _apiKey;

    public EmployeeService(HttpClient httpClient, ILogger<EmployeeService> logger, IOptions<ApiSettings> apiSettings)
    {
        _httpClient = httpClient;
        _logger = logger;
        _apiKey = apiSettings.Value.EmployeeServiceApiKey; // Retrieve API key from configuration
    }

    public async Task<List<Employee>> GetEmployeesAsync()
    {
        string url = $"https://rc-vault-fap-live-1.azurewebsites.net/api/gettimeentries?code={_apiKey}";

        try
        {
            var response = await _httpClient.GetStringAsync(url);

            // Deserializes JSON response into a list of EmployeeEntry objects
            var timeEntries = JsonConvert.DeserializeObject<List<EmployeeEntry>>(response);

            if (timeEntries == null)
            {
                _logger.LogWarning("No time entries were returned from the API.");
                return new List<Employee>();
            }

            // Processes the list to group entries by employee and calculate total hours
            var employees = timeEntries
                .GroupBy(e => string.IsNullOrEmpty(e.EmployeeName) ? "Unknown Employee" : e.EmployeeName)
                .Select(g => new Employee
                {
                    Name = g.Key,
                    TotalTimeWorked = Math.Round(g.Sum(e =>
                    {
                        DateTime start = DateTime.Parse(e.StarTimeUtc);
                        DateTime end = DateTime.Parse(e.EndTimeUtc);
                        return CalculateTotalHours(start, end);
                    }), 2),
                    EntryNotes = string.Join("; ", g.Select(e => e.EntryNotes)) // Concatenates entry notes
                })
                .OrderByDescending(e => e.TotalTimeWorked) // Orders by total time worked
                .ToList();

            _logger.LogInformation("Processed employees: {@employees}", employees);
            return employees;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get employees");
            return new List<Employee>();
        }
    }


    private double CalculateTotalHours(DateTime start, DateTime end)
    {
        return (end - start).TotalHours;
    }
}


public class EmployeeEntry
{
    public required string EmployeeName { get; set; }
    public required string StarTimeUtc { get; set; }
    public required string EndTimeUtc { get; set; }
    public required string EntryNotes { get; set; }
}
