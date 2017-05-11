using System;
using System.Collections.Generic;
using System.Data.Common;
using System.IO;
using System.Linq;
using System.Net;
using System.Web;
using System.Data.SQLite;

using MySql.Data;
using MySql.Data.MySqlClient;

using HtmlAgilityPack;

using Encoding = System.Text.Encoding;

namespace sona
{
	using Date = Tuple<int, int>;
	class MainClass
	{
		public static readonly string[] sites =
		{
			"http://jitcs.ru/",			// WIP: looking only last magazine issue for
			"http://www.jit.nsu.ru/",	// WIP: looking only last magazine issue for | /index.php?+ru -- for RU-lang
			"http://sv-journal.org/",	// WIP: looking only last magazine issue for | /issues.php?lang=ru -- for RU-lang
			"http://www.novtex.ru/",	// WIP: looking only last magazine issue for | /IT/newissue.htm
			"http://aidt.ru/",			// WIP: looking only last magazine issue for | /index.php?lang=ru for RU-lang
			"http://ipiran.ru/",
			"http://ubs.mtas.ru/",		// WIP: looking only last magazine issue for | /archive
			"http://bijournal.hse.ru/",
			"http://www.isa.ru/"		// WIP: looking only last magazine issue for | /proceedings
		};

		public enum WebResource {
			JITCS,
			JITNSU,
			SVJOURNAL,
			NOVTEX,
			AIDT,
			IPIRAN,
			UBSMTAS,
			BIJOURNAL,
			ISA
		};

		public class Issue {
			private int number;
			public int Number { get; set; }

			private int year;
			public int Year { get; set; }
		};

		public interface ISource {
			void Parse();
		}

		public class Jitcs : ISource {
			public Jitcs() { }

			public int Issue { get; set; }

			public void Parse() {
				WebClient client = new WebClient ();
				client.Encoding = Encoding.GetEncoding (
					SearchEnc.SearchEncoding(url));
				var htmlNode = new HtmlDocument ();
				htmlNode.LoadHtml (client.DownloadString (url));
				var documentNode = htmlNode.DocumentNode;
				var trNode = documentNode
					.SelectNodes ("//tr[@class='leftmenuarticles']") [0]
					.SelectSingleNode ("td/div");
				var aNode = trNode.SelectSingleNode ("a");
				date = Tuple.Create(Convert.ToInt32 (aNode.InnerText.Replace (" / ", " ").Split (' ') [0]),
						Convert.ToInt32 (aNode.InnerText.Replace(" / ", " ").Split(' ')[1]));
				var link = aNode.Attributes ["href"] != null
					? HttpUtility.HtmlDecode (aNode.Attributes ["href"].Value.ToString ())
					: "Can't parse";
				htmlNode.LoadHtml (client.DownloadString (link));
				documentNode = htmlNode.DocumentNode;
				var trArtNodes = documentNode
					.SelectNodes ("//tr[@class='leftmenuarticles']");
				List<string> articlesList = new List<string>();
				articlesList.Add (link);
				foreach (var trArtNode in trArtNodes.Skip(1)) {
					var aArtNodes = trArtNode
						.SelectNodes ("td/div/a");
					foreach (var aArtNode in aArtNodes) {
						articlesList.Add(aArtNode.Attributes ["href"] != null
							? HttpUtility.HtmlDecode (aArtNode.Attributes ["href"].Value.ToString ())
							: "Can't parse");
					}
				}
				articles = articlesList.ToArray();
			}

			private string url = "http://jitcs.ru/";
			private Date date;
			private Array articles;
		}

		public class JitNsu : ISource {
			public JitNsu() { }

			public int Issue { get; set; }

