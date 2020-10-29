using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace CountryAPI.Models
{
    public class Country
    {
        public ulong Id { get; set; }
        public string Name { get; set; }
        public List<CountryConnection> CountryConnections { get; set; }

        [NotMapped]
        public List<Country> Connections { get; set; }
        // value derived from CountryConnections list for easier use
        // using a List<Country> directly violates Entity model rules, as
        // it creates a Many-Many self relationship.


        public Country(ulong id, string name)
        {
            this.Id = id;
            this.Name = name;
            CountryConnections = new List<CountryConnection>();
        }

        public void AddConnections(List<Country> countries)
        {
            // Allows us to add a list of countries, and also have them
            // be added to the Entity Model relevant data structures

            Connections = countries;
            foreach (Country country in countries)
            {
                CountryConnection conn = new CountryConnection();
                conn.CountryOneID = this.Id;
                conn.CountryTwoID = country.Id;

                CountryConnections.Add(conn);
            }
        }

        public void SetConnections(List<Country> allCountries)
        {
            // If we haven't set the list of connected countries based on the IDs
            // from the database, we do that here

            if (Connections == null || CountryConnections.Count != Connections.Count)
            {
                List<ulong> ids = CountryConnections.Select(a => a.CountryOneID).Concat(
                                  CountryConnections.Select(a => a.CountryTwoID).ToList())
                                                    .Where(a => a != this.Id).ToList();

                Connections = allCountries.Where(a => ids.Contains(a.Id)).ToList();
            }
        }

        public ulong GetDistanceToConnected(Country country)
        {
            if (Connections.Contains(country))
            {
                return 1;
            }
            else return ulong.MaxValue;
        }


    }   
}
