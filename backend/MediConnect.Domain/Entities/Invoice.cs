using MediConnect.Domain.Common;
using MediConnect.Domain.Enums;

namespace MediConnect.Domain.Entities;

public class Invoice : TenantEntity
{
    public Guid PatientId { get; set; }
    public Guid? AppointmentId { get; set; }

    public string InvoiceNumber { get; set; } = string.Empty;
    public DateOnly IssuedDate { get; set; }
    public InvoiceStatus Status { get; set; } = InvoiceStatus.Draft;

    public decimal SubTotal { get; set; }
    public decimal DiscountAmount { get; set; }
    public decimal TaxPercent { get; set; }
    public decimal TaxAmount { get; set; }
    public decimal Total { get; set; }
    public decimal AmountPaid { get; set; }

    public string? Notes { get; set; }

    public Patient? Patient { get; set; }
    public ICollection<InvoiceItem> Items { get; set; } = new List<InvoiceItem>();
    public ICollection<Payment> Payments { get; set; } = new List<Payment>();

    public decimal Balance => Total - AmountPaid;
}

public class InvoiceItem : TenantEntity
{
    public Guid InvoiceId { get; set; }
    public string Description { get; set; } = string.Empty;
    public int Quantity { get; set; } = 1;
    public decimal UnitPrice { get; set; }
    public decimal LineTotal => Quantity * UnitPrice;

    public Invoice? Invoice { get; set; }
}

public class Payment : TenantEntity
{
    public Guid InvoiceId { get; set; }
    public decimal Amount { get; set; }
    public PaymentMode Mode { get; set; }
    public DateTime PaidAtUtc { get; set; } = DateTime.UtcNow;
    public string? ReferenceNumber { get; set; }

    public Invoice? Invoice { get; set; }
}
