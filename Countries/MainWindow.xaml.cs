using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;

namespace Countries
{
    using Microsoft.Maps.MapControl.WPF;
    using Modelos;
    using System.Linq;
    using System.Windows.Input;

    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private readonly List<Country> Countries;
        private readonly List<Rate> Rates;
        public CovidData CovidDataList { get; }

        public MainWindow(LoadWindow loadWindow)
        {
            Countries = loadWindow.Countries;
            Rates = loadWindow.Rates;
            CovidDataList = loadWindow._CovidData;
            GetCovidByCountry(Countries);

            InitializeComponent();

            var connection = loadWindow.networkService.CheckConnection();

            if (!connection.IsSuccess)
            {
                TabMap.Visibility = Visibility.Hidden;
                TabCovid.Visibility = Visibility.Hidden;
            }

            listBoxPaises.ItemsSource = Countries;
            treeContinents.ItemsSource = GetContinents(Countries);
            cbWorldCurrencies.ItemsSource = Rates;

            string CountryCode = loadWindow.LocalCountry;

            if (!string.IsNullOrEmpty(CountryCode))
            {
                Country selectedCountry = Countries.Find(c => c.alpha2Code == CountryCode);

                if (selectedCountry != null)
                {
                    listBoxPaises.SelectedItem = selectedCountry;
                }
                else
                {
                    listBoxPaises.SelectedIndex = 0;
                }
            }
            else
            {
                listBoxPaises.SelectedIndex = 0;
            }
        }

        private List<Continent> GetContinents(List<Country> Paises)
        {
            List<Continent> Continents = new List<Continent>();
            Continent continent;

            foreach (var pais in Paises)
            {
                if (Continents.Find(x => x.Name == pais.region) == null)
                {
                    if (string.IsNullOrEmpty(pais.region))
                    {
                        if (Continents.Find(x => x.Name == "Others") == null)
                        {
                            continent = new Continent { Name = "Others" };
                            continent.CountriesList.Add(pais);

                            Continents.Add(continent);
                        }
                        else
                        {
                            Continents.First(x => x.Name == "Others").CountriesList.Add(pais);
                        }
                    }
                    else
                    {
                        continent = new Continent { Name = pais.region };
                        continent.CountriesList.Add(pais);

                        Continents.Add(continent);
                    }
                }
                else
                {
                    Continents.First(x => x.Name == pais.region).CountriesList.Add(pais);
                }
            }

            return Continents;
        }

        private string ConvertedValue(decimal valor, bool conversionType)
        {
            Currency findRate = (Currency)cbCountryCurrencies.SelectedItem;

            if (findRate == null)
            {
                return null;
            }

            if (Rates == null)
            {
                return null;
            }

            Rate rate1 = Rates.Find(r => r.code == findRate.code);

            Rate rate2 = (Rate)cbWorldCurrencies.SelectedItem;

            if (rate1 != null)
            {
                var valorConvertido = conversionType == true ? valor / (decimal)rate1.taxRate * (decimal)rate2.taxRate : valor / (decimal)rate2.taxRate * (decimal)rate1.taxRate;

                return valorConvertido.ToString("N4");
            }
            else
            {
                return "Conversion not available!";
            }
        }

        private void SearchLists(string text)
        {
            if (Countries != null)
            {
                IEnumerable<Country> lista = Countries.FindAll(x => x.name.ToLower().Contains(text.ToLower()));

                listBoxPaises.ItemsSource = lista;

                treeContinents.ItemsSource = GetContinents(lista.ToList());

                listBoxPaises.SelectedIndex = 0;
            }
        }

        private void GetCovidByCountry(List<Country> countries)
        {
            foreach (Country country in countries)
            {
                if (CovidDataList != null)
                {
                    country.CountryCovidData = CovidDataList.Countries.FirstOrDefault(c => c.CountryCode == country.alpha2Code);
                }
            }
        }

        private void searchbox_TextChanged(object sender, TextChangedEventArgs e)
        {
            SearchLists(searchbox.Text);
        }

        private void btnMinimize_Click(object sender, RoutedEventArgs e)
        {
            mainGrid.ColumnDefinitions[0].Width = new GridLength(0, GridUnitType.Star);
            btnMaximize.Visibility = Visibility.Visible;
        }

        private void btnMaximize_Click(object sender, RoutedEventArgs e)
        {
            mainGrid.ColumnDefinitions[0].Width = new GridLength(0.9, GridUnitType.Star);
            btnMaximize.Visibility = Visibility.Hidden;
        }

        private void listBoxPaises_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            Country country = (Country)listBoxPaises.SelectedItem;

            if (country != null && country.latlng != null && country.latlng.Count > 0)
            {
                Location location = new Location { Latitude = country.latlng[0], Longitude = country.latlng[1] };

                Mapa.Center = location;
            }

            countryAmount.Clear();
            worldAmount.Clear();
        }

        private void treeContinents_SelectedItemChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
        {
            listBoxPaises.SelectedItem = treeContinents.SelectedItem;
        }

        private void tabContryContinent_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (e.Source is TabControl)
            {
                if (tabContryContinent.SelectedIndex == 1)
                {
                    Keyboard.ClearFocus();
                }
            }
        }

        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            Grid.SetColumn(countryCurrenciesPanel, Grid.GetColumn(countryCurrenciesPanel) == 0 ? 2 : 0);

            Grid.SetColumn(worldCurrenciesPanel, Grid.GetColumn(worldCurrenciesPanel) == 2 ? 0 : 2);
        }

        private void countryAmount_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (countryAmount.IsFocused)
            {
                if (string.IsNullOrEmpty(countryAmount.Text))
                {
                    worldAmount.Clear();
                    return;
                }

                if (!decimal.TryParse(countryAmount.Text, out decimal valor))
                {
                    worldAmount.Text = "Please insert a valid value";
                    return;
                }

                worldAmount.Text = ConvertedValue(valor, true);
            }
        }

        private void worldAmount_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (worldAmount.IsFocused)
            {
                if (string.IsNullOrEmpty(worldAmount.Text))
                {
                    countryAmount.Clear();
                    return;
                }

                if (!decimal.TryParse(worldAmount.Text, out decimal valor))
                {
                    countryAmount.Text = "Please insert a valid value";
                    return;
                }

                countryAmount.Text = ConvertedValue(valor, false);
            }
        }

        private void cbCountryCurrencies_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (IsLoaded)
            {

                if (string.IsNullOrEmpty(worldAmount.Text))
                {
                    countryAmount.Clear();
                    return;
                }

                if (!decimal.TryParse(worldAmount.Text, out decimal valor))
                {
                    countryAmount.Text = "Please insert a valid value";
                    return;
                }

                countryAmount.Text = ConvertedValue(valor, false);
            }
        }

        private void cbWorldCurrencies_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (IsLoaded)
            {
                if (string.IsNullOrEmpty(countryAmount.Text))
                {
                    worldAmount.Clear();
                    return;
                }

                if (!decimal.TryParse(countryAmount.Text, out decimal valor))
                {
                    worldAmount.Text = "Please insert a valid value";
                    return;
                }

                worldAmount.Text = ConvertedValue(valor, true);
            }
        }
    }
}
