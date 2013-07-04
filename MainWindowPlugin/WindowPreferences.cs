using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;

namespace MainWindowPlugin
{
    class WindowPreferences
    {
    	public double windowTop;
		public double windowLeft;
	    public double windowWidth;
	    public double windowHeight;
	    public WindowState windowState;
	
	    public WindowPreferences()
	    {
	    	Load();
	    	MoveIntoView();
	    }
	    
	    private void Load()
	    {
	    	windowTop = Properties.Settings.Default.WindowTop;
	    	windowLeft = Properties.Settings.Default.WindowLeft;
	    	windowWidth = Properties.Settings.Default.WindowWidth;
	    	windowHeight = Properties.Settings.Default.WindowHeight;
	    	windowState = Properties.Settings.Default.WindowState;
	    }

	    public void Save()
	    {
	    	if (windowState != WindowState.Minimized)
	    	{
		    	Properties.Settings.Default.WindowTop = windowTop;
		    	Properties.Settings.Default.WindowLeft = windowLeft;
		    	Properties.Settings.Default.WindowWidth = windowWidth;
		    	Properties.Settings.Default.WindowHeight = windowHeight;
                Properties.Settings.Default.WindowState = windowState;
		    	Properties.Settings.Default.Save();
	    	}
	    }
	    
	    private void MoveIntoView()
	    {
	    	if (windowTop + windowHeight / 2 > System.Windows.SystemParameters.VirtualScreenHeight)
	    	{
	    		windowTop = 100;
                windowLeft = 100;
	    	}
	    	
	    	if (windowLeft + windowWidth / 2 > System.Windows.SystemParameters.VirtualScreenWidth)
	    	{
                windowLeft = 100;
                windowTop = 100;
	    	}
	    }
    }
}
