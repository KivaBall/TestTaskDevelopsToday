using System.Globalization;
using CsvHelper;
using Microsoft.Data.SqlClient;
using TestTaskDevelopsToday.Entities;
using TestTaskDevelopsToday.Helpers;
using TestTaskDevelopsToday.Mapping;
using TestTaskDevelopsToday.Resources;

Dapper.DefaultTypeMap.MatchNamesWithUnderscores = true;

var rm = Localisation.ResourceManager;

// 1: Check Database exists

await using (var sqlConnection = new SqlConnection(rm.GetString("server_sql_connection"))) // bad practice & code smell
{
    await sqlConnection.OpenAsync();

    if (!await sqlConnection.DatabaseExistsAsync())
    {
        await sqlConnection.CreateDatabaseAsync();
    }
}

// 2: Check Table exists

await using (var sqlConnection = new SqlConnection(rm.GetString("db_sql_connection"))) // bad practice & code smell
{
    await sqlConnection.OpenAsync();

    if (!await sqlConnection.TableExistsAsync())
    {
        await sqlConnection.CreateTableAsync();
    }

    // 3: Check table has data

    if (!await sqlConnection.RowsExistAsync())
    {
        using var streamReader = new StreamReader("input_data.csv");
        using var csvReader = new CsvReader(streamReader, CultureInfo.InvariantCulture);

        csvReader.Context.RegisterClassMap<CabCsvMap>();
        var cabs = csvReader.GetRecords<CabCsv>().ToList();

        var forDb = new List<CabCsv>();
        var forFile = new List<CabCsv>();

        var temp = new HashSet<(DateTime, DateTime, byte?)>();

        foreach (var cab in cabs)
        {
            if (!temp.Add((cab.TpepPickupDatetime, cab.TpepDropoffDatetime, cab.PassengerCount)))
            {
                forFile.Add(cab);
            }
            else
            {
                forDb.Add(cab);
            }
        }

        await using var streamWriter = new StreamWriter("duplicates.csv");
        await using var csvWriter = new CsvWriter(streamWriter, CultureInfo.InvariantCulture);

        await csvWriter.WriteRecordsAsync(forFile);
        await sqlConnection.InsertRowsAsync(forDb);
    }

    // 4: User's requests

    while (true)
    {
        Console.WriteLine(rm.GetString("1_button"));
        Console.WriteLine(rm.GetString("2_button"));
        Console.WriteLine(rm.GetString("3_button"));
        Console.WriteLine(rm.GetString("4_button"));
        Console.WriteLine(rm.GetString("5_button"));

        Console.Write(rm.GetString("pre_selection_text"));
        var selection = Console.ReadLine();

        switch (selection)
        {
            case "1":
                var value = await sqlConnection.HighestTipAmountOnAverageByPuLocationAsync();
                
                Console.WriteLine(rm.GetString("1_button_text")!, value);
                
                break;
            case "2":
                Console.WriteLine(rm.GetString("2_button_text"));
                
                var cabsByDistance = await sqlConnection.GetTop100LongestFaresByDistanceAsync();
                
                foreach (var cab in cabsByDistance)
                {
                    Console.WriteLine(cab);
                }
                
                break;
            case "3":
                Console.WriteLine(rm.GetString("3_button_text"));
                
                var cabsByTravel = await sqlConnection.GetTop100LongestFaresByTravelTimeAsync();
                
                foreach (var cab in cabsByTravel)
                {
                    Console.WriteLine(cab);
                }
                
                break;
            case "4":
                Console.Write(rm.GetString("4_button_selection_text"));
                
                var strId = Console.ReadLine();
                if (!ushort.TryParse(strId, CultureInfo.InvariantCulture, out var id))
                {
                    Console.WriteLine(rm.GetString("4_button_invalid_id"));
                    break;
                }
                
                Console.WriteLine(rm.GetString("4_button_text"));
                
                var cabs = await sqlConnection.GetByPuLocationIdAsync(id);
                
                foreach (var cab in cabs)
                {
                    Console.WriteLine(cab);
                }
                
                break;
            case "5":
                return;
            default:
                Console.WriteLine(rm.GetString("pre_selection_invalid_text"));
                break;
        }

        Console.WriteLine();
    }
}
