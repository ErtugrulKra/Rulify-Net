using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Rulify.Net;
using Rulify.Net.Json;

namespace Rulify.Net.Examples
{
    /// <summary>
    /// Example demonstrating JSON rule loading
    /// </summary>
    public class JsonRulesExample
    {
        public static Task RunExample()
        {
            Console.WriteLine("JSON Rules Example");
            Console.WriteLine("==================");

            // Sample JSON rules
            var jsonRules = @"{
  ""rules"": [
    {
      ""id"": ""hotel_price"",
      ""name"": ""Hotel Price Calculation"",
      ""description"": ""Hafta içi/sonu otel fiyat farkı ve haftalık indirim"",
      ""priority"": 1,
      ""conditions"": {
        ""all"": [
          { ""var"": ""stay_days"", "">="": 1 }
        ]
      },
      ""actions"": [
        {
          ""type"": ""compute"",
          ""expression"": ""hotel_total = (weekdays * base_rate_weekday) + (weekends * base_rate_weekend)""
        },
        {
          ""type"": ""if"",
          ""condition"": ""stay_days >= 7 and start_day == 'Monday'"",
          ""then"": ""hotel_total *= 0.9""
        }
      ]
    },
    {
      ""id"": ""child_discounts"",
      ""name"": ""Child Discounts"",
      ""description"": ""Çocuk yaşına göre konaklama indirimi"",
      ""priority"": 2,
      ""foreach"": ""children"",
      ""actions"": [
        {
          ""type"": ""if"",
          ""condition"": ""item <= 4"",
          ""then"": ""child_price = 0""
        },
        {
          ""type"": ""if"",
          ""condition"": ""item >= 5 and item <= 12"",
          ""then"": ""child_price = base_rate_weekday * 0.5""
        },
        {
          ""type"": ""if"",
          ""condition"": ""item > 12"",
          ""then"": ""child_price = base_rate_weekday""
        }
      ]
    },
    {
      ""id"": ""flight_discount"",
      ""name"": ""Flight Discount"",
      ""description"": ""Toplu alımlarda uçak bileti indirimi"",
      ""priority"": 3,
      ""conditions"": {
        ""any"": [
          { ""var"": ""adults"", "">"": 0 }
        ]
      },
      ""actions"": [
        {
          ""type"": ""compute"",
          ""expression"": ""flight_total = (adults * flight_price_adult)""
        },
        {
          ""type"": ""if"",
          ""condition"": ""adults >= 5"",
          ""then"": ""flight_total *= 0.95""
        }
      ]
    },
    {
      ""id"": ""transfer_discount"",
      ""name"": ""Transfer Discount"",
      ""description"": ""Uçak bileti alındıysa transfer indirimi"",
      ""priority"": 4,
      ""conditions"": {
        ""all"": [
          { ""var"": ""has_flight"", ""=="": true }
        ]
      },
      ""actions"": [
        { ""type"": ""compute"", ""expression"": ""transfer_total = transfer_price * 0.5"" }
      ]
    },
    {
      ""id"": ""package_bonus"",
      ""name"": ""Package Bonus"",
      ""description"": ""Uzun süreli ve tam paket alımlarında ek indirim"",
      ""priority"": 5,
      ""conditions"": {
        ""all"": [
          { ""var"": ""stay_days"", "">="": 7 },
          { ""var"": ""has_flight"", ""=="": true },
          { ""var"": ""has_transfer"", ""=="": true }
        ]
      },
      ""actions"": [
        {
          ""type"": ""compute"",
          ""expression"": ""total_package *= 0.93""
        }
      ]
    }
  ]
}";

            // Load rules from JSON string
            var engine = JsonRuleLoader.LoadEngineFromJson(jsonRules);

            Console.WriteLine("\nLoaded {0} rules from JSON", engine.GetRules().Count());
            
            foreach (var rule in engine.GetRules())
            {
                Console.WriteLine($"  - {rule.Name} (Priority: {rule.Priority})");
            }

            // Create test context
            var bookingContext = new Dictionary<string, object>
            {
                ["stay_days"] = 7,
                ["weekdays"] = 5,
                ["weekends"] = 2,
                ["base_rate_weekday"] = 100m,
                ["base_rate_weekend"] = 150m,
                ["start_day"] = "Monday",
                ["adults"] = 3,
                ["flight_price_adult"] = 500m,
                ["has_flight"] = true,
                ["has_transfer"] = true,
                ["transfer_price"] = 50m,
                ["children"] = new[] { 3, 8, 14 },
                ["total_package"] = 3000m
            };

            Console.WriteLine("\n\nEvaluating rules against booking context:");
            Console.WriteLine("----------------------------------------");

            // Evaluate rules
            var results = engine.Evaluate(bookingContext);

