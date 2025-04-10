using CsvHelper.Configuration;
using TestTaskDevelopsToday.Entities;

namespace TestTaskDevelopsToday.Mapping;

public sealed class CabCsvMap : ClassMap<CabCsv>
{
    public CabCsvMap()
    {
        Map(m => m.TpepPickupDatetime).Name("tpep_pickup_datetime");
        Map(m => m.TpepDropoffDatetime).Name("tpep_dropoff_datetime");
        Map(m => m.PassengerCount).Name("passenger_count").Default(default(byte?));
        Map(m => m.TripDistance).Name("trip_distance");
        Map(m => m.StoreAndFwdFlag).Name("store_and_fwd_flag").Default(default(char?)!);
        Map(m => m.PuLocationId).Name("PULocationID");
        Map(m => m.DoLocationId).Name("DOLocationID");
        Map(m => m.FareAmount).Name("fare_amount");
        Map(m => m.TipAmount).Name("tip_amount");
    }
}
