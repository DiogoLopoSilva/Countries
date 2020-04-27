using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Countries.Modelos
{
   public class Continent
    {
        public string Name { get; set; }

        //Representa uma coleção de dados dinâmica que fornece notificações quando itens são adicionados, removidos ou quando a lista inteira é atualizada.
        public ObservableCollection<Country> CountriesList { get; set; } = new ObservableCollection<Country>();
    }
}
