
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TestMVC.Models
{
    public class Transaction
    {
        [Column("Id")]
        public Guid ID { get; set; }
        [Column("amount")]
        public decimal Amount { get; set; }
        public string Note { get; set; }

        [Column("category_id")]
        public Guid CategoryID  { get; set; }

        [Column("source_id")]
        public Guid SourceID { get; set; }

        public Guid TransactionTypeID { get; set; }
        [DataType(DataType.Date, ErrorMessage = "Date only")]
        [DisplayFormat(DataFormatString = "{0:yyyy-MM-dd}", ApplyFormatInEditMode = true)]
        public DateTime TransactionDate { get; set; }

        public string CategoryName { get; set; }
        public string SourceName { get; set; }

        public string TransactionType { get; set; }

    }
}
