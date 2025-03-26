using AutoMapper;
using Payment.Application.Dtos;
using Payment.Domain.Entities;

namespace Payment.Application.AutoMapper;
public class AutoMapperProfile : Profile
{
    public AutoMapperProfile()
    {
        CreateMap<PaymentRecordDto, PaymentRecord>().ReverseMap();
        CreateMap<AddPaymentRecordDto, PaymentRecord>().ReverseMap();
        CreateMap<UpdatePaymentRecordDto, PaymentRecord>().ReverseMap();
    }
}
