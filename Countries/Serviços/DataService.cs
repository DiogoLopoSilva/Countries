namespace Countries.Servicos
{
    using Countries.Modelos;
    using System;
    using System.Collections.Generic;
    using System.Data.SQLite;
    using System.IO;
    using System.Net;
    using System.Threading.Tasks;
    using System.Windows;

    public class DataService
    {
        private readonly SQLiteConnection connection;
        private SQLiteCommand command;
        private readonly DirectoryInfo Location;

        public DataService()
        {
            Location = new DirectoryInfo(Directory.GetCurrentDirectory()).Parent.Parent;

            try
            {
                if (!Directory.Exists(Location.FullName + "\\Images\\"))
                {
                    Directory.CreateDirectory(Location.FullName + "\\Images\\");
                }

                if (!Directory.Exists(Location.FullName + "\\Images\\Thumbnails\\"))
                {
                    Directory.CreateDirectory(Location.FullName + "\\Images\\Thumbnails\\");
                }

                if (!Directory.Exists("Data"))
                {
                    Directory.CreateDirectory("Data");
                }

                var path = @"Data\Countries.sqlite";

                connection = new SQLiteConnection("Data Source=" + path);
                connection.Open();

                string sqlcommand = "create table if not exists Countries(alpha3code char(3) primary key, " +
                    "alpha2code char(2), " +
                    "name nchar, capital nchar, " +
                    "region nchar, subregion nchar," +
                    "population int, area decimal(10,2)," +
                    "gini decimal(4,2))";

                command = new SQLiteCommand(sqlcommand, connection);

                command.ExecuteNonQuery();

                sqlcommand = "create table if not exists Currencies(code nchar , name nchar unique, symbol nchar)";

                command.CommandText = sqlcommand;

                command.ExecuteNonQuery();

                sqlcommand = "create table if not exists CountryCurrencies(countrycode char(3),currencyname nchar, " +
                    "FOREIGN KEY(countrycode) REFERENCES Countries(alpha3code), " +
                    "FOREIGN KEY(currencyname) REFERENCES Currencies(name))";

                command.CommandText = sqlcommand;

                command.ExecuteNonQuery();

                sqlcommand = "create table if not exists rates(rateId int, code varchar(5),taxRate real, name varchar(250))";

                command.CommandText = sqlcommand;

                command.ExecuteNonQuery();
            }
            catch
            {

            }
        }

        public async Task SaveData(List<Country> Countries, List<Rate> Rates, IProgress<ProgressReport> progress)
        {
            var transaction = connection.BeginTransaction();

            try
            {
                ProgressReport report = new ProgressReport();

                foreach (var country in Countries)
                {
                    string sql = $"insert into Countries(alpha3code, alpha2code, name, capital, region, subregion, population, area, gini)" +
                        $" values('{country.alpha3Code}','{country.alpha2Code}','{country.name.Replace("'", "''")}','{country.capital.Replace("'", "''")}','{country.region}','{country.subregion}',{country.population},'{country.area}','{country.gini}')";

                    command.CommandText = sql;

                    await command.ExecuteNonQueryAsync();

                    foreach (var currency in country.currencies)
                    {
                        if (currency.name != null)
                        {
                            sql = $"insert or ignore into Currencies(code,name,symbol) values('{currency.code}','{currency.name.Replace("'", "''")}','{currency.symbol}')";

                            command.CommandText = sql;

                            await command.ExecuteNonQueryAsync();

                            sql = $"insert into CountryCurrencies(countrycode,currencyname) values('{country.alpha3Code}','{currency.name.Replace("'", "''")}')";

                            command.CommandText = sql;

                            await command.ExecuteNonQueryAsync();
                        }
                    }

                    report.PercentageCompleted++;
                    report.Description = $"Updating DB({report.PercentageCompleted}/{Countries.Count + Rates.Count})";
                    progress.Report(report);
                }

                foreach (var rate in Rates)
                {
                    string sql = $"insert into Rates (rateId, code, taxRate, name)" +
                        $" values({rate.rateId},'{rate.code}',{rate.taxRate},'{rate.name}')";

                    command.CommandText = sql;

                    await command.ExecuteNonQueryAsync();

                    report.PercentageCompleted++;
                    report.Description = $"Updating DB({report.PercentageCompleted}/{Countries.Count + Rates.Count})";
                    progress.Report(report);
                }

                transaction.Commit();
                connection.Close();
            }
            catch (Exception e)
            {
                transaction.Rollback();
                connection.Close();
                MessageBox.Show(e.Message);
            }
        }

        public List<Country> GetDataCountries()
        {
            List<Country> countries = new List<Country>();

            try
            {
                string sql = "select alpha3code, alpha2code, name, capital, region, subregion, population, area, gini from Countries";

                command = new SQLiteCommand(sql, connection);

                SQLiteDataReader reader = command.ExecuteReader();

                while (reader.Read())
                {
                    countries.Add(new Country
                    {
                        alpha3Code = (string)reader["alpha3code"],
                        alpha2Code = (string)reader["alpha2code"],
                        name = (string)reader["name"],
                        capital = (string)reader["capital"],
                        region = (string)reader["region"],
                        subregion = (string)reader["subregion"],
                        population = (int)reader["population"],
                        area = Convert.ToDouble((decimal)reader["area"]),
                        gini = Convert.ToDouble((decimal)reader["gini"]),
                        currencies = new List<Currency>()
                    });
                }

                CountriesCurrency(countries);

                return countries;
            }
            catch (Exception e)
            {
                MessageBox.Show(e.Message);
                return null;
            }
        }
        public List<Rate> GetDataRates()
        {
            List<Rate> rates = new List<Rate>();

            try
            {
                string sql = "select rateId, code, taxRate, name from Rates";

                command = new SQLiteCommand(sql, connection);

                SQLiteDataReader reader = command.ExecuteReader();

                while (reader.Read())
                {
                    rates.Add(new Rate
                    {
                        rateId = (int)reader["rateId"],
                        code = (string)reader["code"],
                        taxRate = (double)reader["taxRate"],
                        name = (string)reader["name"]
                    });
                }

                connection.Close();

                return rates;
            }
            catch (Exception e)
            {
                MessageBox.Show(e.Message);
                return null;
            }
        }
        public async Task DeleteData()
        {
            try
            {
                string sql = "delete from Countries";

                command.CommandText = sql;

                await command.ExecuteNonQueryAsync();

                sql = "delete from Currencies";

                command.CommandText = sql;

                await command.ExecuteNonQueryAsync();

                sql = "delete from CountryCurrencies";

                command.CommandText = sql;

                await command.ExecuteNonQueryAsync();

                sql = "delete from rates";

                command.CommandText = sql;

                await command.ExecuteNonQueryAsync();

            }
            catch (Exception e)
            {
                MessageBox.Show(e.Message);
            }
        }

        private void CountriesCurrency(List<Country> countries)
        {
            foreach (Country country in countries)
            {
                string sql = $"select DISTINCT Currencies.code,Currencies.name,Currencies.symbol From CountryCurrencies" +
                    $" INNER JOIN Currencies on currencyname = Currencies.name " +
                    $"INNER JOIN Countries on countrycode = alpha3code WHERE alpha3code = \"{country.alpha3Code}\"";

                command = new SQLiteCommand(sql, connection);

                SQLiteDataReader reader = command.ExecuteReader();

                while (reader.Read())
                {
                    country.currencies.Add(new Currency
                    {
                        code = (string)reader["code"],
                        name = (string)reader["name"],
                        symbol = (string)reader["symbol"]
                    });
                }
            }
        }

        public async Task RunDownloadParallelAsync(List<Country> Countries, IProgress<ProgressReport> progress)
        {
            List<Task> tasks = new List<Task>();

            ProgressReport report = new ProgressReport();

            foreach (Country country in Countries)
            {
                tasks.Add(Task.Run(() => DownloadSVG(country, progress, report, Countries.Count * 2)));

                tasks.Add(Task.Run(() => DownloadThumbnail(country, progress, report, Countries.Count * 2)));
            }

            await Task.WhenAll(tasks);
        }

        private void DownloadSVG(Country country, IProgress<ProgressReport> progress, ProgressReport report, int count)
        {
            if (!File.Exists($"{Location.FullName}\\Images\\{country.name}.svg"))
            {
                using (WebClient webClient = new WebClient())
                {
                    try
                    {
                        webClient.DownloadFile(new Uri(country.flag), $"{Location.FullName}\\Images\\{country.name}.svg");

                        country.caminhoImage = $"{Location.FullName}\\Images\\{country.name}.svg";
                    }
                    catch
                    {
                        country.caminhoImage = Location.FullName + "\\Resources\\notavailable.svg";
                    }
                }
            }
            else
            {
                country.caminhoImage = $"{Location.FullName}\\Images\\{country.name}.svg";
            }

            report.PercentageCompleted++;
            report.Description = $"Fetching Flags({report.PercentageCompleted}/{count})";
            progress.Report(report);
        }

        private void DownloadThumbnail(Country country, IProgress<ProgressReport> progress, ProgressReport report, int count)
        {
            if (!File.Exists($"{Location.FullName}\\Images\\Thumbnails\\{country.name}.png"))
            {
                using (WebClient webClient = new WebClient())
                {
                    try
                    {
                        webClient.DownloadFile(new Uri("https://www.countryflags.io/" + $"{country.alpha2Code}" + "/shiny/64.png"), $"{Location.FullName}\\Images\\Thumbnails\\{country.name}.png");

                        country.caminhoThumbnail = $"{Location.FullName}\\Images\\Thumbnails\\{country.name}.png";
                    }
                    catch
                    {
                    }
                }
            }
            else
            {
                country.caminhoThumbnail = $"{Location.FullName}\\Images\\Thumbnails\\{country.name}.png";
            }

            report.PercentageCompleted++;
            report.Description = $"Fetching Flags({report.PercentageCompleted}/{count})";
            progress.Report(report);
        }

        public void CheckImages(List<Country> Countries)
        {
            foreach (Country country in Countries)
            {
                try
                {
                    if (File.Exists($"{Location.FullName}\\Images\\{country.name}.svg"))
                    {
                        country.caminhoImage = $"{Location.FullName}\\Images\\{country.name}.svg";
                    }
                    else
                    {
                        country.caminhoImage = Location.FullName + "\\Resources\\notavailable.svg";
                    }

                    if (File.Exists($"{Location.FullName}\\Images\\Thumbnails\\{country.name}.png"))
                    {
                        country.caminhoThumbnail = $"{Location.FullName}\\Images\\Thumbnails\\{country.name}.png";
                    }
                }
                catch (Exception)
                {

                }
            }
        }
    }
}
