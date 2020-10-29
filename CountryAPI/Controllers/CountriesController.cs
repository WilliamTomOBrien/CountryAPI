using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using CountryAPI.Models;
using TodoApi.Models;

namespace CountryAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CountriesController : ControllerBase
    {
        private readonly PathContext _context;
        public readonly string DefaultStartCountry = "USA";

        public CountriesController(PathContext context)
        {
            _context = context;
        }

        // GET: api/Countries/USA
        [HttpGet("{id}")]
        public async Task<ActionResult<IEnumerable<string>>> GetPath(string id)
        {

            List<Country> countries = await _context.Countries.ToListAsync();
            List<CountryConnection> connections = await _context.Connections.ToListAsync();

            // Lazy-loading seems to require us to pull the CountryConnections to ensure they
            // load in the country objects.

            Country source = countries.Find(x => x.Name == DefaultStartCountry);
            Country destination = countries.Find(x => x.Name == id);

            // Find the source, and ensure the destination exists

            if (destination == null)
            {
                return StatusCode(StatusCodes.Status400BadRequest,
                                            new { message = $"No country of name {id}" });
            }

            CountryGraph graph = new CountryGraph(source, destination, countries);
            // Create the minimum spanning tree that contains the source and destination
            // with the source as it's root



            List<Country> pathList = graph.CreatePathList(source, destination);
            // Determine the minimum length path from source to destination, if it exists

            if (pathList == null)
            {
                return StatusCode(StatusCodes.Status400BadRequest,
                            new { message = $"No Valid Path From {DefaultStartCountry} To Destination {id}" });
            }

            // Return the names of the countries in the required path order
            return pathList.Select(a => a.Name).ToList();
        }


        private class CountryGraph
        {

            private Dictionary<Country, ulong> countryDistance; // Holds distance from start to the Key Country
            Dictionary<Country, Country> previousCountry; // Holds the next node on the path from the key node to the source

            public CountryGraph(Country source, Country destination, List<Country> countries)
            {
                // Utilizes Djikstra's algorithm to create the minimum spanning tree that contains
                // the source and destination node

                countryDistance = new Dictionary<Country, ulong>();
                previousCountry = new Dictionary<Country, Country>();

                List<Country> visitedCountries = new List<Country>();

                countryDistance.Add(source, 0);

                foreach (Country country in countries)
                {
                    // for every non-source country, initialize distance dictionary with
                    // a maximum distance

                    if (!country.Equals(source))
                    {
                        countryDistance.Add(country, ulong.MaxValue);
                    }

                    country.SetConnections(countries);
                }

                ulong? minimumDistance = 0;

                while (source != null && minimumDistance != ulong.MaxValue)
                {
                    visitedCountries.Add(source);

                    // As we "visit" a country, add it to a list so we don't do so again

                    ulong distance = 0;
                    countryDistance.TryGetValue(source, out distance);

                    foreach (Country country in source.Connections)
                    {
                        ulong previousDistance = 0;
                        countryDistance.TryGetValue(country, out previousDistance);

                        ulong newDistance = distance + source.GetDistanceToConnected(country);

                        if (previousDistance > newDistance)
                        {
                            // If we've found a path to a country with less distance than
                            // one previously found, replace the listed paths in the data structures

                            countryDistance.Remove(country);
                            countryDistance.Add(country, newDistance);

                            previousCountry.Remove(country);
                            previousCountry.Add(country, source);

                        }
                    }


                    // Find the country that has not yet been visited with the least distance
                    // from the source

                    minimumDistance = countryDistance.Where(d => !visitedCountries.Contains(d.Key))
                                                     .Min(a => (ulong?)a.Value);

                    if (minimumDistance != null)
                    {
                        source = countryDistance.Where(d => !visitedCountries.Contains(d.Key))
                                                .Where(d => d.Value == minimumDistance).FirstOrDefault().Key;
                    }
                    else
                    {
                        source = null;
                    }

                    if (source != null && source.Equals(destination))
                    {
                        // If we are visiting the destination, we have found the optimal path
                        // from the source to destination, end the loop

                        source = null;
                    }
                }
            }

            public List<Country> CreatePathList(Country source, Country destination)
            {
                // Creates an ordered list to describe the path from the source country
                // to the destination country, if it exists.

                Country prev = null;
                previousCountry.TryGetValue(destination, out prev);

                if (prev == null && source.Id != destination.Id)
                {
                    return null;
                }

                List<Country> path = new List<Country>();

                // Start at destination
                path.Add(destination);

                while (prev != null)
                {
                    // Continue iterating on the "previous" node
                    // until we reach the beginning of the path

                    path.Add(prev);
                    previousCountry.TryGetValue(prev, out prev);
                }

                path.Reverse();

                return path;
            }
        }
    }
}