			public void Parse() {
				WebClient client = new WebClient ();
				client.Encoding = Encoding.GetEncoding (
					SearchEnc.SearchEncoding(url));
				var htmlNode = new HtmlDocument ();
				htmlNode.LoadHtml (client.DownloadString (url));
				var documentNode = htmlNode.DocumentNode;
				var lastIssue = documentNode
					.SelectNodes ("//dd/a")
					.Where (node => node.InnerText == "Последний выпуск")
					.Select(node => sites[1] + node.Attributes["href"].Value.ToString())
					.First();
				htmlNode.LoadHtml (client.DownloadString (lastIssue));
				documentNode = htmlNode.DocumentNode;
				date = Tuple.Create(Convert.ToInt32(documentNode
						.SelectNodes("//body/p")[0]
						.InnerText
						.Split('№')[1]
						.Replace("(", string.Empty)
						.Replace(")", string.Empty)
						.Substring(1)
						.Split(' ')[0]),
					Convert.ToInt32(documentNode
						.SelectNodes("//body/p")[0]
						.InnerText
						.Split('№')[1]
						.Replace("(", string.Empty)
						.Replace(")", string.Empty)
						.Substring(1)
						.Split(' ')[1]));
				var aNode = documentNode
					.SelectNodes ("//dd/a");
				articles = aNode
					.Take(aNode.Count - 2)
					.Select(node => node.Attributes ["href"] != null
						? HttpUtility.HtmlDecode (sites[1] + node.Attributes ["href"].Value.ToString ())
						: "Can't parse")
					.ToArray();
			}

			private string url = "http://jit.nsu.ru/index.php?+ru";
			private Date date;
			private Array articles;
		}

		public class Isa : ISource {
			public Isa() { }

			public int Issue { get; set; }

			public void Parse() {
				WebClient client = new WebClient ();
				client.Encoding = Encoding.GetEncoding (
					SearchEnc.SearchEncoding(url));
				var htmlNode = new HtmlDocument ();
				htmlNode.LoadHtml (client.DownloadString (url));
				var documentNode = htmlNode.DocumentNode;
				var trNode = documentNode
					.SelectNodes ("//tr[@class='leftmenuarticles']") [0]
					.SelectSingleNode ("td/div");
				var aNode = trNode.SelectSingleNode ("a");
				date = Tuple.Create(Convert.ToInt32 (aNode.InnerText.Split (new Char [] {' ', '-'}).ToList()[1]), -1);
				var link = aNode.Attributes ["href"] != null
					? HttpUtility.HtmlDecode (aNode.Attributes ["href"].Value.ToString ())
					: "Can't parse";
				htmlNode.LoadHtml (client.DownloadString (link));
				documentNode = htmlNode.DocumentNode;
				var trArtNodes = documentNode
					.SelectNodes ("//tr[@class='leftmenuarticles']");
				List<string> articlesList = new List<string>();
				articlesList.Add (link);
				foreach (var trArtNode in trArtNodes.Skip(1)) {
					var aArtNodes = trArtNode
						.SelectNodes ("td/div/a");
					foreach (var aArtNode in aArtNodes) {
						articlesList.Add(aArtNode.Attributes ["href"] != null
							? HttpUtility.HtmlDecode (aArtNode.Attributes ["href"].Value.ToString ())
							: "Can't parse");
					}
				}
				articles = articlesList.ToArray ();
			}

			private string url = "http://isa.ru/proceedings";
			private Date date;
			private Array articles;
		}

		public class UbsMtas : ISource {
			public UbsMtas() { }

			public int Issue { get; set; }

			public void Parse() {
				WebClient client = new WebClient ();
				client.Encoding = Encoding.GetEncoding (
					SearchEnc.SearchEncoding(url));
				var htmlNode = new HtmlDocument ();
				while (!String.IsNullOrEmpty (url)) {
					htmlNode.LoadHtml (client.DownloadString (url));
					var documentNode = htmlNode.DocumentNode;
					try {
						var ulNode = documentNode
							.SelectNodes ("//ul")
							.Select (node => node.LastChild);
						var aNode = ulNode.ElementAt (0).SelectSingleNode ("a");
						var linkNode = aNode.Attributes ["href"] != null
							? HttpUtility.HtmlDecode (sites [6] + aNode.Attributes ["href"].Value.ToString ())
							: "Can't parse";
						date = Tuple.Create(Convert.ToInt32 (aNode.InnerText.Split (new Char [] {' ', '&'}).ToList()[1]), -1);
						url = linkNode;
					} catch (ArgumentNullException) {
						var articlesUrl = documentNode
							.SelectNodes ("//td[not(@*)]/a")
							.Select (node => node.Attributes ["href"].Value != null
								? HttpUtility.HtmlDecode (sites [6] + node.Attributes ["href"].Value)
								: "Can't parse")
							.ToArray ();
						articles = articlesUrl;
					}
				}
				articles = new string[] {};
			}

