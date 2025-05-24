using Admin.Application.Services;
using AutoMapper;
using Common.Protos;
using Grpc.Core;

namespace Admin.Infrastructure.GrpcServices;
public class AdminService : Common.Protos.AdminGrpcService.AdminGrpcServiceBase
{
    private readonly IMapper _mapper;
    private readonly ISeatService _seatService;
    public AdminService(IMapper mapper, ISeatService seatService)
    {
        _mapper = mapper;
        _seatService = seatService;
    }
    public override async Task<GetSeatInformationResponse> GetSeatInformation(
        GetSeatInformationRequest request,
        ServerCallContext context)
    {
        var seatId = Guid.Parse(request.SeatId);
        var results = await _seatService.GetSeatWithTrainInformationAsync(seatId);

        return _mapper.Map<GetSeatInformationResponse>(results);
    }
}
