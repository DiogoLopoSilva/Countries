namespace Countries.Servicos
{
    using Countries.Modelos;
    using System;
    using System.Collections.Generic;
    using System.Data.SQLite;
    using System.Globalization;
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

                command.CommandText = sqlcommand;

                command.ExecuteNonQuery();

                sqlcommand = "create table if not exists CountryCurrencies(countrycode char(3),currencyname nchar, FOREIGN KEY(countrycode) REFERENCES Countries(alpha3code), FOREIGN KEY(currencyname) REFERENCES Currencies(name))";

                command.CommandText = sqlcommand;

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

                    command.CommandText = sql;

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

                            command.CommandText = sql;

                            await command.ExecuteNonQueryAsync();

                            sql = $"insert into CountryCurrencies(countrycode,currencyname) values('{country.alpha3Code}','{currencyname}')";

                            command.CommandText = sql;

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

        public List<Country> GetData()
        {
            List<Country> countries = new List<Country>();

            try
            {
                string sql = "select alpha3code, alpha2code, name, capital, region, subregion, population, area, gini from Countries";

                command = new SQLiteCommand(sql, connection);

                //Lê cada registo
                SQLiteDataReader reader = command.ExecuteReader();
                // string test=null;

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
                        currencies=new List<Currency>()
                    });
                }

                CountriesCurrency(countries);

                connection.Close();

                return countries;
            }
            catch(Exception e)
            {
                MessageBox.Show(e.Message);
                return null;
            }
        }

        private void CountriesCurrency(List<Country> countries)
        {
            foreach (Country country in countries)
            {
                string sql = $"select DISTINCT Currencies.code,Currencies.name,Currencies.symbol From CountryCurrencies INNER JOIN Currencies on currencyname = Currencies.name INNER JOIN Countries on countrycode = alpha3code WHERE alpha3code = \"{country.alpha3Code}\"";

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

            }
            catch(Exception e)
            {
                MessageBox.Show(e.Message);
            }
        }
    }
}
