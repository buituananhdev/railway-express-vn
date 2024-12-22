using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Admin.Domain.Enums
{
    public enum Track
    {
        Track1 = 1,
        Track2 = 2,
    }

    public enum SeatType
    {
        Standard,
        Business
    }

    public enum TrainStatus
    {
        InTransit,
        AtStation,
        Delayed,
        AwaitingDeparture,
        Reversing,
        Cancelled,
        Faulty
    }
}