            foreach (var result in results)
            {
                Console.WriteLine($"\nRule: {result.IsSuccess} (Execution: {result.ExecutionTime.TotalMilliseconds:F2}ms)");
                if (!string.IsNullOrEmpty(result.ErrorMessage))
                {
                    Console.WriteLine($"  Error: {result.ErrorMessage}");
                }
            }

            // Print final context values
            Console.WriteLine("\n\nFinal Context Values:");
            Console.WriteLine("--------------------");
            Console.WriteLine($"hotel_total: {bookingContext.GetValueOrDefault("hotel_total")}");
            Console.WriteLine($"flight_total: {bookingContext.GetValueOrDefault("flight_total")}");
            Console.WriteLine($"transfer_total: {bookingContext.GetValueOrDefault("transfer_total")}");
            Console.WriteLine($"total_package: {bookingContext.GetValueOrDefault("total_package")}");

            // Example: Loading from file
            Console.WriteLine("\n\nExample: Loading from JSON file");
            Console.WriteLine("--------------------------------");
            
            // Save JSON to a temporary file
            var tempFile = Path.GetTempFileName();
            try
            {
                File.WriteAllText(tempFile, jsonRules);
                Console.WriteLine($"Created temporary JSON file: {tempFile}");
                
                var engineFromFile = JsonRuleLoader.LoadEngineFromFile(tempFile);
                Console.WriteLine($"Loaded {engineFromFile.GetRules().Count()} rules from file");
                
                // Clean up
                File.Delete(tempFile);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error: {ex.Message}");
            }

            return Task.CompletedTask;
        }

        public static Task RunPerformanceTest()
        {
            Console.WriteLine("\n\nPerformance Test - 1000 Booking Combinations");
            Console.WriteLine("==============================================");

            var json = @"{
  ""rules"": [
    {
      ""id"": ""hotel_price"",
      ""name"": ""Hotel Price Calculation"",
      ""description"": ""Hafta içi/sonu otel fiyat farkı ve haftalık indirim"",
      ""priority"": 1,
      ""conditions"": {
        ""all"": [
          { ""var"": ""stay_days"", "">="": 1 }
        ]
      },
      ""actions"": [
        {
          ""type"": ""compute"",
          ""expression"": ""hotel_total = (weekdays * base_rate_weekday) + (weekends * base_rate_weekend)""
        },
        {
          ""type"": ""if"",
          ""condition"": ""stay_days >= 7 and start_day == 'Monday'"",
          ""then"": ""hotel_total *= 0.9""
        }
      ]
    },
    {
      ""id"": ""child_discounts"",
      ""name"": ""Child Discounts"",
      ""description"": ""Çocuk yaşına göre konaklama indirimi"",
      ""priority"": 2,
      ""foreach"": ""children"",
      ""actions"": [
        {
          ""type"": ""if"",
          ""condition"": ""item <= 4"",
          ""then"": ""child_price = 0""
        },
        {
          ""type"": ""if"",
          ""condition"": ""item >= 5 and item <= 12"",
          ""then"": ""child_price = base_rate_weekday * 0.5""
        },
        {
          ""type"": ""if"",
          ""condition"": ""item > 12"",
          ""then"": ""child_price = base_rate_weekday""
        }
      ]
    },
    {
      ""id"": ""flight_discount"",
      ""name"": ""Flight Discount"",
      ""description"": ""Toplu alımlarda uçak bileti indirimi"",
      ""priority"": 3,
      ""conditions"": {
        ""any"": [
          { ""var"": ""adults"", "">"": 0 }
        ]
      },
      ""actions"": [
        {
          ""type"": ""compute"",
          ""expression"": ""flight_total = (adults * flight_price_adult)""
        },
        {
          ""type"": ""if"",
          ""condition"": ""adults >= 5"",
          ""then"": ""flight_total *= 0.95""
        }
      ]
    },
    {
      ""id"": ""transfer_discount"",
      ""name"": ""Transfer Discount"",
      ""description"": ""Uçak bileti alındıysa transfer indirimi"",
      ""priority"": 4,
      ""conditions"": {
        ""all"": [
          { ""var"": ""has_flight"", ""=="": true }
        ]
      },
      ""actions"": [
        { ""type"": ""compute"", ""expression"": ""transfer_total = transfer_price * 0.5"" }
      ]
    },
    {
      ""id"": ""package_bonus"",
      ""name"": ""Package Bonus"",
      ""description"": ""Uzun süreli ve tam paket alımlarında ek indirim"",
      ""priority"": 5,
      ""conditions"": {
        ""all"": [
          { ""var"": ""stay_days"", "">="": 7 },
          { ""var"": ""has_flight"", ""=="": true },
          { ""var"": ""has_transfer"", ""=="": true }
        ]
      },
      ""actions"": [
        {
          ""type"": ""compute"",
          ""expression"": ""total_package *= 0.93""
        }
      ]
    }
  ]
}";

