using AutoMapper;
using Booking.Application.Dtos;
using Booking.Application.Repositories;
using Booking.Domain.Entities;

namespace Booking.Application.Services;
public class PassengerInfoService : IPassengerInfoService
{
    private readonly IBookingUnitOfWork _bookingUnitOfWork;
    private readonly IMapper _mapper;

    public PassengerInfoService(IBookingUnitOfWork bookingUnitOfWork, IMapper mapper)
    {
        _bookingUnitOfWork = bookingUnitOfWork;
        _mapper = mapper;
    }

    public async Task<PassengerInfoDto> AddPassengerInfoAsync(AddPassengerInfoDto passengerInfoDto)
    {
        try
        {
            var passengerInfo = _mapper.Map<PassengerInfo>(passengerInfoDto);
            await _bookingUnitOfWork.PassengerInfoRepository.AddAsync(passengerInfo);
            await _bookingUnitOfWork.SaveChangesAsync();

            return _mapper.Map<PassengerInfoDto>(passengerInfo);
        }
        catch (Exception ex)
        {
            throw new Exception(ex.Message);
        }
    }

    public async Task<List<PassengerInfoDto>> AddPassengerInforsAsync(List<AddPassengerInfoDto> passengerInfoDtos)
    {
        try
        {
            var passengerInfos = _mapper.Map<List<PassengerInfo>>(passengerInfoDtos);
            await _bookingUnitOfWork.PassengerInfoRepository.AddRangeAsync(passengerInfos);
            await _bookingUnitOfWork.SaveChangesAsync();

            return _mapper.Map<List<PassengerInfoDto>>(passengerInfos);
        }
        catch (Exception ex)
        {
            throw new Exception(ex.Message);
        }
    }
}
