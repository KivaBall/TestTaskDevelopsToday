using System.Data;
using Dapper;
using Microsoft.Data.SqlClient;
using TestTaskDevelopsToday.Entities;

namespace TestTaskDevelopsToday.Helpers;

public static class DbHelper
{
    public static async Task<bool> DatabaseExistsAsync(this IDbConnection sqlConnection)
    {
        const string query =
            """
            SELECT 1
            FROM sys.databases
            WHERE name = 'TestTaskDatabase';
            """;

        return await sqlConnection.QueryFirstOrDefaultAsync<int?>(query) != null;
    }

    public static async Task CreateDatabaseAsync(this IDbConnection sqlConnection)
    {
        const string query =
            """
            CREATE DATABASE TestTaskDatabase;
            """;

        await sqlConnection.ExecuteAsync(query);
    }

    public static async Task<bool> TableExistsAsync(this IDbConnection sqlConnection)
    {
        const string query =
            """
            SELECT IIF(EXISTS (SELECT * FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = 'Cabs'), 1, 0);
            """;

        return await sqlConnection.QueryFirstOrDefaultAsync<int>(query) == 1;
    }

    public static async Task CreateTableAsync(this IDbConnection sqlConnection)
    {
        const string query =
            """
            CREATE TABLE Cabs
            (
                tpep_pickup_datetime  DATETIME,
                tpep_dropoff_datetime DATETIME,
                passenger_count       TINYINT,
                trip_distance         DECIMAL(4, 2), 
                store_and_fwd_flag    CHAR(3),
                PULocationID          SMALLINT,
                DOLocationID          SMALLINT,
                fare_amount           DECIMAL(5, 2),
                tip_amount            DECIMAL(5, 2)
                CONSTRAINT uniqueness_three_columns UNIQUE (tpep_pickup_datetime, tpep_dropoff_datetime, passenger_count)
            );
            """;

        await sqlConnection.ExecuteAsync(query);
    }

    public static async Task<bool> RowsExistAsync(this IDbConnection sqlConnection)
    {
        const string query =
            """
            SELECT IIF(EXISTS (SELECT * FROM Cabs), 1, 0);
            """;

        return await sqlConnection.QueryFirstOrDefaultAsync<int>(query) == 1;
    }

    public static async Task InsertRowsAsync(this IDbConnection sqlConnection,
        IEnumerable<CabCsv> cabs)
    {
        var dataTable = new DataTable();

        dataTable.Columns.Add("tpep_pickup_datetime", typeof(DateTime));
        dataTable.Columns.Add("tpep_dropoff_datetime", typeof(DateTime));
        var passengerCountColumn = new DataColumn("passenger_count", typeof(byte))
        {
            AllowDBNull = true
        };
        dataTable.Columns.Add(passengerCountColumn);
        dataTable.Columns.Add("trip_distance", typeof(decimal));
        var storeAndFwdFlagColumn = new DataColumn("store_and_fwd_flag", typeof(string))
        {
            AllowDBNull = true
        };
        dataTable.Columns.Add(storeAndFwdFlagColumn);
        dataTable.Columns.Add("PULocationID", typeof(ushort));
        dataTable.Columns.Add("DOLocationID", typeof(ushort));
        dataTable.Columns.Add("fare_amount", typeof(decimal));
        dataTable.Columns.Add("tip_amount", typeof(decimal));

        foreach (var cab in cabs)
        {
            dataTable.Rows.Add(
                cab.TpepPickupDatetime,
                cab.TpepDropoffDatetime,
                cab.PassengerCount,
                cab.TripDistance,
                cab.StoreAndFwdFlag switch
                {
                    "Y" => "Yes",
                    "N" => "No",
                    _ => null
                },
                cab.PuLocationId,
                cab.DoLocationId,
                cab.FareAmount,
                cab.TipAmount);
        }

        using var bulkCopy = new SqlBulkCopy((SqlConnection)sqlConnection);
        bulkCopy.DestinationTableName = "Cabs";

        await bulkCopy.WriteToServerAsync(dataTable);
    }

    public static async Task<byte> HighestTipAmountOnAverageByPuLocationAsync(
        this IDbConnection sqlConnection)
    {
        const string query =
            """
            SELECT TOP 1 PULocationID
            FROM Cabs
            GROUP BY PULocationID
            ORDER BY AVG(TIP_amount) DESC;
            """;

        return await sqlConnection.QueryFirstOrDefaultAsync<byte>(query);
    }

    public static async Task<IEnumerable<CabCsv>> GetTop100LongestFaresByDistanceAsync(
        this IDbConnection sqlConnection)
    {
        const string query =
            """
            SELECT TOP 100 *
            FROM Cabs
            ORDER BY trip_distance DESC;
            """;

        return await sqlConnection.QueryAsync<CabCsv>(query);
    }

    public static async Task<IEnumerable<CabCsv>> GetTop100LongestFaresByTravelTimeAsync(
        this IDbConnection sqlConnection)
    {
        const string query =
            """
            SELECT TOP 100 *
            FROM Cabs
            ORDER BY DATEDIFF(SECOND, tpep_pickup_datetime, tpep_dropoff_datetime) DESC;
            """;


        return await sqlConnection.QueryAsync<CabCsv>(query);
    }

    public static async Task<IEnumerable<CabCsv>> GetByPuLocationIdAsync(
        this IDbConnection sqlConnection,
        ushort puLocationId)
    {
        const string query =
            """
            SELECT *
            FROM Cabs
            WHERE PULocationID = @Id;
            """;

        return await sqlConnection.QueryAsync<CabCsv>(query, new { Id = (int)puLocationId });
    }
}
