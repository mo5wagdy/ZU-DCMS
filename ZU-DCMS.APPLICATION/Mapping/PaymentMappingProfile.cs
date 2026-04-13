using AutoMapper;
using ZU_DCMS.APPLICATION.DTOs.Payment;
using ZU_DCMS.Domain.Entities;

namespace ZU_DCMS.APPLICATION.Mapping
{
    // __ AutoMapper profile for mapping between Payment and PaymentDto __ //
    public class PaymentMappingProfile : Profile
    {
        public PaymentMappingProfile()
        {
            // => Payment -> PaymentDto 
            CreateMap<Payment, PaymentDto>();
        }
    }
}
