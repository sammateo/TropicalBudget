using System.Text.Json;
using Google.GenAI;
using Google.GenAI.Types;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using TropicalBudget.Models;
using TropicalBudget.Services;
using TropicalBudget.Utilities;

namespace TropicalBudget.Controllers
{
    public class InsightsController : Controller
    {
        private readonly DatabaseService _db;
        private readonly GeminiSettings _geminiSettings;
        public InsightsController(DatabaseService db, IOptions<GeminiSettings> geminiSettings)
        {
            _db = db;
            _geminiSettings = geminiSettings.Value;
        }
        public async Task<IActionResult> Index(Guid budgetID, int? year, int? month)
        {
            if (budgetID == Guid.Empty)
                return RedirectToAction("Index", "Home");
            Tuple<Guid, List<Transaction>> budgetTransactions = new(new(), new());
            try
            {
                string userID = UserUtility.GetUserID(User);
                DateTime currentDate = DateTime.Now;
                string currentMonth = string.Empty;
                DateTime startDate;
                DateTime endDate;
                if (year == null || month == null)
                {
                    currentMonth = $"{currentDate.ToString("MMMM")}, {currentDate.ToString("yyyy")}";
                    //get start and end date of the month
                    startDate = new DateTime(currentDate.Year, currentDate.Month, 1, 0, 0, 0);
                    endDate = startDate.AddMonths(1).AddSeconds(-1);
                }
                else
                {
                    if (month.Value > 12 || month.Value < 1)
                    {
                        return RedirectToAction("Index");
                    }
                    startDate = new DateTime(year.Value, month.Value, 1, 0, 0, 0);
                    endDate = startDate.AddMonths(1).AddSeconds(-1);
                    currentMonth = $"{startDate.ToString("MMMM")}, {startDate.ToString("yyyy")}";
                }
                TempData["currentMonthString"] = currentMonth;
                TempData["startDate"] = startDate;
                Budget budget = await _db.GetBudget(userID, budgetID);
                TempData["BudgetName"] = budget != null && !string.IsNullOrWhiteSpace(budget.Name) ? budget.Name : "Unknown";
                List<Transaction> transactions = await _db.GetTransactions(budgetID, startDate, endDate);
                // if(transactions.Count > 0)
                // {
                //     //generate ai insights
                //     Console.WriteLine("Generating insights");
                //     string insights = await generateAIInsightsWithGemini(transactions);
                //     TempData["AIInsights"] = insights;
                // }
                budgetTransactions = new(budgetID, transactions);
            }
            catch (Exception ex)
            {
                SentrySdk.CaptureException(ex);
            }
            return View("ViewInsights", budgetTransactions);
        }

        public async Task<IActionResult> AI(Guid budgetID, int? year, int? month)
        {
            if (budgetID == Guid.Empty)
                return RedirectToAction("Index", "Home");
            Tuple<Guid, List<Transaction>> budgetTransactions = new(new(), new());
            try
            {
                string userID = UserUtility.GetUserID(User);
                DateTime currentDate = DateTime.Now;
                string currentMonth = string.Empty;
                DateTime startDate;
                DateTime endDate;
                if (year == null || month == null)
                {
                    currentMonth = $"{currentDate.ToString("MMMM")}, {currentDate.ToString("yyyy")}";
                    //get start and end date of the month
                    startDate = new DateTime(currentDate.Year, currentDate.Month, 1, 0, 0, 0);
                    endDate = startDate.AddMonths(1).AddSeconds(-1);
                }
                else
                {
                    if (month.Value > 12 || month.Value < 1)
                    {
                        return RedirectToAction("Index");
                    }
                    startDate = new DateTime(year.Value, month.Value, 1, 0, 0, 0);
                    endDate = startDate.AddMonths(1).AddSeconds(-1);
                    currentMonth = $"{startDate.ToString("MMMM")}, {startDate.ToString("yyyy")}";
                }
                TempData["currentMonthString"] = currentMonth;
                TempData["startDate"] = startDate;
                Budget budget = await _db.GetBudget(userID, budgetID);
                TempData["BudgetName"] = budget != null && !string.IsNullOrWhiteSpace(budget.Name) ? budget.Name : "Unknown";
                List<Transaction> transactions = await _db.GetTransactions(budgetID, startDate, endDate);

                budgetTransactions = new(budgetID, transactions);
            }
            catch (Exception ex)
            {
                SentrySdk.CaptureException(ex);
            }
            return View("ViewAIInsights", budgetTransactions);
        }

