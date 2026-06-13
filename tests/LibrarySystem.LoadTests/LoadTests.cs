using System;
using System.Linq;
using System.Net.Http;
using NBomber.Contracts;
using NBomber.CSharp;
using Shouldly;
using Xunit;

namespace LibrarySystem.LoadTests;

public class LoadTests
{
    private const string BaseUrl = "http://localhost:5133";

    [Fact]
    public void L1_ReadScenarios_P99LatencyMustNotExceed300ms()
    {
        using var httpClient = new HttpClient { BaseAddress = new Uri(BaseUrl) };

        var listAllBooks = Scenario.Create("list_all_books", async context =>
        {
            var response = await httpClient.GetAsync("/api/books");
            return response.IsSuccessStatusCode
                ? Response.Ok()
                : Response.Fail(statusCode: ((int)response.StatusCode).ToString());
        })
        .WithLoadSimulations(
            Simulation.Inject(rate: 100, interval: TimeSpan.FromSeconds(1), during: TimeSpan.FromSeconds(30))
        );

        var random = new Random();
        var getSingleBook = Scenario.Create("get_single_book", async context =>
        {
            var id = random.Next(1, 51);
            var response = await httpClient.GetAsync($"/api/books/{id}");

            return response.StatusCode == System.Net.HttpStatusCode.InternalServerError
                ? Response.Fail()
                : Response.Ok();
        })
        .WithLoadSimulations(
            Simulation.Inject(rate: 50, interval: TimeSpan.FromSeconds(1), during: TimeSpan.FromSeconds(30))
        );

        var stats = NBomberRunner
            .RegisterScenarios(listAllBooks, getSingleBook)
            .WithReportFileName("L1_ReadScenarios")
            .WithReportFolder("nbomber-reports")
            .WithTestSuite("LibrarySystem.LoadTests")
            .WithTestName("L1_ReadScenarios")
            .Run();

        var listStats = stats.ScenarioStats.First(s => s.ScenarioName == "list_all_books");
        var singleStats = stats.ScenarioStats.First(s => s.ScenarioName == "get_single_book");

        listStats.Ok.Latency.MaxMs.ShouldBeLessThanOrEqualTo(300,
            "Max Latency for listing all books exceeded 300ms under load (SLA Violated)!");

        singleStats.Ok.Latency.MaxMs.ShouldBeLessThanOrEqualTo(300,
            "Max Latency for fetching a single book exceeded 300ms under load (SLA Violated)!");
    }

    [Fact]
    public void L2_BorrowScenario_No500ErrorsAllowed()
    {
        using var httpClient = new HttpClient { BaseAddress = new Uri(BaseUrl) };
        var random = new Random();

        var borrowBook = Scenario.Create("borrow_book", async context =>
        {
            var memberId = random.Next(1, 201);
            var bookId = random.Next(1, 51);

            var payload = new StringContent(
                $"{{\"memberId\":{memberId},\"bookId\":{bookId}}}",
                System.Text.Encoding.UTF8,
                "application/json"
            );

            var response = await httpClient.PostAsync("/api/loans", payload);

            return response.StatusCode switch
            {
                System.Net.HttpStatusCode.Created => Response.Ok(),
                System.Net.HttpStatusCode.UnprocessableEntity => Response.Ok(statusCode: "422_rule_violation"),
                System.Net.HttpStatusCode.NotFound => Response.Ok(statusCode: "404_not_found"),
                System.Net.HttpStatusCode.Conflict => Response.Ok(statusCode: "409_conflict"),
                System.Net.HttpStatusCode.InternalServerError => Response.Fail(
                    statusCode: "500",
                    message: await response.Content.ReadAsStringAsync()),
                _ => Response.Fail(
                    statusCode: ((int)response.StatusCode).ToString(),
                    message: await response.Content.ReadAsStringAsync())
            };
        })
        .WithLoadSimulations(
            Simulation.Inject(rate: 20, interval: TimeSpan.FromSeconds(1), during: TimeSpan.FromSeconds(60))
        );

        var stats = NBomberRunner
            .RegisterScenarios(borrowBook)
            .WithReportFileName("L2_BorrowScenario")
            .WithReportFolder("nbomber-reports")
            .WithTestSuite("LibrarySystem.LoadTests")
            .WithTestName("L2_BorrowScenario")
            .Run();

        var borrowStats = stats.ScenarioStats.First(s => s.ScenarioName == "borrow_book");

        borrowStats.Fail.Request.Count.ShouldBe(0,
            "500 error rate must be 0% — any 500 means the server crashed under concurrency");
    }
}