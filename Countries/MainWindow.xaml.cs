using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;

namespace Countries
{
    using Modelos;
    using Servicos;
    using System.Linq;

    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private readonly List<Country> Paises;
        private readonly ApiService apiService;
        private readonly NetworkService networkService;
        private readonly List<Continent> Continents;
        public MainWindow(List<Country> paises)
        {
            InitializeComponent();
            apiService = new ApiService();
            networkService = new NetworkService();
            Paises = paises;
            Continents = GetContinents(Paises);
            listBoxPaises.ItemsSource = Paises;
            treeContinents.ItemsSource = Continents;
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
            if (searchbox.DataContext.ToString() == "listBoxPaises")
            {
                if (Paises != null)
                {
                    IEnumerable<Country> lista = Paises.FindAll(x => x.name.ToLower().Contains(searchbox.Text.ToLower()));

                    listBoxPaises.ItemsSource = lista;
                }
            }
            else
            {
                if (Paises != null)
                {
                    IEnumerable<Country> lista = Paises.FindAll(x => x.name.ToLower().Contains(searchbox.Text.ToLower()));

                    treeContinents.ItemsSource = GetContinents(lista.ToList());
                }
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
            if (tabContryContinent.IsLoaded)
            {
                if (tabContryContinent.SelectedIndex == 0)
                {
                    searchbox.DataContext = "listBoxPaises";
                }
                else
                {
                    searchbox.DataContext = "treeContinents";
                }
            }
        }
        private void listBoxPaises_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            imageFlag.DataContext = listBoxPaises;
            name.DataContext = listBoxPaises;
            capital.DataContext = listBoxPaises;
            population.DataContext = listBoxPaises;
            gini.DataContext = listBoxPaises;
            area.DataContext = listBoxPaises;

        }
        private void treeContinents_SelectedItemChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
        {
            imageFlag.DataContext = treeContinents;
            name.DataContext = treeContinents;
            capital.DataContext = treeContinents;
            population.DataContext = treeContinents;
            gini.DataContext = treeContinents;
            area.DataContext = treeContinents;
        }
    }
}
