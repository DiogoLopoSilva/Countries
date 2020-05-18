using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;

namespace Countries
{
    using Modelos;
    using Servicos;
    using Svg;
    using System;
    using System.Collections.ObjectModel;
    using System.Linq;

    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private readonly List<Country> Paises;
        //public ObservableCollection<Rate> Rates { get; set; }
        public List<Rate> Rates { get; set; }
        private readonly ApiService apiService;
        private readonly NetworkService networkService;
        private readonly List<Continent> Continents;
        //private readonly DataService dataService;

        public MainWindow(List<Country> paises, List<Rate> rates)
        {
            InitializeComponent();
            apiService = new ApiService(); 
            networkService = new NetworkService();
            //dataService = new DataService();
            Paises = paises;
            //Rates = new ObservableCollection<Rate>(rates);
            Rates = rates;
            Continents = GetContinents(Paises);
            listBoxPaises.ItemsSource = Paises;
            treeContinents.ItemsSource = Continents;

            this.DataContext = listBoxPaises;

            cbWorldCurrencies.ItemsSource = Rates;

            //dataService.DeleteData();
            //dataService.SaveData(Paises);
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

        private void searchbox_TextChanged(object sender, TextChangedEventArgs e)
        {
            SearchLists(searchbox.Text);
        }

        private void SearchLists(string text)
        {
            //if (searchbox.DataContext.ToString() == "listBoxPaises")
            //{
            //    if (Paises != null)
            //    {
            //        IEnumerable<Country> lista = Paises.FindAll(x => x.name.ToLower().Contains(text.ToLower()));

            //        listBoxPaises.ItemsSource = lista;
            //    }
            //}
            //else
            //{
            //    if (Paises != null)
            //    {
            //        IEnumerable<Country> lista = Paises.FindAll(x => x.name.ToLower().Contains(text.ToLower()));

            //        treeContinents.ItemsSource = GetContinents(lista.ToList());
            //    }
            //}

            if (Paises != null)
            {
                //listBoxPaises.SelectedIndex = -1;

                IEnumerable<Country> lista = Paises.FindAll(x => x.name.ToLower().Contains(text.ToLower()));

                listBoxPaises.ItemsSource = lista;

                treeContinents.ItemsSource = GetContinents(lista.ToList());
            }
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            //tabContryContinent.Width = tabContryContinent.Width == 300 ? 50 : 300;
            //tabContryContinent.HorizontalAlignment = HorizontalAlignment.Left;
            //btnList.Content = btnList.Content.ToString() == "<" ? ">" : "<";

            //listBoxPaises.Width = listBoxPaises.Width == 300 ? 50 : 300;
            //btnList.Content = btnList.Content.ToString() == "<" ? ">" : "<";
        }

        private void TabControl_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (e.Source is TabControl)
            {
                if (tabContryContinent.IsLoaded)
                {
                    if (tabContryContinent.SelectedIndex == 0)
                    {
                        searchbox.DataContext = "listBoxPaises";
                        this.DataContext = listBoxPaises;
                    }
                    else
                    {
                        searchbox.DataContext = "treeContinents";
                        this.DataContext = treeContinents;
                    }

                    searchbox.Clear(); //Nao fazer isto, arranjar alternativa
                }

               

                //SearchLists(searchbox.Text);
            }
        }
        private void listBoxPaises_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            //this.DataContext = listBoxPaises;

            //groupBoxCurrencies.Visibility = groupBoxCurrencies.Visibility == Visibility.Hidden ? Visibility.Visible : Visibility.Hidden;

        }
        private void treeContinents_SelectedItemChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
        {
            //this.DataContext = treeContinents;

            //groupBoxCurrencies.Visibility = groupBoxCurrencies.Visibility == Visibility.Hidden ? Visibility.Visible : Visibility.Hidden;
        }
    }
}
