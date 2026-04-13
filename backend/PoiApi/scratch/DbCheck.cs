
using Microsoft.EntityFrameworkCore;
using PoiApi.Data;
using PoiApi.Models;
using PoiApi.Services;
using System.Text.Json;

public class DbChecker
{
    public static async Task Run(AppDbContext context)
    {
        var packages = await context.ServicePackages.ToListAsync();
        Console.WriteLine("=== SERVICE PACKAGES ===");
        foreach (var p in packages)
        {
            Console.WriteLine($"ID: {p.Id}, Name: {p.Name}, Tier: {p.Tier}, Tinder: {p.AllowTinderAccess}, AI: {p.AllowAiPlanAccess}, Chatbot: {p.AllowChatbotAccess}");
        }

        var subs = await context.Subscriptions
            .Include(s => s.ServicePackage)
            .OrderByDescending(s => s.CreatedAt)
            .Take(5)
            .ToListAsync();
        
        Console.WriteLine("\n=== RECENT SUBSCRIPTIONS ===");
        foreach (var s in subs)
        {
            Console.WriteLine($"ID: {s.Id}, User: {s.UserId}, Pkg: {s.ServicePackage.Name}, Status: {s.Status}, End: {s.EndDate}");
        }
    }
}
