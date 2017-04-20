using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Net;
using System.Web;

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

		public static int Main (string[] args)
		{
			if (args.Length > 1) {
				Console.WriteLine ("Too much command line args.");
				return -1;
			} else if (args.Length < 1) {
				Console.WriteLine ("Too few command line args.");
				Console.WriteLine (args.Length.ToString());
				return -1;
			}
			var journal = args[0];

			string startingPoint = sites [1];
			WebClient client = new WebClient ();
			client.Encoding = Encoding.GetEncoding (
				SearchEnc.SearchEncoding(startingPoint));
			Array links = novtexParser (sites[3], client);;
			string user = @"server=localhost;userid=yurov;password=Heckbr9573175;database=macDuck;CharSet=utf8;";
			MySqlConnection conn = new MySqlConnection(user);
			conn.Open();
			if (journal == "jitnsu") {
				links = jitnsuParser (sites [1] + "index.php?+ru", client);
			} else if (journal == "jitcs") {
			links = jitcsIsaParser (sites[0], client);
			} else if (journal == "isa") {
				links = jitcsIsaParser (sites[8] + "proceedings", client);
			} else if (journal == "ubsMtas") {
				links = ubsMtasParser (sites[6] + "archive", client);
			} else if (journal == "svJournal") {
				links = svJournalParser (sites[2], client);
			/*} else if (journal == "novtex") {
				links = novtexParser (sites[3], client);*/
			} else {
				links = aidtParser(sites[4], client);
			}
			foreach(var item in links)
			{
				Console.WriteLine(item.ToString());
			}
			return 0;
		}


		/*
		 * WIP: looking only last ubs.mtas' issue for
		 * TODO: Refactoring
		 */

		public static Array ubsMtasParser(string url, WebClient client)
		{
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

		public static Array jitcsIsaParser(string url, WebClient client)
		{
			var htmlNode = new HtmlDocument ();
			htmlNode.LoadHtml (client.DownloadString (url));
			var documentNode = htmlNode.DocumentNode;
			var trNode = documentNode
				.SelectNodes ("//tr[@class='leftmenuarticles']") [0]
				.SelectSingleNode ("td/div");
			var aNode = trNode.SelectSingleNode ("a");
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


		/*
		 * WIP: looking only last jitnsu's issue for
		 * TODO: Need refactoring
		 */

		public static Array jitnsuParser(string url, WebClient client)
		{
			var htmlNode = new HtmlDocument ();
			htmlNode.LoadHtml (client.DownloadString (url));
			var documentNode = htmlNode.DocumentNode;
			var lastIssue1 = documentNode
				.SelectNodes ("//dd/a")
				.Where (node => node.InnerText == "Последний выпуск");
			var lastIssue = documentNode
				.SelectNodes ("//dd/a")
				.Where (node => node.InnerText == "Последний выпуск")
				.Select(node => sites[1] + node.Attributes["href"].Value.ToString())
				.First();
			htmlNode.LoadHtml (client.DownloadString (lastIssue));
			documentNode = htmlNode.DocumentNode;
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

		public static Array svJournalParser(string url, WebClient client)
		{
			var htmlNode = new HtmlDocument ();
			htmlNode.LoadHtml (client.DownloadString (url + "issues.php?lang=ru"));
			var documentNode = htmlNode.DocumentNode;
			var lastIssue = documentNode
				.SelectNodes ("//tr[2]/td[@class='nr'and last()]/a")
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
			return articles;
		}


		/*
		 * WIP: looking only last novtex's issue for
		 * TODO: Need refactoring
		 */

		public static Array novtexParser(string url, WebClient client)
		{
			var htmlNode = new HtmlDocument ();
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

		public static Array aidtParser(string url, WebClient client)
		{
			var htmlNode = new HtmlDocument ();
			htmlNode.LoadHtml (client.DownloadString (url + "index.php?lang=ru"));
			var documentNode = htmlNode.DocumentNode;
			var lastIssue = url + documentNode.SelectSingleNode ("//div[@id='avatar-right']//li[1]/a")
				.GetAttributeValue("href", "Can't parse");
			htmlNode.LoadHtml (client.DownloadString (lastIssue));
			documentNode = htmlNode.DocumentNode;
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