			private string url = "http://ubs.mtas.ru/archive";
			private Date date;
			private Array articles;
		}

		public class SvJournal : ISource {
			public SvJournal() { }

			public int Issue { get; set; }

			public void Parse() {
				WebClient client = new WebClient ();
				client.Encoding = Encoding.GetEncoding (
					SearchEnc.SearchEncoding(url));
				var htmlNode = new HtmlDocument ();
				htmlNode.LoadHtml (client.DownloadString (url));
				var documentNode = htmlNode.DocumentNode;
				var lastIssue = documentNode
					.SelectNodes ("//tr[2]/td[@class='nr' and last()]/a")
					.Select (node => node.Attributes ["href"] != null
						? HttpUtility.HtmlDecode (url + node.Attributes ["href"].Value.ToString ())
						: "Can't parse").ElementAt(0);
				var uriAddress = new Uri (lastIssue);
				var issueDate = uriAddress.AbsolutePath.Split('/')[1] + '/';
				htmlNode.LoadHtml (client.DownloadString (lastIssue));
				documentNode = htmlNode.DocumentNode;
				var articlesArray = documentNode
					.SelectNodes ("//tr/td[@class='pub_pp']/a")
					.Select (node => node.Attributes ["href"] != null
						? HttpUtility.HtmlDecode (url + issueDate + node.Attributes ["href"].Value.ToString ())
						: "Can't parse")
					.ToArray();
				date = Tuple.Create(Convert.ToInt32 (articlesArray [0]
						.Split ('/') [3]
						.Replace ('-', ' ').Split(' ')[0]),
					Convert.ToInt32 (articlesArray [0]
						.Split ('/') [3]
						.Replace ('-', ' ').Split(' ')[1]));
				articles = articlesArray;
			}

			private string url = "http://sv-journal.org/issues.php?lang=ru";
			private Date date;
			private Array articles;
		}

		public class Aidt : ISource {
			public Aidt() { }

			public int Issue { get; set; }

			public void Parse() {
				WebClient client = new WebClient ();
				client.Encoding = Encoding.GetEncoding (
					SearchEnc.SearchEncoding(url));
				var htmlNode = new HtmlDocument ();
				htmlNode.LoadHtml (client.DownloadString (url));
				var documentNode = htmlNode.DocumentNode;
				var lastIssue = url + documentNode.SelectSingleNode ("//div[@id='avatar-right']//li[1]/a")
					.GetAttributeValue("href", "Can't parse");
				htmlNode.LoadHtml (client.DownloadString (lastIssue));
				documentNode = htmlNode.DocumentNode;
				date = Tuple.Create(Convert.ToInt32 (documentNode
						.SelectNodes ("//div[@class='category-list']/h2/span") [0]
						.InnerText
						.Replace (" / ", " ").Split(' ')[0]),
					Convert.ToInt32 (documentNode
						.SelectNodes ("//div[@class='category-list']/h2/span") [0]
						.InnerText
						.Replace (" / ", " ").Split(' ')[1]));
				articles = documentNode
					.SelectNodes ("//div[@class='sectionlist']/ul/li/a")
					.Select( node => node.Attributes ["href"] != null
						? HttpUtility.HtmlDecode (url + node.Attributes ["href"].Value.ToString ())
						: "Can't parse")
					.ToArray();
			}

			private string url = "http://aidt.ru/index.php?lang=ru";
			private Date date;
			private Array articles;
		}

		public class BiJournal : ISource {
			public BiJournal() { }

			public int Issue { get; set; }