            // Load engine
            var engine = JsonRuleLoader.LoadEngineFromJson(json);
            
            Console.WriteLine($"Loaded {engine.GetRules().Count()} rules from JSON\n");

            // Track statistics
            var totalExecutionTime = 0L;
            var successCount = 0;
            var skipCount = 0;
            var failureCount = 0;
            var minTime = long.MaxValue;
            var maxTime = long.MinValue;
            
            var random = new Random(42); // Seed for reproducibility
            var daysOfWeek = new[] { "Monday", "Tuesday", "Wednesday", "Thursday", "Friday", "Saturday", "Sunday" };

            Console.WriteLine("Generating and evaluating 1000 booking combinations...");
            Console.WriteLine();

            // Progress indicator
            var progressInterval = 100;
            var startTime = DateTime.UtcNow;

            for (int i = 0; i < 1000; i++)
            {
                // Generate random booking combination
                var stayDays = random.Next(1, 30);
                var weekdays = (int)Math.Floor(stayDays * 0.7);
                var weekends = stayDays - weekdays;
                var baseRateWeekday = random.Next(50, 300);
                var baseRateWeekend = random.Next(100, 400);
                var startDay = daysOfWeek[random.Next(daysOfWeek.Length)];
                var adults = random.Next(1, 10);
                var flightPriceAdult = random.Next(200, 800);
                var hasFlight = random.Next(0, 2) == 1;
                var hasTransfer = random.Next(0, 2) == 1;
                var transferPrice = random.Next(20, 100);
                var totalPackage = random.Next(1000, 10000);
                
                // Random number of children
                var childrenCount = random.Next(0, 4);
                var children = new List<int>();
                for (int j = 0; j < childrenCount; j++)
                {
                    children.Add(random.Next(2, 16));
                }

                var bookingContext = new Dictionary<string, object>
                {
                    ["stay_days"] = stayDays,
                    ["weekdays"] = weekdays,
                    ["weekends"] = weekends,
                    ["base_rate_weekday"] = (decimal)baseRateWeekday,
                    ["base_rate_weekend"] = (decimal)baseRateWeekend,
                    ["start_day"] = startDay,
                    ["adults"] = adults,
                    ["flight_price_adult"] = (decimal)flightPriceAdult,
                    ["has_flight"] = hasFlight,
                    ["has_transfer"] = hasTransfer,
                    ["transfer_price"] = (decimal)transferPrice,
                    ["total_package"] = (decimal)totalPackage,
                    ["children"] = children.ToArray()
                };

                // Measure execution time
                var sw = System.Diagnostics.Stopwatch.StartNew();
                var results = engine.Evaluate(bookingContext);
                sw.Stop();

                // Update statistics
                var executionTime = sw.ElapsedTicks;
                totalExecutionTime += executionTime;
                
                if (executionTime < minTime) minTime = executionTime;
                if (executionTime > maxTime) maxTime = executionTime;

                var success = results.All(r => r.IsSuccess || r.Data.GetValueOrDefault("skipped")?.Equals(true) == true);
                if (results.All(r => r.Data.GetValueOrDefault("skipped")?.Equals(true) == true))
                    skipCount++;
                else if (success)
                    successCount++;
                else
                    failureCount++;

                // Show progress
                if ((i + 1) % progressInterval == 0)
                {
                    Console.WriteLine($"Progress: {i + 1} / 1000 combinations tested ({((i + 1) * 100.0 / 1000):F1}%)");
                }
            }

            var totalWallClockTime = DateTime.UtcNow - startTime;

            Console.WriteLine("\n" + new string('=', 50));
            Console.WriteLine("PERFORMANCE TEST RESULTS");
            Console.WriteLine(new string('=', 50));
            Console.WriteLine($"Total Bookings Tested: 1000");
            Console.WriteLine($"Successfully Evaluated: {successCount}");
            Console.WriteLine($"Skipped (No Matching Rules): {skipCount}");
            Console.WriteLine($"Failed: {failureCount}");
            Console.WriteLine();
            Console.WriteLine("Timing Statistics:");
            Console.WriteLine($"  Total Wall Clock Time: {totalWallClockTime.TotalMilliseconds:F2} ms");
            Console.WriteLine($"  Average per booking: {totalWallClockTime.TotalMilliseconds / 1000:F3} ms");
            Console.WriteLine($"  Min execution time: {minTime / 10000.0:F3} ms");
            Console.WriteLine($"  Max execution time: {maxTime / 10000.0:F3} ms");
            Console.WriteLine($"  Average execution time: {(totalExecutionTime / 1000.0) / 10000.0:F3} ms");
            Console.WriteLine();
            Console.WriteLine($"Bookings per second: {1000.0 / totalWallClockTime.TotalSeconds:F2}");
            Console.WriteLine(new string('=', 50));

            return Task.CompletedTask;
        }
    }
}

