using System.Collections.ObjectModel;

namespace Countries.Modelos
{
    public class Continent
    {
        public string Name { get; set; }

        public ObservableCollection<Country> CountriesList { get; set; } = new ObservableCollection<Country>();
    }
}
