using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace tef_banpro_app.Model
{
    [Table("DCL_TRANSACTION")]
    public class DclTransaction
    {
        [Key]
        [Column(Order = 1)]
        [StringLength(10)]
        [Display(Name = "Comercio ID")]
        public string MerchantId { get; set; }

        [Key]
        [Column(Order = 2)]
        [StringLength(8)]
        [Display(Name = "Terminal ID")]
        public string TerminalId { get; set; }

        [Key]
        [Column(Order = 3)]
        [StringLength(20)]
        [Display(Name = "Tipo")]
        public string TransactionType { get; set; }

        [StringLength(20)]
        [Display(Name = "Ref. ICG")]
        public string ExternalReference { get; set; }

        [Key]
        [Column(Order = 4)]
        [StringLength(6)]
        [Display(Name = "Boleta")]
        public string Invoice { get; set; }

        [StringLength(5)]
        [Display(Name = "Moneda")]
        public string CurrencySymbol { get; set; }

        [StringLength(8)]
        [Display(Name = "Fecha")]
        public string TransactionDate { get; set; }

        [StringLength(6)]
        [Display(Name = "Hora")]
        public string TransactionTime { get; set; }

        [StringLength(12)]
        [Display(Name = "Referencia")]
        public string ReferenceNumber { get; set; }

        [StringLength(6)]
        [Display(Name = "Autorizado")]
        public string AuthorizationNumber { get; set; }

        [StringLength(6)]
        [Display(Name = "STAN")]
        public string SystemTraceNumber { get; set; }

        [StringLength(20)]
        [Display(Name = "Total")]
        public string Amount { get; set; }

        [StringLength(20)]
        [Display(Name = "Impuestos")]
        public string TaxAmount { get; set; }

        [StringLength(30)]
        [Display(Name = "Tarejeta N°")]
        public string MaskedPan { get; set; }

        [StringLength(50)]
        [Display(Name = "Tarjetahabiente")]
        public string CardHolderName { get; set; }

        [Display(Name = "Cerrado")]
        public bool Closed { get; set; } = false;
    }
}
