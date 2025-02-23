using AutoMapper;
using Booking.Application.Dtos;
using Booking.Application.Repositories;
using Booking.Domain.Entities;
using Booking.Domain.Specifications;

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

    public async Task<AddPassengerDetailsDto> AddPassengerDetailsAsync(AddPassengerDetailsDto addPassengerDetailDto)
    {
        try
        {
            var passengerInfos = _mapper.Map<List<PassengerInfo>>(addPassengerDetailDto.PassengerInfos);
            passengerInfos.ForEach(p => p.TicketId = addPassengerDetailDto.TicketId);
            await _bookingUnitOfWork.PassengerInfoRepository.AddRangeAsync(passengerInfos);
            await _bookingUnitOfWork.SaveChangesAsync();

            return addPassengerDetailDto;
        }
        catch (Exception ex)
        {
            throw new Exception(ex.Message);
        }
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

    public async Task<List<PassengerInfoDto>> GetPassengerDetailsByTicketIdAsync(Guid ticketId)
    {
        try
        {
            var specifiation = new PassengerInfoTicketIdSpecification(ticketId);
            var passengers = await _bookingUnitOfWork.PassengerInfoRepository
                .ToListAsync(spec: specifiation);

            return _mapper.Map<List<PassengerInfoDto>>(passengers);
        }
        catch (Exception ex)
        {
            throw;
        }
    }
}
