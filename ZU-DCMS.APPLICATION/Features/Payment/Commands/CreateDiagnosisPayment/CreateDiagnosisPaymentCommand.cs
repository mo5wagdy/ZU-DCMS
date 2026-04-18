namespace ZU_DCMS.APPLICATION.Features.Payment.Commands.CreateDiagnosisPayment
{
    public class CreateDiagnosisPaymentCommand
    {
        public int PatientId { get; set; }
        public int BookingId { get; set; }
        public decimal Amount { get; set; }
    }
}
