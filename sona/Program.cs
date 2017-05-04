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
			private int _number;
			public int Number { get; set; }

			private int _year;
			public int Year { get; set; }
		};

		public class Journal {
			public Journal(WebResource resource)
			{
				_name = resource;
				_issue.Number = 0;
				_issue.Year = 0;
			}

			public Journal(WebResource resource, Issue issue)
			{
				_name = resource;
				_issue.Number = issue.Number;
				_issue.Year = issue.Year;
			}

			public WebResource Name { get; set; }
			public int Issue { get; set; }

			public int Number() { return _issue.Number; }
			public void Number(int number) { _issue.Number = number; }

			public int Year() { return _issue.Year; }
			public void Year(int year) { _issue.Year = year; }

			//public Array Parse ();

			public override string ToString()
			{
				switch (_name)
				{
					case WebResource.JITCS:
						return "jitcs";
					case WebResource.JITNSU:
						return "jitnsu";
					case WebResource.AIDT:
						return "aidt";
					case WebResource.BIJOURNAL:
						return "bijournal";
					case WebResource.IPIRAN:
						return "ipiran";
					case WebResource.ISA:
						return "isa";
					case WebResource.NOVTEX:
						return "novtex";
					case WebResource.SVJOURNAL:
						return "svJournal";
					case WebResource.UBSMTAS:
						return "ubsMtas";
					default:
						throw new ArgumentOutOfRangeException();
				}
			}

			private Issue _issue;
			private WebResource _name;
		};

		public static int Main (string[] args)
		{
			/*if (args.Length > 2) {
				Console.WriteLine ("Too much command line args.");
				return -1;
			} else if (args.Length < 1) {
				Console.WriteLine ("Too few command line args.");
				return -1;
			}*
			var journal = args[0];
			string directArticleLink = args[1];*/
			Journal journal = new Journal(WebResource.AIDT);
			//var journal = "aidt";
			string directArticleLink = "";

			WebClient client = new WebClient ();
			client.Encoding = Encoding.GetEncoding (
				SearchEnc.SearchEncoding(sites[1]));
			Array links;
			string number = "";
			var lastIssueDate = getDateFromDb (journal);
			switch (journal.Name)
			{
			case WebResource.JITNSU:
				links = jitnsuParser (sites [1] + "index.php?+ru", client, ref number, directArticleLink);
				if (lastIssueDate.Item1 == Convert.ToInt32 (number.Split (' ') [0]) || lastIssueDate.Item2 == Convert.ToInt32 (number.Split (' ') [1])) {
					return -1;
				}
				File.WriteAllText ("jitnsu.txt", number);
				break;
			case WebResource.JITCS:
				links = jitcsParser (sites [0], client, ref number, directArticleLink);
				if (lastIssueDate.Item1 == Convert.ToInt32 (number.Split (' ') [0]) || lastIssueDate.Item2 == Convert.ToInt32 (number.Split (' ') [1])) {
					return -1;
				}
				File.WriteAllText ("jitcs.txt", number);
				break;
			case WebResource.ISA:
				links = isaParser (sites [8] + "proceedings", client, ref number, directArticleLink);
				if (lastIssueDate.Item1 == Convert.ToInt32 (number)) {
					return -1;
				}
				File.WriteAllText ("isa.txt", number);
				break;
			case WebResource.UBSMTAS:
				links = ubsMtasParser (sites [6] + "archive", client, ref number, directArticleLink);
				if (lastIssueDate.Item1 == Convert.ToInt32 (number)) {
					return -1;
				}
				File.WriteAllText ("ubsMtas.txt", number);
				break;
			case WebResource.SVJOURNAL:
				links = svJournalParser (sites [2], client, ref number, directArticleLink);
				if (lastIssueDate.Item1 == Convert.ToInt32 (number.Split (' ') [0]) || lastIssueDate.Item2 == Convert.ToInt32 (number.Split (' ') [1])) {
					return -1;
				}
				File.WriteAllText ("svJournal.txt", number);
				break;
			/*
			case WebResource.NOVTEX:
				links = novtexParser (sites[3], client);
				File.WriteAllText("novtex.txt", createText);
				break
			*/
			case WebResource.AIDT:
				links = aidtParser (sites [4], client, ref number, directArticleLink);
				if (lastIssueDate.Item1 == Convert.ToInt32 (number.Split (' ') [0]) || lastIssueDate.Item2 == Convert.ToInt32 (number.Split (' ') [1])) {
					return -1;
				}
				File.WriteAllText ("aidt.txt", number);
				break;
			case WebResource.BIJOURNAL:
				throw new NotImplementedException ("Method no implemented. Please try again later.");
			default:
				throw new ArgumentOutOfRangeException();
			}
			foreach(var item in links) {
				Console.WriteLine(item.ToString());
			}
			return 0;
		}

		public static Tuple<int, int> getDateFromDb(Journal journal) {
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
				return Tuple.Create(0, 0);
			} catch (MySqlException ex)	{
				Console.WriteLine("Error: {0}",  ex.ToString());
				return Tuple.Create(0, 0);
			}
		}


		/*
		 * WIP: looking only last ubs.mtas' issue for
		 * TODO: Refactoring
		 */

		public static Array ubsMtasParser(string url, WebClient client, ref string issueNumber, string directArticleLink)
		{
			var htmlNode = new HtmlDocument ();
			while (!String.IsNullOrEmpty (url)) {
				if (string.IsNullOrEmpty(directArticleLink)) {
					htmlNode.LoadHtml (client.DownloadString (url));
				} else {
					htmlNode.LoadHtml (directArticleLink);
				}
				var documentNode = htmlNode.DocumentNode;
				try {
					if (!string.IsNullOrEmpty(directArticleLink)) {
						throw new ArgumentNullException();
					}
					var ulNode = documentNode
					.SelectNodes ("//ul")
					.Select (node => node.LastChild);
					var aNode = ulNode.ElementAt (0).SelectSingleNode ("a");
					var linkNode = aNode.Attributes ["href"] != null
						? HttpUtility.HtmlDecode (sites [6] + aNode.Attributes ["href"].Value.ToString ())
						: "Can't parse";
					issueNumber = aNode.InnerText.Split (new Char [] {' ', '&'}).ToList()[1];
					url = linkNode;
				} catch (ArgumentNullException) {
					var articlesUrl = documentNode
					.SelectNodes ("//td[not(@*)]/a")
					.Select (node => node.Attributes ["href"].Value != null
						? HttpUtility.HtmlDecode (sites [6] + node.Attributes ["href"].Value)
						: "Can't parse")
					.ToArray ();
					return articlesUrl;
				}
			}
			return new string[] {};
		}


		/*
		 * WIP: looking only last jitcs' and isa's issue for
		 * TODO: Refactoring
		 */

		public static Array isaParser(string url, WebClient client, ref string issueNumber, string directArticleLink)
		{
			var htmlNode = new HtmlDocument ();
			if (string.IsNullOrEmpty(directArticleLink)) {
				htmlNode.LoadHtml (client.DownloadString (url));
			} else {
				htmlNode.LoadHtml (directArticleLink);
			}
			var documentNode = htmlNode.DocumentNode;
			var trNode = documentNode
				.SelectNodes ("//tr[@class='leftmenuarticles']") [0]
				.SelectSingleNode ("td/div");
			var aNode = trNode.SelectSingleNode ("a");
			issueNumber = aNode.InnerText.Split (new Char [] {' ', '-'}).ToList()[1];
			var link = aNode.Attributes ["href"] != null
				? HttpUtility.HtmlDecode (aNode.Attributes ["href"].Value.ToString ())
				: "Can't parse";
			htmlNode.LoadHtml (client.DownloadString (link));
			documentNode = htmlNode.DocumentNode;
			var trArtNodes = documentNode
				.SelectNodes ("//tr[@class='leftmenuarticles']");
			//var articles = new string[trArtNode.Count];
			List<string> articles = new List<string>();
			articles.Add (link);
			foreach (var trArtNode in trArtNodes.Skip(1)) {
				var aArtNodes = trArtNode
					.SelectNodes ("td/div/a");
				foreach (var aArtNode in aArtNodes) {
					articles.Add(aArtNode.Attributes ["href"] != null
						? HttpUtility.HtmlDecode (aArtNode.Attributes ["href"].Value.ToString ())
						: "Can't parse");
				}
			}
			return articles.ToArray ();
		}

		public static Array jitcsParser(string url, WebClient client, ref string issueNumber, string directArticleLink)
		{
			var htmlNode = new HtmlDocument ();
			if (string.IsNullOrEmpty(directArticleLink)) {
				htmlNode.LoadHtml (client.DownloadString (url));
			} else {
				htmlNode.LoadHtml (directArticleLink);
			}
			var documentNode = htmlNode.DocumentNode;
			var trNode = documentNode
				.SelectNodes ("//tr[@class='leftmenuarticles']") [0]
				.SelectSingleNode ("td/div");
			var aNode = trNode.SelectSingleNode ("a");
			issueNumber = aNode.InnerText.Replace(" / ", " ");
			var link = aNode.Attributes ["href"] != null
				? HttpUtility.HtmlDecode (aNode.Attributes ["href"].Value.ToString ())
				: "Can't parse";
			htmlNode.LoadHtml (client.DownloadString (link));
			documentNode = htmlNode.DocumentNode;
			var trArtNodes = documentNode
				.SelectNodes ("//tr[@class='leftmenuarticles']");
			List<string> articles = new List<string>();
			articles.Add (link);
			foreach (var trArtNode in trArtNodes.Skip(1)) {
				var aArtNodes = trArtNode
					.SelectNodes ("td/div/a");
				foreach (var aArtNode in aArtNodes) {
					articles.Add(aArtNode.Attributes ["href"] != null
						? HttpUtility.HtmlDecode (aArtNode.Attributes ["href"].Value.ToString ())
						: "Can't parse");
				}
			}
			return articles.ToArray ();
		}


		/*
		 * WIP: looking only last jitnsu's issue for
		 * TODO: Need refactoring
		 */

		public static Array jitnsuParser(string url, WebClient client, ref string issueNumber, string directArticleLink)
		{
			var htmlNode = new HtmlDocument ();
			if (string.IsNullOrEmpty(directArticleLink)) {
				htmlNode.LoadHtml (client.DownloadString (url));
			} else {
				htmlNode.LoadHtml (directArticleLink);
			}
			var documentNode = htmlNode.DocumentNode;
			var lastIssue = documentNode
				.SelectNodes ("//dd/a")
				.Where (node => node.InnerText == "Последний выпуск")
				.Select(node => sites[1] + node.Attributes["href"].Value.ToString())
				.First();
			htmlNode.LoadHtml (client.DownloadString (lastIssue));
			documentNode = htmlNode.DocumentNode;
			issueNumber = documentNode
				.SelectNodes("//body/p")[0]
				.InnerText
				.Split('№')[1]
				.Replace("(", string.Empty)
				.Replace(")", string.Empty)
				.Substring(1);
			var aNode = documentNode
				.SelectNodes ("//dd/a");
			var articles = aNode
				.Take(aNode.Count - 2)
				.Select(node => node.Attributes ["href"] != null
					? HttpUtility.HtmlDecode (sites[1] + node.Attributes ["href"].Value.ToString ())
					: "Can't parse");
			return articles.ToArray ();
		}


		/*
		 * WIP: looking only last sv-journal's issue for
		 * TODO: Need refactoring
		 */

		public static Array svJournalParser(string url, WebClient client, ref string issueNumber, string directArticleLink)
		{
			var htmlNode = new HtmlDocument ();
			if (string.IsNullOrEmpty(directArticleLink)) {
				htmlNode.LoadHtml (client.DownloadString (url + "issues.php?lang=ru"));
			} else {
				htmlNode.LoadHtml (directArticleLink);
			}
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
			var articles = documentNode
				.SelectNodes ("//tr/td[@class='pub_pp']/a")
				.Select (node => node.Attributes ["href"] != null
					? HttpUtility.HtmlDecode (url + issueDate + node.Attributes ["href"].Value.ToString ())
					: "Can't parse")
				.ToArray();
			issueNumber = articles [0]
				.Split ('/') [3]
				.Replace ('-', ' ');
			return articles;
		}


		/*
		 * WIP: looking only last novtex's issue for
		 * TODO: Need refactoring
		 */

		public static Array novtexParser(string url, WebClient client)
		{
			var htmlNode = new HtmlDocument ();
			string issueNumber = "";
			htmlNode.LoadHtml (client.DownloadString (url + "IT/newissue.htm"));
			var documentNode = htmlNode.DocumentNode;
			var articlesOnOnePage = url + documentNode.SelectNodes ("//td[@class='itmain']/p[@class='itmain']/a") [0]
				.GetAttributeValue("href", "Can't parse");
			return articlesOnOnePage.ToArray ();
		}


		/*
		 * WIP: looking only last aidt's issue for
		 * TODO: Need refactoring
		 * 		 Latter issue not always really latter
		 */

		public static Array aidtParser(string url, WebClient client, ref string issueNumber, string directArticleLink)
		{
			var htmlNode = new HtmlDocument ();
			if (string.IsNullOrEmpty(directArticleLink)) {
				htmlNode.LoadHtml (client.DownloadString (url + "index.php?lang=ru"));
			} else {
				htmlNode.LoadHtml (directArticleLink);
			}
			var documentNode = htmlNode.DocumentNode;
			var lastIssue = url + documentNode.SelectSingleNode ("//div[@id='avatar-right']//li[1]/a")
				.GetAttributeValue("href", "Can't parse");
			htmlNode.LoadHtml (client.DownloadString (lastIssue));
			documentNode = htmlNode.DocumentNode;
			issueNumber = documentNode
				.SelectNodes ("//div[@class='category-list']/h2/span") [0]
				.InnerText
				.Replace (" / ", " ");
			var articles = documentNode
				.SelectNodes ("//div[@class='sectionlist']/ul/li/a")
				.Select( node => node.Attributes ["href"] != null
					? HttpUtility.HtmlDecode (url + node.Attributes ["href"].Value.ToString ())
					: "Can't parse")
				.ToArray();
			return articles;
		}
	}
}

