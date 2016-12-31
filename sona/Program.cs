using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Web;

using HtmlAgilityPack;

using Encoding = System.Text.Encoding;

namespace sona
{
	class MainClass
	{
		public static readonly string[] sites =
		{
			"http://jitcs.ru",		// WIP: looking only last magazine issue for
			"http://www.jit.nsu.ru",
			"http://sv-journal.org",
			"http://www.novtex.ru",
			"http://aidt.ru",
			"http://ipiran.ru",
			"http://ubs.mtas.ru",	// WIP: looking only last magazine issue for | /archive
			"http://bijournal.hse.ru",
			"http://www.isa.ru"
		};
		
		public static void Main (string[] args)
		{
			string startingPoint = sites [0];
			WebClient client = new WebClient ();
			client.Encoding = Encoding.GetEncoding (
				SearchEnc.SearchEncoding(startingPoint));
			var url = startingPoint;
			jitcsParser (url, client);
			//ubsMtasParser (url, client);
		}


		/*
		 * WIP: looking only last magazine issue for
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
		 * WIP: looking only last magazine issue for
		 * TODO: Refactoring
		 */

		public static Array jitcsParser(string url, WebClient client)
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
	}
}
