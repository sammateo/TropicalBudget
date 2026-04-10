using System;
using System.ComponentModel.DataAnnotations.Schema;

namespace TropicalBudget.Models;

public class AIInsight
{
    [Column("Id")]
    public Guid ID { get; set; }

    public Guid BudgetID { get; set; }

    public int Month { get; set; }
    public int Year { get; set; }

    public string Content { get; set; }

    public DateTime CreatedAt { get; set; }

}
