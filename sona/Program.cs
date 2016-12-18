using System;
using System.Net;

using Encoding = System.Text.Encoding;

namespace sona
{
	class MainClass
	{
		public static readonly string[] sites = {"http://jitcs.ru",
			"http://www.jit.nsu.ru", "http://sv-journal.org",
			"http://www.novtex.ru", "http://aidt.ru",
			"http://ipiran.ru", "http://ubs.mtas.ru",
			"http://bijournal.hse.ru", "http://www.isa.ru"};
		
		public static void Main (string[] args)
		{
			WebClient client = new WebClient ();
			client.Encoding = Encoding.GetEncoding("windows-1251");
			string reply = client.DownloadString (sites[6]);
		}
	}
}
