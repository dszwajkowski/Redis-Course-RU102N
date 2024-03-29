﻿using System.Diagnostics;
using System.Globalization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Distributed;

namespace section3._2.Controllers;

[ApiController]
[Route("/api/[controller]")]
public class EmployeeController : ControllerBase
{
    private readonly SalesContext _salesDb;
    private readonly IDistributedCache _cache;

    public EmployeeController(SalesContext salesDb, IDistributedCache cache)
    {
        _cache = cache;
        _salesDb = salesDb;
    }

    [HttpGet("all")]
    public IEnumerable<Employee> GetEmployees()
    {
        return _salesDb.Employees;
    }

    [HttpGet("top")]
    public async Task<Dictionary<string,object>> GetTopSalesperson()
    {
        var stopwatch = Stopwatch.StartNew();

        var topSalesTask = _cache.GetStringAsync("top:sales");
        var topNameTask = _cache.GetStringAsync("top:name");
        await Task.WhenAll(topSalesTask, topNameTask);

        if (!string.IsNullOrEmpty(topSalesTask.Result) && !string.IsNullOrEmpty(topNameTask.Result))
        {
            stopwatch.Stop();
            return new Dictionary<string, object>
            {
                { "sum_sales", topSalesTask.Result },
                { "employee_name", topNameTask.Result },
                { "time", stopwatch.ElapsedMilliseconds }
            };
        }

        var topSalesperson = await _salesDb.Employees.Select(x=>new {Employee = x, sumSales = x.Sales
            .Sum(x=>x.Total)}).OrderByDescending(x=>x.sumSales)
            .FirstAsync();
        stopwatch.Stop();

        var cacheOptions = new DistributedCacheEntryOptions { AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(5) };
        var topSalesInsertTask = _cache.SetStringAsync("top:sales", topSalesperson.sumSales.ToString(), cacheOptions);
        var topNameInsertTask = _cache.SetStringAsync("top:name", topSalesperson.Employee.Name, cacheOptions);
        await Task.WhenAll(topSalesInsertTask, topNameInsertTask);

        return new Dictionary<string, object>
        {
            { "sum_sales", topSalesperson.sumSales },
            { "employee_name", topSalesperson.Employee.Name },
            { "time", stopwatch.ElapsedMilliseconds }
        };
    }

    [HttpGet("average/{id}")]
    public async Task<Dictionary<string,double>> GetAverage([FromRoute] int id)
    {
        var stopwatch = Stopwatch.StartNew();

        var key = $"employee:{id}:avg";
        var cacheResult = await _cache.GetStringAsync(key);

        if (cacheResult != null)
        {
            stopwatch.Stop();
            return new Dictionary<string, double>
            {
                { "average", double.Parse(cacheResult, CultureInfo.InvariantCulture) },
                { "elapsed", stopwatch.ElapsedMilliseconds }
            };
        }

        var avg = await _salesDb.Employees.Include(x => x.Sales)
            .Where(x=>x.EmployeeId == id)
            .Select(x=>x.Sales.Average(y=>y.Total))
            .FirstAsync();

        await _cache.SetStringAsync(key, avg.ToString(CultureInfo.InvariantCulture), 
            options: new DistributedCacheEntryOptions { SlidingExpiration = TimeSpan.FromMinutes(30) });

        stopwatch.Stop();
        return new Dictionary<string, double>
        {
            { "average", avg },
            { "elapsed", stopwatch.ElapsedMilliseconds }
        };
    }

    [HttpGet("totalSales")]
    public async Task<Dictionary<string, long>> GetTotalSales()
    {
        var stopwatch = Stopwatch.StartNew();

        var cacheResult = await _cache.GetStringAsync("totalSales");
        if (cacheResult != null)
        {
            stopwatch.Stop();
            return new Dictionary<string, long>
            {
                { "total_sales", long.Parse(cacheResult) },
                { "elapsed", stopwatch.ElapsedMilliseconds }
            };
        }

        var totalSales = await _salesDb.Sales.SumAsync(x => x.Total);

        await _cache.SetStringAsync("totalSales", totalSales.ToString(CultureInfo.InvariantCulture), 
            new DistributedCacheEntryOptions() { AbsoluteExpiration = DateTime.Today.AddDays(1) });

        stopwatch.Stop();
        return new Dictionary<string, long>
        {
            { "total_sales", totalSales },
            { "elapsed", stopwatch.ElapsedMilliseconds }
        }; 
    }
}