        [HttpPost]
        public async Task<IActionResult> GetAIInsights([FromBody] GetAIInsightsRequest getAIInsightsRequest)
        {
            Guid budgetID = getAIInsightsRequest.budgetID;
            int? year = getAIInsightsRequest.year;
            int? month = getAIInsightsRequest.month;
            if (budgetID == Guid.Empty)
                return RedirectToAction("Index", "Home");

            string insights = "";
            try
            {
                string userID = UserUtility.GetUserID(User);
                DateTime currentDate = DateTime.Now;
                DateTime startDate;
                DateTime endDate;
                if (year == null || month == null)
                {
                    //get start and end date of the month
                    startDate = new DateTime(currentDate.Year, currentDate.Month, 1, 0, 0, 0);
                    endDate = startDate.AddMonths(1).AddSeconds(-1);
                }
                else
                {
                    if (month.Value > 12 || month.Value < 1)
                    {
                        return RedirectToAction("Index");
                    }
                    startDate = new DateTime(year.Value, month.Value, 1, 0, 0, 0);
                    endDate = startDate.AddMonths(1).AddSeconds(-1);
                }
                List<Transaction> transactions = await _db.GetTransactions(budgetID, startDate, endDate);
                List<PlanItem> planItems = await _db.GetPlanItems(budgetID);

                AIInsight aIInsight = await _db.GetAIInsight(budgetID, startDate.Month, startDate.Year);

                if (transactions.Count > 0 && (aIInsight == null || aIInsight.Content == null || aIInsight.Content.Trim() == ""))
                {
                    //generate ai insights
                    insights = await generateAndSaveAIInsights(transactions, planItems, budgetID, startDate);
                }
                else if (aIInsight != null && aIInsight.Content != null && aIInsight.Content.Trim() != "")
                {
                    //if insights exist but for a previous date - rerun
                    DateOnly today = DateOnly.FromDateTime(currentDate);
                    DateOnly insightsCreated = DateOnly.FromDateTime(aIInsight.CreatedAt);

                    if (insightsCreated < today)
                    {

                        insights = await generateAndSaveAIInsights(transactions, planItems, budgetID, startDate);
                    }
                    else
                    {
                        // do not regenerate if alreadt run for the day
                        insights = aIInsight.Content;
                    }
                }

            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                SentrySdk.CaptureException(ex);
                return Problem();
            }
            return Ok(new { message = insights });
        }

        private async Task<string> generateAndSaveAIInsights(List<Transaction> transactions, List<PlanItem> planItems, Guid budgetID, DateTime startDate)
        {
            string insights = await generateAIInsightsWithGemini(transactions, planItems);
            AIInsight newInsights = new();
            newInsights.BudgetID = budgetID;
            newInsights.Month = startDate.Month;
            newInsights.Year = startDate.Year;
            newInsights.Content = insights;
            await _db.InsertAIInsight(newInsights);
            return insights;
        }

        private async Task<string> generateAIInsightsWithGemini(List<Transaction> transactions, List<PlanItem> planItems)
        {
            var client = new Client(apiKey: _geminiSettings.API_KEY);

            //response schema
            Schema aiInsightsInfo = new()
            {
                Type = Google.GenAI.Types.Type.Array,
                Items = new Schema
                {
                    Properties = new Dictionary<string, Schema>
                {
                    {
                        "title", new Schema {Type = Google.GenAI.Types.Type.String, Title = "Name" }
                    },
                    {
                        "details", new Schema {Type = Google.GenAI.Types.Type.String, Title = "Details" }
                    },
                    {
                        "type", new Schema {Type = Google.GenAI.Types.Type.String, Title = "Type" }
                    },
                },
                    PropertyOrdering = ["title", "details", "type"],
                    Required = ["title", "details", "type"],
                    Title = "AIInsight",
                    Type = Google.GenAI.Types.Type.Object
                }
            };


            //generation config
            GenerateContentConfig generateContentConfig = new()
            {
                ResponseJsonSchema = aiInsightsInfo,
                ResponseMimeType = "application/json",
                SystemInstruction = new Content
                {
                    Parts = [
                        new Part {
                            Text = "You are a budgeting app, evaluate the following transactions and provide insights into the spending of the user and recommendations.",
                        },
                        new Part{
                            Text = "The budgeting app allows users to budget monthly, the transactions you are given are for a given month."
                        },
                        new Part{
                            Text = "Do not give me detailed transactions or any breakdowns."
                        },
                        new Part{
                            Text = "When giving the recommendations and insights, you can use the transaction data to provide numbers for the various categories."
                        },
                        new Part{
                            Text = "The user may also have set target amounts for specific categories for the month which will be outlined in their plan."
                        },
                        new Part{
                            Text = "You can use their plan to determine if the user is hitting their goals, overspending, etc."
                        },
                        new Part{
                            Text = "You can also provide recommendations and insights using both the transaction and plan data."
                        },
                        new Part{
                            Text = "The response type should be a string that is either 'insight' or 'recommendation'."
                        },
                        new Part{
                            Text = "Ensure that generated recommendations and insights details that are returned are no more than 2/3 sentences."
                        },
                        new Part{
                            Text = "The details field can be returned as html."
                        },
                    ]
                }

            };

            var response = await client.Models.GenerateContentAsync(
                model: "gemini-2.5-flash",
                config: generateContentConfig,
                contents:
                $"""
                Transactions:
                {JsonSerializer.Serialize(transactions)}
 
                Plan:
                {JsonSerializer.Serialize(planItems.Select(x => new { x.Amount, x.CategoryName, x.CategoryID, x.CategoryColor, x.BudgetID }))}
                """
            );
            return response.Candidates?[0].Content?.Parts?[0].Text ?? "";
        }
    }
}
