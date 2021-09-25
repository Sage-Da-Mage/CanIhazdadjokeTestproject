using Learning.Models.ViewModels;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Net.Http;
using Microsoft.Extensions.DependencyInjection;
using System.Net.Http.Headers;
using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;

namespace Learning.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class JokeController : ControllerBase
    {
        private static readonly HttpClient client = new HttpClient();


        private readonly MyContext _context;  
        public JokeController(MyContext context)
        {
            _context = context;
        }

        // The Endpoint Joke -- Returns a single joke with an Id, Joke message and the time it was retrieved
        [HttpGet]
        public async Task<ActionResult<EntityJoke>> GetJoke(MyContext context)
        {
            // Get a joke (URL needed https://icanhazdadjoke.com/)
            // The following four lines set up the client so that I can retrieve 
            client.DefaultRequestHeaders.Accept.Clear();
            client.DefaultRequestHeaders.Accept.Add(
                new MediaTypeWithQualityHeaderValue("application/json"));
            client.DefaultRequestHeaders.Add("User-Agent", "Sage");


            // The following two lines get the joke from icanhazdadjoke.com
            // and then unserializes it from the Json format
            var streamTask = await client.GetStreamAsync("https://icanhazdadjoke.com/");
            var deserializedJoke = await JsonSerializer.DeserializeAsync<CompletedJoke>(streamTask);


            //Get the UTC time at the time of deserialization completing
            DateTime utcTime = DateTime.UtcNow;


            // Save joke and UTC time to a database
            //{

            // Add an entity to the postgress sql database
            EntityJoke joke;
            
            // Comenting out old using code
            //using (var context = new MyContext(new DbContextOptions<MyContext>()))
                                    
            joke = new EntityJoke();
            joke.Message = deserializedJoke.joke;
            joke.Time = utcTime;

            _context.EntityJokes.Add(joke);
            await _context.SaveChangesAsync();

            //}

            // Return the joke
            return Ok(joke);
        }


        // The Endpoint CountJoke -- Returns a number of jokes with an Id, Joke message and the time
        // it was retrieved based on an inputted value
        [HttpGet ("Count")]
        public async Task<ActionResult<List<EntityJoke>>> CountJoke(int count)
        {

            // Create a list of Jokes so that it can be filled wit jokes and returned at the end
            List<EntityJoke> listOfJokeCount = new List<EntityJoke>();

            //recieve input for how many jokes to get/log
            // Create a loop to get/logg jokes as many times as inputted
            for (int i = 0; i < count; i++)
            {
                // Get a joke
                // URL needed https://icanhazdadjoke.com/
                //{
                client.DefaultRequestHeaders.Accept.Clear();
                client.DefaultRequestHeaders.Accept.Add(
                    new MediaTypeWithQualityHeaderValue("application/json"));
                client.DefaultRequestHeaders.Add("User-Agent", "Sage");
                //}

                // This works to get the unserialized Joke from icanhazdadjoke.com
                // Trying to deserialize the JSON into just the joke
                //{
                var streamTask = await client.GetStreamAsync("https://icanhazdadjoke.com/");
                var deserializedJoke = await JsonSerializer.DeserializeAsync<CompletedJoke>(streamTask);
                //}

                //Get the UTC time at the time of deserialization completing
                DateTime utcTime = DateTime.UtcNow;


                // Save joke and UTC time to a database
                //{

                // Add an entity to the database
                EntityJoke joke;
                
                // Commenting out old using code
                //using (var context = new MyContext(new DbContextOptions<MyContext>()))
                {
                    
                    joke = new EntityJoke();
                    joke.Message = deserializedJoke.joke;
                    joke.Time = utcTime;


                    _context.EntityJokes.Add(joke);
                    await _context.SaveChangesAsync();

                }
                //}

                // Add the current joke to a List of jokes that will be returned
                listOfJokeCount.Add(joke); 
            }
            // This is the end of the loop. It has logged as many jokes as "count" has requested
            // and has added the jokes to a list of jokes to return.


            // Return the the joke(s) that were called
            return Ok(listOfJokeCount);

        }


        // The Endpoint History, retrieves a specified number of most recent requests including the date and joke
        [HttpGet ("history")]
        public async Task<ActionResult<List<EntityJoke>>> HistoryJoke(int count)
        {
            // Takes all the jokes (plus some diagnostics?) and puts them in databasewhole
            //var databaseWhole = context.EntityJokes.ToListAsync();
            
            var context = new MyContext(new DbContextOptions<MyContext>());
            

            // This line reads from the database from the newest entry (based on the Time variable)
            var historyOfJokes = await context.EntityJokes.OrderByDescending(b => b.Time).Take(count).ToListAsync();

            return Ok(historyOfJokes);

        }

        [HttpPost]
        public async Task<EntityJoke> postJokeTime([FromBody] EntityJoke postJoke)
        {

            //Get the UTC time at the time of posting the joke
            DateTime utcTime = DateTime.UtcNow;

            // Commenting out old using code
            //using (var context = new MyContext(new DbContextOptions<MyContext>()))
            {

                postJoke.Time = utcTime;


                _context.EntityJokes.Add(postJoke);
                await _context.SaveChangesAsync();

            }


            return postJoke;
        }




    }


    // The below are classes that facilitate the above endpoints

    // This class stores the deserialized joke so that it can be stored in it's entity
    public class CompletedJoke
    {
        // 
        public string joke { get; set; }

    }
    
    //{ The below classes are for Database creation/work

    // This is the entity for a joke, it includes an Id (generated by guid), a message
    // (the joke itself) and the time it was retrieved from icanhazdadjoke
    public class EntityJoke
    {
        [Key]
        public Guid Id { get; set; }
        
        
        public string Message { get; set; }
        public  DateTime Time { get; set; }


    }


    // My Context I worked on with Jean to create a Database (sqlite) 
    public class MyContext : DbContext
    {

        public MyContext(DbContextOptions<MyContext> options)
            : base(options)
        {
        }

        public DbSet<EntityJoke> EntityJokes { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            // The below lines are from before I connected the postgres SQL database
            // commented out for reference

            //var folder = Environment.SpecialFolder.LocalApplicationData;
            //var path = Environment.GetFolderPath(folder);
            //var DbPath = $"{path}/JokeDB.db";

            //optionsBuilder.UseSqlite($"Data Source={DbPath}");
        }
    }


    //} End of classes for database work


}
