using System.Globalization;
using CsvHelper;
using ENSEK.API.Models;

namespace ENSEK.API.Data;

public static class SeedData
{
    public static async Task InitializeAsync(ApplicationDbContext context)
    {
        await context.Database.EnsureCreatedAsync();
        
        if (context.Accounts.Any())
            return;
        
        var accounts = new List<Account>();
        var csvPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "..", "..", "..", "..", "Test_Accounts.csv");
        
        if (!File.Exists(csvPath))
            csvPath = Path.Combine(Directory.GetCurrentDirectory(), "Test_Accounts.csv");
        
        if (File.Exists(csvPath))
        {
            using var reader = new StreamReader(csvPath);
            using var csv = new CsvReader(reader, CultureInfo.InvariantCulture);
            
            csv.Read();
            csv.ReadHeader();
            
            while (csv.Read())
            {
                accounts.Add(new Account
                {
                    AccountId = csv.GetField<int>("AccountId"),
                    FirstName = csv.GetField<string>("FirstName") ?? string.Empty,
                    LastName = csv.GetField<string>("LastName") ?? string.Empty
                });
            }

        await context.Accounts.AddRangeAsync(accounts);
        await context.SaveChangesAsync();
    }
}
}
