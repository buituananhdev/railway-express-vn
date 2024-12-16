using Common.Application.Repositories;

namespace Admin.Application.Repositories
{
    public interface IAdminUnitOfWork : IUnitOfWork
    {
        ITrainRepository TrainRepository { get; }
        ITrainCarRepository TrainCarRepository { get; }
        ITrainStatusRepository TrainStatusRepository { get; }
        ITrainScheduleRepository TrainScheduleRepository { get; }
        IStationRepository StationRepository { get; }
        ISeatRepository SeatRepository { get; }
    }
}
