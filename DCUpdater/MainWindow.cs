using System;
using Gtk;

public partial class MainWindow: Gtk.Window
{	
	public MainWindow (): base (Gtk.WindowType.Toplevel)
	{
		Build ();
		this.button1.Clicked += new EventHandler (button1_Click);
	}
	
	protected void button1_Click (object obj,EventArgs e)
	{
		Application.Quit ();
	}
	
	protected void OnDeleteEvent (object sender, DeleteEventArgs a)
	{
		Application.Quit ();
		a.RetVal = true;
	}
	
	public void makeQuitVisible ()
	{
		this.button1.Visible = true;
	}
	
	public void changeStatus (string status)
	{
		this.label1.Text = status;
	}
	public void quit ()
	{
		Application.Quit ();
	}
}
