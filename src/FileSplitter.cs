
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

//using Conrado;

public class FileSplitter {


	private const string hexMovieNameList = "0090";
	private const string hexMovieNameListSize = "0D40";

	private const string hexContainerNameList = "0DD0";
//	private const string hexContainerNameListSize = "005A";


	private bool isAmerica;
	private string inputFolder;
	private string outputFolder;
	private string inputVideoExtension;
	private string outputVideoExtension;


	public static void Main(string[] args) {

		if(args.Length < 1) {
			Console.Out.WriteLine("Usage: BinkSplitter movie_items_us.win32.wdb");
		}
		else {
			new FileSplitter().Split(args.FirstOrDefault());
		}

	}


	public FileSplitter() {

	}


	public void Split(string databaseFilePath) {


		if(Path.GetFileName(databaseFilePath).Contains("win32")) {
			inputVideoExtension = ".win32.wmp";
			outputVideoExtension = ".bk2";
		}
		else if(Path.GetFileName(databaseFilePath).Contains("ps3")) {
			inputVideoExtension = ".ps3.wmp";
			outputVideoExtension = ".pamf";
		}
		else if(Path.GetFileName(databaseFilePath).Contains("x360")) {
			inputVideoExtension = ".x360.wmp";
			outputVideoExtension = ".bik";
		}

		inputFolder = Path.GetDirectoryName(databaseFilePath);
		if(!string.IsNullOrEmpty(inputFolder))
			inputFolder = inputFolder.Replace("\\", "/") + "/";

		isAmerica = Path.GetFileName(databaseFilePath).Contains("us");
		outputFolder = isAmerica ? "extracted_us/" : "extracted_jp/";


		if(!File.Exists(databaseFilePath)) {
			Console.Error.WriteLine("File \"" + databaseFilePath + "\" not found");
			return;
		}

		List<Movie> movieList = new List<Movie>();
		byte[] byteArray = new byte[16];
		
		using(FileStream inputStream = new FileStream(databaseFilePath, FileMode.Open, FileAccess.Read)) {

			Console.Out.WriteLine("File \"" + databaseFilePath + "\" opened successfully");

			if(!inputStream.CanSeek || !inputStream.CanRead) {
				Console.Error.WriteLine("File cannot seek or read its content");
				return;
			}


			inputStream.Seek(hexMovieNameList.ToBase64(), SeekOrigin.Begin);
			
//			long amountDataRead;
			long amountDataLeft;

			amountDataLeft = hexMovieNameListSize.ToBase64();

			while(amountDataLeft > 0) {

				Movie movie = new Movie();

				amountDataLeft -= inputStream.Read(byteArray, 0, byteArray.Length);
				movie.name = byteArray.ToUTF8String();
				
				amountDataLeft -= inputStream.Read(byteArray, 0, byteArray.Length);
				movie.pos = byteArray.ToHexString(0, 4);
				movie.size = byteArray.ToHexString(4, 4);

				movieList.Add(movie);
			}

			for(int i = 0; i < movieList.Count; i++) {
				Movie movie = movieList[i];

				inputStream.Seek(movie.pos.ToBase64(), SeekOrigin.Begin);

				inputStream.Read(byteArray, 0, byteArray.Length);

				movie.size = byteArray.ToHexString(4, 4);
				movie.pos = byteArray.ToHexString(12, 4);

				inputStream.Seek(hexContainerNameList.ToBase64(), SeekOrigin.Begin);
				inputStream.Seek(byteArray.ToHexString(0, 4).ToBase64(), SeekOrigin.Current);
				inputStream.Read(byteArray, 0, 5);

				movie.container = byteArray.ToUTF8String(0, 5) + (isAmerica ? "_us" : "");
			}

		}

//		movieList.OrderBy( movie => movie.container ).ToList().ForEach( movie => File.AppendAllText("files.txt", movie.container + " - " + movie.name.PadRight(11) + " - " + movie.pos + " - " + movie.size + " (" + (movie.size.ToBase64() / 1024d / 1024d).ToString("F2", System.Globalization.CultureInfo.InvariantCulture) + " MB)" + Environment.NewLine) );

		byteArray = new byte[64 * 1024 * 1024];
		
		foreach(var container in movieList.GroupBy( movie => movie.container ).OrderBy( container => container.Key )) {

			string inputFilePath = inputFolder + container.Key + inputVideoExtension;

			Console.Out.WriteLine("Reading container \"" + inputFilePath + "\"");

			if(!File.Exists(inputFilePath)) {
				Console.Error.WriteLine("File \"" + inputFilePath + "\" not found");
				continue;
			}

			using(FileStream inputStream = File.OpenRead(inputFilePath)) {

				if(!inputStream.CanSeek || !inputStream.CanRead) {
					Console.Error.WriteLine("File cannot seek or read its content");
					continue;
				}

				long amountDataRead;
				long amountDataLeft;
				double totalData;
				double currentData;
				int count = 0;

				foreach(Movie movie in container.OrderBy( movie => movie.pos.ToBase64() )) {

					Console.Out.Write("--extracting file " + movie.name);

					string outputFilePath = outputFolder + movie.name + outputVideoExtension;

					if(!Directory.Exists(Path.GetDirectoryName(outputFilePath)))
						Directory.CreateDirectory(Path.GetDirectoryName(outputFilePath));

					inputStream.Seek(movie.pos.ToBase64(), SeekOrigin.Begin);
					
					amountDataRead = 0;
					amountDataLeft = movie.size.ToBase64();
					totalData = (double) amountDataLeft;
					currentData = 0d;
					count++;

					using(FileStream outputStream = new FileStream(outputFilePath, FileMode.Create, FileAccess.Write)) {

						while(amountDataLeft > 0) {

							if(amountDataLeft < byteArray.Length)
								amountDataRead = inputStream.Read(byteArray, 0, (int) amountDataLeft);
							else
								amountDataRead = inputStream.Read(byteArray, 0, byteArray.Length);

							if(amountDataRead == 0)
								break;

							outputStream.Write(byteArray, 0, (int) amountDataRead);

							amountDataLeft -= amountDataRead;
							currentData += amountDataRead;

							Console.Out.Write('\r');
							Console.Out.Write("--extracting file " + movie.name + ": " + ((currentData / totalData) * 100).ToString("F2", System.Globalization.CultureInfo.InvariantCulture) + "%");

						}

						if(amountDataLeft > 0) {
							Console.Out.WriteLine(" EOF");
						}
						else {
							Console.Out.Write('\r');
							Console.Out.WriteLine("--extracting file " + movie.name + ": 100%    ");
						}

					}

				}

			}

		}

	}

	
	[Serializable]
	private class Movie {
		public string name;
		public string container;
		public string pos;
		public string size;
	}


}
