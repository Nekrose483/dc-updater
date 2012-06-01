using System;
using System.Net;
using System.Windows.Forms; //for application.startuppath
using System.IO; //for file routines
using System.Diagnostics;


namespace DCUpdater
{
	public class Download
	{
		double currentVersion = 0;
		double onlineVersion = 0;
		
		bool downloading = false;
		
		string version_filename;
		string version_remoteUri;
		
		string dc_filename;
		string dc_remoteUri;
		string dc_temporaryLocalFilename;
		
		MainWindow window;					//keep a copy of the mainwindow to update it
		
		WebClient versionWebClient;
		WebClient dcWebClient; 				//keeping these webclient separate
		
		public Download (MainWindow win_)
		{
			currentVersion = getCurrentVersion ();
			Console.WriteLine ("Found current version: " + currentVersion);
			downloading = false;
			version_filename = options.remotefilename_currentversionnumber;
			version_remoteUri = "http://" + options.serveraddress + "/";
			
			dc_filename = options.remotefilename_clientexe;
			dc_remoteUri = "http://" + options.serveraddress + "/";
			dc_temporaryLocalFilename = "tmpdc";
			
			window = win_;
		}
		
		public void begin ()
		{
			//Delete the last downloaded version number if we can find it
			if (File.Exists (version_filename)) {
				try {
					File.Delete (version_filename);
				} catch (Exception e) {
					Console.WriteLine ("{0} Exception", e);
				}
			}
			
			
			Uri myWebResource = new Uri (version_remoteUri + version_filename);
			
			versionWebClient = new WebClient ();
			versionWebClient.DownloadFileCompleted += new System.ComponentModel.AsyncCompletedEventHandler (handleVersionDownloadComplete);
			versionWebClient.DownloadProgressChanged += new DownloadProgressChangedEventHandler (handleVersionDownloadProgressChanged);
			
			//();
			
			Console.WriteLine ("Downloading file \"{0}\" from \"{1}\"\n", version_filename, myWebResource.ToString ());
			
			downloading = true;
			versionWebClient.DownloadFileAsync (myWebResource, version_filename);
			
			
		}
		void handleVersionDownloadProgressChanged (object sender, DownloadProgressChangedEventArgs r)
		{
			Console.Write (".");
		}
		
		void handleVersionDownloadComplete (object sender, System.ComponentModel.AsyncCompletedEventArgs r)
		{
			downloading = false;
			
			if (r.Cancelled) {
				Console.WriteLine ("failed to download file. Now what?");
				
				window.changeStatus ("Failed to Communicate with Server.");
				window.makeQuitVisible ();
				
			} else {
				
				Console.WriteLine ();
				Console.WriteLine ("file downloaded successfully to: " + Application.StartupPath);
				
				onlineVersion = getDownloadedVersion ();
				if (onlineVersion > currentVersion) {
					window.changeStatus ("New Version Available");
					Console.WriteLine ("New Version Available.");
					beginClientDownload ();
				} else {
					Console.WriteLine ("Using the latest version.");	
					window.changeStatus ("You have the latest version.");
					window.makeQuitVisible ();
					
					//startDC ();
					//window.quit ();
				}
				//
			}
		}
		
		private void beginClientDownload ()
		{
			if (File.Exists (dc_temporaryLocalFilename)) {
				try {
					File.Delete (dc_temporaryLocalFilename);
				} catch (Exception e) {
					window.changeStatus ("ERROR: Could not modify local temporary file.");
					window.makeQuitVisible ();
				}
			}
			
			Uri myWebResource = new Uri (dc_remoteUri + dc_filename);
			
			dcWebClient = new WebClient ();
			dcWebClient.DownloadFileCompleted += new System.ComponentModel.AsyncCompletedEventHandler (handleDCDownloadComplete);
			dcWebClient.DownloadProgressChanged += new DownloadProgressChangedEventHandler (handleDCDownloadProgressChanged);
			
			//();
			
			Console.WriteLine ("Downloading file \"{0}\" from \"{1}\"\n", dc_filename, myWebResource.ToString ());
			
			downloading = true;
			
			window.changeStatus ("Downloading New Version...");
			dcWebClient.DownloadFileAsync (myWebResource, dc_temporaryLocalFilename);
		}
		
		void handleDCDownloadProgressChanged (object sender, DownloadProgressChangedEventArgs r)
		{
			Console.WriteLine("Progress: " + r.ProgressPercentage);
			window.changeStatus ("Downloading New Version. Progress = " + r.ProgressPercentage);
		}
		
		void handleDCDownloadComplete (object sender, System.ComponentModel.AsyncCompletedEventArgs r)
		{
			downloading = false;
			
			if (r.Cancelled) {
				Console.WriteLine ("failed to download file. Now what?");
				
				window.changeStatus ("Failed to Communicate with Server.");
				window.makeQuitVisible ();
				
			} else {
				if (File.Exists (dc_temporaryLocalFilename)) {
					if (File.Exists (options.localfilename_clientexe)) {
						File.Delete (options.localfilename_clientexe);
					}
					try {
						File.Copy (dc_temporaryLocalFilename, options.localfilename_clientexe);
					} catch (Exception e) {
						//err?
						Console.WriteLine ("{0} Exception renaming downloaded client exe.", e);
						window.changeStatus ("Exception renaming file!");
						window.makeQuitVisible ();
						return;
					}
					if (File.Exists (dc_temporaryLocalFilename)) {
						try {
							File.Delete (dc_temporaryLocalFilename);
						} catch (Exception e) {
							window.changeStatus ("ERROR: Could not delete temporary download.");
							window.makeQuitVisible ();
						}
					}
					
					window.changeStatus ("Client downloaded and renamed successfully!");
					window.makeQuitVisible ();
					
					//two remaining steps:
					//   1. change file permission to executable (this may be a bitch)
					//	 2. run the client with startDC();
				} else {
					//Weird, we downloaded the file, but we can't find it!
					Console.WriteLine ("Error: We downloaded the file, but we can't find it");
					Console.WriteLine ("Quitting");
					window.changeStatus ("Downloaded client, but can not find it.");
					window.makeQuitVisible ();
					//window.quit ();	
				}
				
				
			}
		}
		
		public double getCurrentVersion ()
		{
			return readVersionFromFile (options.localfilename_currentversionnumber);
		}
		public double getDownloadedVersion ()
		{
			return readVersionFromFile (options.remotefilename_currentversionnumber);
		}
		public double readVersionFromFile (string filename)
		{
			if (File.Exists (filename)) {
				string[] lines = System.IO.File.ReadAllLines (@filename);
				if (lines.Length >= 1) {
					double currentVersion = -1;
					if (double.TryParse (lines[0], out currentVersion)) {
						
						return currentVersion;
						
					}
					
					return -1;
				}
			} else {
				return -1;
			}
			return -1;
		}
		
		private void startDC ()
		{
			Process.Start (options.localfilename_clientexe);
		}
	}
}

