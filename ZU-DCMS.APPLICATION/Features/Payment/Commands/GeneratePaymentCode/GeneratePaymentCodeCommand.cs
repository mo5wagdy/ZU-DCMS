namespace ZU_DCMS.APPLICATION.Features.Payment.Commands.GeneratePaymentCode
{
    public class GeneratePaymentCodeCommand
    {
        public Domain.Entities.Payment Payment { get; set; } = null!;
    }
}
