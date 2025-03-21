using AutoMapper;
using Payment.Application.Dtos;
using Payment.Domain.Entities;

namespace Payment.Application.AutoMapper;
public class AutoMapperProfile : Profile
{
    public AutoMapperProfile()
    {
        CreateMap<PaymentRecord, PaymentRecordDto>().ReverseMap();
        CreateMap<AddPaymentRecordDto, PaymentRecord>().ReverseMap();
    }
}
