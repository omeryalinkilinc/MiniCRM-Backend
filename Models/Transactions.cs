namespace MiniCRM.Api.Models
{
    public class Transaction
    {
        public int Id { get; set; }

        public int CustomerId { get; set; }
        public Customer? Customer { get; set; } // ✅ opsiyonel hale getir

        public string TransactionType { get; set; } // Ödeme, Destek Talebi, Başvuru
        public string Description { get; set; }
        public DateTime Date { get; set; }

        public string Status { get; set; } // Beklemede, Tamamlandı, İptal Edildi

        public decimal? Amount { get; set; } // Opsiyonel hale getirildi
    }
}
