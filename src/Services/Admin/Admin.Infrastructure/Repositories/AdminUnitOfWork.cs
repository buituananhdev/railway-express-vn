using Admin.Application.Repositories;
using Common.Infrastructure;
using Common.Infrastructure.Repositories;

namespace Admin.Infrastructure.Repositories;
public class AdminUnitOfWork : UnitOfWork, IAdminUnitOfWork
{
    public ITrainRepository TrainRepository { get; private set; }
    public ITrainCarRepository TrainCarRepository { get; private set; }
    public ITrainStatusRepository TrainStatusRepository { get; private set; }
    public ITrainScheduleRepository TrainScheduleRepository { get; private set; }
    public IStationRepository StationRepository { get; private set; }
    public ISeatRepository SeatRepository { get; private set; }

    public AdminUnitOfWork(
        IDataContext context,
        ITrainRepository trainRepository,
        ITrainCarRepository trainCarRepository,
        ITrainStatusRepository trainStatusRepository,
        ITrainScheduleRepository trainScheduleRepository,
        IStationRepository stationRepository,
        ISeatRepository seatRepository) 
        : base(context)
    {
        TrainRepository = trainRepository;
        TrainCarRepository = trainCarRepository;
        TrainStatusRepository = trainStatusRepository;
        TrainScheduleRepository = trainScheduleRepository;
        StationRepository = stationRepository;
        SeatRepository = seatRepository;
    }
}
