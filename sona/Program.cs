using System;
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
			{"http://jitcs.ru",
			"http://www.jit.nsu.ru",
			"http://sv-journal.org",
			"http://www.novtex.ru",
			"http://aidt.ru",
			"http://ipiran.ru",
			"http://ubs.mtas.ru",	// WIP: отслеживаются лишь статьи последнего номера журнала
			"http://bijournal.hse.ru",
			"http://www.isa.ru"};
		
		public static void Main (string[] args)
		{
			string startingPoint = sites [6] + "/archive";
			WebClient client = new WebClient ();
			client.Encoding = Encoding.GetEncoding (SearchEnc.SearchEncoding(startingPoint));
			var htmlNode = new HtmlDocument ();
			var url = startingPoint;
			while(!String.IsNullOrEmpty(url)) {
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
				}
				catch (ArgumentNullException) {
					var articleNode = documentNode
						.SelectNodes ("//td[not(@*)]/a")
						.Select (node => node.Attributes["href"].Value != null
							? HttpUtility.HtmlDecode(sites [6] + node.Attributes["href"].Value)
							: "Can't parse")
						.ToArray();
				}
			}

		}
	}
}
