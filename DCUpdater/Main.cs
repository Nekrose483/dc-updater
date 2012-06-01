using System;
using Gtk;
using System.Net;


namespace DCUpdater
{
	class MainClass
	{
		public static void Main (string[] args)
		{
			MainWindow win;
			Download download;
			
			Application.Init ();
			win = new MainWindow ();
			download = new Download (win);
			
			win.Show ();
			download.begin ();
			
			//MainWindow win = new MainWindow ();
			//win.Show ();
			Application.Run ();
		}
	}
}
