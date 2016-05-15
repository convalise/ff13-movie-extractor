
using System.Linq;

namespace com.convalise.FF13MovieExtractor
{

public class MovieExtractor
{
	public static void Main(string[] args)
	{
		if((args.Length < 3) || !args[0].EndsWith(".wdb"))
		{
			System.Console.Out.WriteLine();
			System.Console.Out.WriteLine("usage: FF13MovieExtractor <wdb-file> [commands]");
			System.Console.Out.WriteLine();
			System.Console.Out.WriteLine("The wdb database file can be found within your game disc.");
			System.Console.Out.WriteLine();
			System.Console.Out.WriteLine("Available commands:");
			System.Console.Out.WriteLine("    -dumpoffsettable    Create a text file with the container, offset and length information of the movies.");
			System.Console.Out.WriteLine("    -dumpmovies         Extract all movies from their containers.");
			return;
		}

		FileSplitter fileSplitter = new FileSplitter(args[0]);

		Movie[] movieDatabase = fileSplitter.ReadDatabase();

		//Dumps the offset table to a text file
		if(args.Contains("-dumpoffsettable"))
		{
			fileSplitter.DumpOffsetTable(movieDatabase);
		}

		//Dumps the movies from their containers
		if(args.Contains("-dumpmovies"))
		{
			fileSplitter.DumpMovies(movieDatabase);
		}

	}

}

} /// End of namespace.
