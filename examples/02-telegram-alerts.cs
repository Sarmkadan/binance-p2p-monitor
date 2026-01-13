#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using BinanceP2pMonitor.Services;
using BinanceP2pMonitor.Models;
using BinanceP2pMonitor.Constants;

namespace BinanceP2pMonitor.Examples;

/// <summary>
/// Multi-asset monitoring with Telegram notifications.
/// Creates alerts for price changes and sends notifications via Telegram bot.
/// Requires: TelegramBotToken and TelegramAdminChatId in appsettings.json
/// </summary>
class TelegramAlertsExample
{
    static async Task Main(string[] args)
    {
        var host = Host.CreateDefaultBuilder(args).Build();

        try
        {
            var alertService = host.Services.GetRequiredService<IAlertService>();
            var priceService = host.Services.GetRequiredService<IPriceMonitoringService>();
            var eventBus = host.Services.GetRequiredService<IEventBus>();

            Console.WriteLine("Setting up Telegram alerts...\n");

            // Create alerts for various trading pairs
            var alerts = new[]
            {
                new PriceAlert
                {
                    Asset = "BTC",
                    Fiat = "USD",
                    AlertType = AlertType.PriceChange,
                    LowerThreshold = -5.0m,          // Alert if BTC drops 5%
                    UpperThreshold = 3.0m,           // Alert if BTC rises 3%
                    UserId = "trader@example.com",
                    CooldownMinutes = 15,
                    IsActive = true
                },
                new PriceAlert
                {
                    Asset = "ETH",
                    Fiat = "USD",
                    AlertType = AlertType.PriceChange,
                    LowerThreshold = -4.0m,
                    UpperThreshold = 4.0m,
                    UserId = "trader@example.com",
                    CooldownMinutes = 10,
                    IsActive = true
                },
                new PriceAlert
                {
                    Asset = "BTC",
                    Fiat = "EUR",
                    AlertType = AlertType.SpreadAnomaly,
                    UpperThreshold = 2.0m,           // Alert if spread > 2%
                    UserId = "trader@example.com",
                    CooldownMinutes = 30,
                    IsActive = true
                }
            };

            // Create all alerts
            foreach (var alert in alerts)
            {
                var created = await alertService.CreateAlertAsync(alert);
                Console.WriteLine($"[✓] Created alert: {alert.Asset}/{alert.Fiat} " +
                    $"(ID: {created.Id})");
            }

            Console.WriteLine("\n[!] Monitoring prices and evaluating alerts...\n");

            // Subscribe to alert trigger events
            eventBus.Subscribe<PriceUpdatedEvent>(async @event =>
            {
                // Evaluate all alerts against the new price
                var triggered = await alertService.EvaluateAlertsAsync(@event.Price);

                foreach (var alert in triggered)
                {
                    var change = CalculatePriceChange(@event.Price);
                    Console.WriteLine(
                        $"[ALERT] {alert.Asset}/{alert.Fiat} triggered! " +
                        $"Change: {change:+0.00;-0.00}% | " +
                        $"Current: {@event.Price.Bid:F2}");
                }
            });

            // Monitor price updates
            var subscriptions = new List<string>();
            foreach (var alert in alerts)
            {
                var sub = await priceService.MonitorPriceAsync(
                    alert.Asset, alert.Fiat,
                    price => { /* Alert evaluation happens via event bus */ },
                    interval: TimeSpan.FromSeconds(30));

                subscriptions.Add(sub);
            }

            Console.WriteLine("Press Ctrl+C to stop monitoring...\n");

            // Keep monitoring
            while (true)
            {
                await Task.Delay(1000);
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[ERROR] {ex.Message}");
            Environment.Exit(1);
        }
        finally
        {
            host.Dispose();
        }
    }

    static decimal CalculatePriceChange(Price current)
    {
        // In real scenario, compare with baseline price
        return ((current.Ask - current.Bid) / current.Bid) * 100;
    }
}
