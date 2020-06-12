using System;
using System.Collections.Generic;

namespace Countries.Modelos
{
    public class CovidData
    {
        public GlobalData Global { get; set; }
        public IList<CountryData> Countries { get; set; }
        public DateTime Date { get; set; }
    }
}
