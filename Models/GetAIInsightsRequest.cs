using System;

namespace TropicalBudget.Models;

public class GetAIInsightsRequest
{
    public Guid budgetID { get; set; }

    public int? year { get; set; }
    public int? month { get; set; }
}
