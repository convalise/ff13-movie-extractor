
using com.convalise.Lib;

using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace com.convalise.FF13MovieExtractor
{

public class FileSplitter
{
	/// <summary> The size of the buffer used to read the movie from the container. </summary>
	private const int ReadBufferSize = 16 * 1024 * 1024;

	/// <summary> The name of the text file the offset table will be dumped to. </summary>
	private const string OffsetTableDumpFileName = "OffsetTable.txt";

	/// <summary> The movie name list offset on the wdb file. </summary>
	private const string MovieNameListOffset = "0090";

	/// <summary> The movie name list length on the wdb file. </summary>
	private const string MovieNameListLength = "0D40";

	/// <summary> The container name list offset on the wdb file. </summary>
	private const string ContainerNameListOffset = "0DD0";

	/// <summary> The container name list length on the wdb file. </summary>
	private const string ContainerNameListLength = "005A";



	/// <summary> The path to the wdb database file. </summary>
	public string DatabaseFilePath { get; private set; }

	/// <summary> The path to the containers folder. </summary>
	public string InputContainerFolder { get; private set; }
	
	/// <summary> The input container platform-based extension. </summary>
	public string InputContainerExtension { get; private set; }

	/// <summary> The path to the extracted movies folder. </summary>
	public string OutputMovieFolder { get; private set; }

	/// <summary> The output movie platform-based extension. </summary>
	public string OutputMovieExtension { get; private set; }

	/// <summary> The file sufix based on platform. </summary>
	public string PlatformSufix { get; private set; }

	/// <summary> The file sufix based on region. </summary>
	public string RegionSufix { get; private set; }



	public FileSplitter(string databaseFilePath)
	{
		this.DatabaseFilePath = databaseFilePath;

		string databaseFileName = Path.GetFileName(databaseFilePath);
		string databaseDirectoryPath = Path.GetDirectoryName(databaseFilePath);

		if(databaseFileName.Contains("win32"))
		{
			this.InputContainerExtension = ".win32.wmp";
			this.OutputMovieExtension = ".bk2";
			this.PlatformSufix = "_win32";
		}
		else if(databaseFileName.Contains("ps3"))
		{
			this.InputContainerExtension = ".ps3.wmp";
			this.OutputMovieExtension = ".pamf";
			this.PlatformSufix = "_ps3";
		}
		else if(databaseFileName.Contains("x360"))
		{
			this.InputContainerExtension = ".x360.wmp";
			this.OutputMovieExtension = ".bik";
			this.PlatformSufix = "_x360";
		}
		else
		{
			this.InputContainerExtension = string.Empty;
			this.OutputMovieExtension = string.Empty;
			this.PlatformSufix = string.Empty;
		}

		this.RegionSufix = databaseFileName.Contains("_us") ? "_us" : "";

		this.InputContainerFolder = string.IsNullOrEmpty(databaseDirectoryPath) ? string.Empty : databaseDirectoryPath.Replace("\\", "/") + "/";
		this.OutputMovieFolder = "extracted" + this.PlatformSufix + this.RegionSufix + "/";

	}

	/// <summary>
	/// Parses all movies information from the wdb database file.
	/// </summary>
	public Movie[] ReadDatabase()
	{
		if(!File.Exists(DatabaseFilePath))
		{
			System.Console.Error.WriteLine("File \"" + DatabaseFilePath + "\" not found!");
			return new Movie[0];
		}

		/// The collection of movie information.
		List<Movie> movieList = new List<Movie>();

		/// The byte array used for reading the file.
		byte[] byteArray = new byte[16];

		using(FileStream inputStream = new FileStream(DatabaseFilePath, FileMode.Open, FileAccess.Read))
		{
			if(!inputStream.CanSeek || !inputStream.CanRead)
			{
				System.Console.Error.WriteLine("Could not seek or read the database file content!");
				return new Movie[0];
			}

			/// Seeks to the beginning of the movies list info.
			inputStream.Seek(MovieNameListOffset.ToDecimal64(), SeekOrigin.Begin);

			long amountDataRead = 0L;
			long amountDataLeft = MovieNameListLength.ToDecimal64();

			/// First step: reads the name of the movies and their location info.
			while(amountDataLeft > 0L)
			{
				Movie movie = new Movie();

				amountDataLeft -= inputStream.Read(byteArray, 0, 16);
				movie.Name = byteArray.ToUTF8String(0, 16);

				amountDataLeft -= inputStream.Read(byteArray, 0, 16);
				movie.Offset = byteArray.ToHexString(0, 4);
				movie.Length = byteArray.ToHexString(4, 4);

				movieList.Add(movie);
			}

			/// Second step: reads the offset and length of movies inside the containers.
			for(int i = 0; i < movieList.Count; i++)
			{
				Movie movie = movieList[i];

				inputStream.Seek(movie.Offset.ToDecimal64(), SeekOrigin.Begin);
				inputStream.Read(byteArray, 0, 16);

				movie.Length = byteArray.ToHexString(4, 4);
				movie.Offset = byteArray.ToHexString(12, 4);

				inputStream.Seek(ContainerNameListOffset.ToDecimal64(), SeekOrigin.Begin);
				inputStream.Seek(byteArray.ToHexString(0, 4).ToDecimal64(), SeekOrigin.Current);
				inputStream.Read(byteArray, 0, 4);

				movie.Container = byteArray.ToUTF8String(0, 4) + RegionSufix;
			}

		} /// End of input stream.

		System.Console.Out.WriteLine("Database read completed.");

		return movieList.ToArray();

	}

	/// <summary>
	/// Dumps the movie database to a text file.
	/// </summary>
	public void DumpOffsetTable(Movie[] movieArray)
	{
		if((movieArray == null) || (movieArray.Length == 0))
		{
			System.Console.Error.WriteLine("Could not dump offset table: movie database is null or empty!");
			return;
		}

		StringBuilder infoTable = new StringBuilder();

		infoTable.Append("Container       ");
		infoTable.Append(" ");
		infoTable.Append("Movie           ");
		infoTable.Append(" ");
		infoTable.Append("Offset          ");
		infoTable.Append(" ");
		infoTable.Append("Length                          ");
		infoTable.AppendLine();

		infoTable.Append("----------------");
		infoTable.Append(" ");
		infoTable.Append("----------------");
		infoTable.Append(" ");
		infoTable.Append("----------------");
		infoTable.Append(" ");
		infoTable.Append("--------------------------------");
		infoTable.AppendLine();

		foreach(var movie in movieArray.OrderBy( movie => movie.Container ).ThenBy( movie => movie.Offset.ToDecimal64() ))
		{
			infoTable.Append(movie.Container.PadRight(16));
			infoTable.Append(" ");
			infoTable.Append(movie.Name.PadRight(16));
			infoTable.Append(" ");
			infoTable.Append("0x");
			infoTable.Append(movie.Offset.PadRight(14));
			infoTable.Append(" ");
			infoTable.Append("0x");
			infoTable.Append(movie.Length.PadRight(14));
			infoTable.Append((movie.Length.ToDecimal64() / 1024D / 1024D).ToString("F2", System.Globalization.CultureInfo.InvariantCulture).PadLeft(13));
			infoTable.Append(" MB");
			infoTable.AppendLine();
		}

		File.WriteAllText(OffsetTableDumpFileName, infoTable.ToString());

		System.Console.Out.WriteLine("Offset table dumped to file \"" + OffsetTableDumpFileName + "\".");
	}

	/// <summary>
	/// Dumps the movies from the containers.
	/// </summary>
	public void DumpMovies(Movie[] movieArray)
	{
		if((movieArray == null) || (movieArray.Length == 0))
		{
			System.Console.Error.WriteLine("Could not dump movies: movie database is null or empty!");
			return;
		}

		/// The byte array used for reading the file.
		byte[] byteArray = new byte[ReadBufferSize];

		/// The error counter.
		int errorCount = 0;

		if(!Directory.Exists(OutputMovieFolder))
		{
			Directory.CreateDirectory(OutputMovieFolder);
		}

		/// Will read movies grouped by their container for efficiency.
		foreach(var container in movieArray.GroupBy( movie => movie.Container ).OrderBy( container => container.Key ))
		{
			string inputContainerPath = Path.Combine(InputContainerFolder, container.Key + InputContainerExtension);

			if(!File.Exists(inputContainerPath))
			{
				System.Console.Error.WriteLine("Container \"" + inputContainerPath + "\" not found!");
				errorCount++;
				continue;
			}

			using(FileStream inputStream = new FileStream(inputContainerPath, FileMode.Open, FileAccess.Read))
			{
				System.Console.Out.WriteLine("Opening container \"" + inputContainerPath + "\" for reading.");

				if(!inputStream.CanSeek || !inputStream.CanRead)
				{
					System.Console.Error.WriteLine("Could not seek or read the container file content!");
					errorCount++;
					continue;
				}

				/// Will read movies ordered by their offset for a sequential reading attempt.
				foreach(Movie movie in container.OrderBy( movie => movie.Offset.ToDecimal64() ))
				{
					System.Console.Out.Write("--extracting file \"" + movie.Name + "\"");

					string outputMoviePath = Path.Combine(OutputMovieFolder, movie.Name + OutputMovieExtension);

					/// Seeks to the beginning of the movie data, if needed.
					if(inputStream.Position != movie.Offset.ToDecimal64())
					{
						inputStream.Seek(movie.Offset.ToDecimal64(), SeekOrigin.Begin);
					}

					using(FileStream outputStream = new FileStream(outputMoviePath, FileMode.Create, FileAccess.Write))
					{
						long amountDataRead = 0L;
						long amountDataLeft = movie.Length.ToDecimal64();
						
						long totalData = movie.Length.ToDecimal64();
						long currentDataSum = 0L;
						
						while(amountDataLeft > 0L)
						{
							/// Reads a chunk of movie data.
							if(amountDataLeft >= ReadBufferSize)
							{
								amountDataRead = inputStream.Read(byteArray, 0, ReadBufferSize);
							}
							else
							{
								amountDataRead = inputStream.Read(byteArray, 0, (int) amountDataLeft);
							}

							if(amountDataRead == 0L)
							{
								break;
							}

							/// Writes the read chunk to the output file.
							outputStream.Write(byteArray, 0, (int) amountDataRead);

							amountDataLeft -= amountDataRead;
							currentDataSum += amountDataRead;

							System.Console.Out.Write('\r');
							System.Console.Out.Write("--extracting file \"" + movie.Name + "\": " + ((currentDataSum / (double) totalData) * 100).ToString("F2", System.Globalization.CultureInfo.InvariantCulture) + "%");

						}

						/// Checks if there are missing data on the container.
						if(amountDataLeft > 0L)
						{
							System.Console.Out.Write(" EOF!");
							System.Console.Out.WriteLine();
							errorCount++;
							continue;
						}

						System.Console.Out.Write('\r');
						System.Console.Out.Write("--extracting file \"" + movie.Name + "\": done    ");
						System.Console.Out.WriteLine();

					} /// End of output stream.

				}

			} /// End of input stream.

		}

		System.Console.Out.WriteLine("Extraction completed with " + errorCount.ToString() + " errors.");

	}

}

} /// End of namespace.