			public void Parse() {
				WebClient client = new WebClient ();
				client.Encoding = Encoding.GetEncoding (
					SearchEnc.SearchEncoding(url));
				var htmlNode = new HtmlDocument ();
				htmlNode.LoadHtml (client.DownloadString (url));
				var documentNode = htmlNode.DocumentNode;
				var lastIssue = documentNode.SelectSingleNode ("//div[@class='journal']//div[@class='new-num']/a")
					.GetAttributeValue("href", "Can't parse");
				htmlNode.LoadHtml (client.DownloadString (lastIssue));
				documentNode = htmlNode.DocumentNode;
				date = Tuple.Create(Convert.ToInt32 (new Uri(lastIssue).AbsolutePath.Substring(1).Split('-')[0]),
					Convert.ToInt32 (new Uri(lastIssue).AbsolutePath.Substring(1).Split('-')[2].Split('%')[0]));
				articles = documentNode
					.SelectNodes ("//table[@class='link']//div[@class='link']/a")
					.Select( node => node.Attributes ["href"] != null
						? HttpUtility.HtmlDecode (node.Attributes ["href"].Value.ToString ())
						: "Can't parse")
					.ToArray();
			}

			private string url = "http://bijournal.hse.ru";
			private Date date;
			private Array articles;
		}

		public class Factory {
			public ISource CreateSource (WebResource resource)
			{
				switch (resource)
				{
				case WebResource.JITCS:
					return new Jitcs();
				case WebResource.JITNSU:
					return new JitNsu();
				case WebResource.AIDT:
					return new Aidt();
				case WebResource.SVJOURNAL:
					return new SvJournal();
				case WebResource.UBSMTAS:
					return new UbsMtas();
				case WebResource.BIJOURNAL:
					return new BiJournal();
				case WebResource.IPIRAN:
					throw new NotImplementedException ("Method no implemented. Please try again later.");
				default:
					throw new ArgumentOutOfRangeException();
				}
			}
		}

		public static int Main (string[] args)
		{
			/*if (args.Length > 2) {
				Console.WriteLine ("Too much command line args.");
				return -1;
			} else if (args.Length < 1) {
				Console.WriteLine ("Too few command line args.");
				return -1;
			}
			var journal = args[0];*/
			var journal = "http://bijournal.hse.ru";

			Factory factory = new Factory ();
			var source = factory.CreateSource (toWebResource(journal));

			var lastIssueDate = getDateFromDb (journal);
			source.Parse ();

			/*foreach(var item in source) {
				Console.WriteLine(item.ToString());
			}*/
			return 0;
		}

		public static WebResource toWebResource(string url) {
			Uri uri = new Uri (url);
			switch (uri.Host)
			{
				case "jitcs.ru":
					return WebResource.JITCS;
				case "jit.nsu.ru":
					return WebResource.JITNSU;
				case "aidt.ru":
					return WebResource.AIDT;
				case "sv-journal.org":
					return WebResource.SVJOURNAL;
				case "ipiran.ru":
					return WebResource.IPIRAN;
					case "isa.ru":
					return WebResource.ISA;
				case "ubs.mtas.ru":
					return WebResource.UBSMTAS;
				case "bijournal.hse.ru":
					return WebResource.BIJOURNAL;
				default:
					throw new ArgumentOutOfRangeException();
			}
		}

		public static Tuple<int, int> getDateFromDb(string journal) {
			return Tuple.Create(1, 2017);
			string cs = @"server=localhost;userid=sona;password=123456;database=macDuck";
			MySqlConnection connection = null;
			try {
				connection = new MySqlConnection(cs);
				connection.Open();
				var cmd = connection.CreateCommand();
				cmd.CommandText = "select id from journals where name = @name";
				cmd.Parameters.AddWithValue("@name", journal.ToString());
				string id = "";
				using (var reader = cmd.ExecuteReader()) {
					if (reader.Read()) {
						id = reader[0].ToString();
					}
				}
				cmd.CommandText = "select month, year from last_issue where journal = @journal";
				cmd.Parameters.AddWithValue("@journal", id);
				using (var reader = cmd.ExecuteReader()) {
					if (reader.Read()) {
						if (connection != null) {
							connection.Close();
						}
						return Tuple.Create(Convert.ToInt32(reader[0]), Convert.ToInt32(reader[1]));
					}
				}
				return Tuple.Create(-1, -1);
			} catch (MySqlException ex)	{
				Console.WriteLine("Error: {0}",  ex.ToString());
				return Tuple.Create(-1, -1);
			}
		}

	}
}

