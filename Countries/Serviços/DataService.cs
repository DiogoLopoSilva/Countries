namespace Countries.Servicos
{
    using Countries.Modelos;
    using System;
    using System.Collections.Generic;
    using System.Data.SQLite;
    using System.IO;
    using System.Threading.Tasks;
    using System.Windows;

    public class DataService
    {
        private SQLiteConnection connection;
        private SQLiteCommand command;

        public DataService()
        {
            if (!Directory.Exists("Data"))
            {
                Directory.CreateDirectory("Data");
            }

            var path = @"Data\Countries.sqlite";

            try
            {
                connection = new SQLiteConnection("Data Source=" + path);
                connection.Open();

                string sqlcommand = "create table if not exists Countries(alpha3code char(3) primary key, alpha2code char(2), name nchar, capital nchar, region nchar, subregion nchar,population int, area decimal(10,2),gini decimal(4,2))";

                command = new SQLiteCommand(sqlcommand, connection);

                command.ExecuteNonQuery();

                sqlcommand = "create table if not exists Currencies(code nchar , name nchar unique, symbol nchar)";

                command = new SQLiteCommand(sqlcommand, connection);

                command.ExecuteNonQuery();

                sqlcommand = "create table if not exists CountryCurrencies(countrycode char(3),currencyname nchar, FOREIGN KEY(countrycode) REFERENCES Countries(alpha3code), FOREIGN KEY(currencyname) REFERENCES Currencies(name))";

                command = new SQLiteCommand(sqlcommand, connection);

                command.ExecuteNonQuery();
            }
            catch
            {

            }
        }

        public async Task SaveData(List<Country> Countries)
        {
            try
            {
                foreach (var country in Countries)
                {
                    string name = country.name;

                    string capital = country.capital;

                    if (name.Contains("'"))
                    {
                       name = name.Replace("'", "+");
                    }

                    if (capital.Contains("'"))
                    {
                        capital = capital.Replace("'", "+");
                    }

                    string sql = $"insert into Countries(alpha3code, alpha2code, name, capital, region, subregion, population, area, gini)" +
                        $" values('{country.alpha3Code}','{country.alpha2Code}','{name}','{capital}','{country.region}','{country.subregion}',{country.population},'{country.area}','{country.gini}')";

                    command = new SQLiteCommand(sql, connection);

                    await command.ExecuteNonQueryAsync();

                    foreach (var currency in country.currencies)
                    {
                        string currencyname = currency.name;

                        if (currency.name!=null)
                        {
                            if (currencyname.Contains("'"))
                            {
                                currencyname = currencyname.Replace("'", "+");
                            }

                            sql = $"insert or ignore into Currencies(code,name,symbol) values('{currency.code}','{currencyname}','{currency.symbol}')";

                            command = new SQLiteCommand(sql, connection);

                            await command.ExecuteNonQueryAsync();

                            sql = $"insert into CountryCurrencies(countrycode,currencyname) values('{country.alpha3Code}','{currencyname}')";

                            command = new SQLiteCommand(sql, connection);

                            await command.ExecuteNonQueryAsync();
                        }
                    }                
                }

                connection.Close();
            }
            catch (Exception e)
            {
                MessageBox.Show(e.Message);
            }           
        }

        public List<Rate> GetData()
        {
            List<Rate> rates = new List<Rate>();

            try
            {
                string sql = "select rateId, code, taxRate, name from Rates";

                command = new SQLiteCommand(sql, connection);

                //Lê cada registo
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
            catch
            {
                return null;
            }
        }

        public async Task DeleteData()
        {
            try
            {
                string sql = "delete from Countries";

                command = new SQLiteCommand(sql, connection);

                await command.ExecuteNonQueryAsync();

                sql = "delete from Currencies";

                command = new SQLiteCommand(sql, connection);

                await command.ExecuteNonQueryAsync();

                sql = "delete from CountryCurrency";

                command = new SQLiteCommand(sql, connection);

                await command.ExecuteNonQueryAsync();

            }
            catch
            {
              
            }
        }
    }
}
