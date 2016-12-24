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
			string starting_point = sites [6] + "/archive";
			WebClient client = new WebClient ();
			client.Encoding = Encoding.GetEncoding ("windows-1251");
			var html_node = new HtmlDocument ();
			var url = starting_point;
			while(!String.IsNullOrEmpty(url)) {
				html_node.LoadHtml (client.DownloadString (url));
				var document_node = html_node.DocumentNode;
				try {
					var ul_node = document_node
						.SelectNodes ("//ul")
						.Select (node => node.LastChild);
					var a_node = ul_node.ElementAt (0).SelectSingleNode ("a");
					var link_node = a_node.Attributes ["href"] != null
						? HttpUtility.HtmlDecode (sites [6] + a_node.Attributes ["href"].Value.ToString ())
						: "Can't parse";
					url = link_node;
				}
				catch (ArgumentNullException) {
					var article_node = document_node
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
