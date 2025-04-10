using System.Globalization;

namespace TestTaskDevelopsToday.Entities;

public sealed class CabCsv
{
    public DateTime TpepPickupDatetime { get; init; }
    public DateTime TpepDropoffDatetime { get; init; }
    public byte? PassengerCount { get; init; }
    public decimal TripDistance { get; init; }
    public string? StoreAndFwdFlag { get; init; }
    public ushort PuLocationId { get; init; }
    public ushort DoLocationId { get; init; }
    public decimal FareAmount { get; init; }
    public decimal TipAmount { get; init; }

    public override string ToString()
    {
        return
            $"TpepPickupDatetime: {TpepPickupDatetime}, " +
            $"TpepDropoffDatetime: {TpepDropoffDatetime}, " +
            $"PassengerCount: {PassengerCount?.ToString(CultureInfo.InvariantCulture) ?? "|empty|"}, " +
            $"TripDistance: {TripDistance}, " +
            $"StoreAndFwdFlag: {StoreAndFwdFlag?.ToString(CultureInfo.InvariantCulture) ?? "|empty|"}, " +
            $"PuLocationId: {PuLocationId}, " +
            $"DoLocationId: {DoLocationId}, " +
            $"FareAmount: {FareAmount}, " +
            $"TipAmount: {TipAmount}";
    }
}
