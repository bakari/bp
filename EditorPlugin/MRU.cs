using System;
using System.Collections.Specialized;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Linq;
using AvalonDock;

namespace EditorPlugin
{
    /// <summary>
    /// Most recent used files and opened tabs save & load logic
    /// </summary>
    public class MRU
    {
    	Editor editorPlugin;
    	
    	public MRU(Editor ep)
    	{
    		editorPlugin = ep;
    	}
    	
        #region Most recent used files
        public void UpdateMRUMenu()
        {
        	MenuItem fileMenu = (from MenuItem item in editorPlugin.MainWindow.Menu.Items where item.Header.ToString().Contains("_File") select item).FirstOrDefault();
        	if (fileMenu == null)
	        	return;
        	var mRUMenu = (from Control item in fileMenu.Items where item.Name.ToString().Contains("menuItem_recentFiles") select item).FirstOrDefault() as MenuItem;
			if (mRUMenu == null)
				return;
        	mRUMenu.Items.Clear();
            int i = 1;
            foreach (var path in Properties.Settings.Default.MRU)
            {
                var newItem = new MenuItem();
                newItem.Tag = path;
                newItem.Header = /*i.ToString() + " " +*/path;
                newItem.Click += new RoutedEventHandler(mRUMenuItem_Click);
                mRUMenu.Items.Add(newItem);
                i++;
            }
            if (mRUMenu.Items.Count > 0)
            {
                mRUMenu.Items.Add(new Separator());
                var clearMRUItem = new MenuItem();
                clearMRUItem.Header = "Clear recent files list";
                clearMRUItem.Click += new RoutedEventHandler(clearMRUItem_Click);
                mRUMenu.Items.Add(clearMRUItem);
            }
        }

        public void AddToMRU(string path)
        {
            if (!Properties.Settings.Default.MRU.Contains(path))
            {
                Properties.Settings.Default.MRU.Insert(0, path);
                while (Properties.Settings.Default.MRU.Count > 9)
                {
                    Properties.Settings.Default.MRU.RemoveAt(Properties.Settings.Default.MRU.Count - 1);
                }
                Properties.Settings.Default.Save();
                UpdateMRUMenu();
            }
        }

        public void mRUMenuItem_Click(object sender, RoutedEventArgs e)
        {
            var item = sender as MenuItem;
            if (item == null) return;
            {
                var match = Regex.Match(item.Tag.ToString(), @"\\([^\\\(]*)?$");
                var name = match.Success ? match.Groups[1].Value : "?";
                editorPlugin.CreateNewTab(name, item.Tag.ToString());
            }
        }

        public void clearMRUItem_Click(object sender, RoutedEventArgs e)
        {
            Properties.Settings.Default.MRU.Clear();
            UpdateMRUMenu();
        }
        #endregion

        #region Opened Tabs
        public void SaveOpenedTabs()
        {
            Properties.Settings.Default.OpenedTabs.Clear();
            var tabPane = editorPlugin.MainWindow.TabPane;
            if (tabPane != null && tabPane.HasItems)
            {
                foreach (var tab in tabPane.Items)
                {
                    var path = (tab as DocumentContent).Tag as String;
                    if (path != null)
                    {
                        Properties.Settings.Default.OpenedTabs.Add(path);
                    }
                }
                Properties.Settings.Default.Save();
            }
        }

        public void LoadOpenedTabs()
        {
            foreach (var path in Properties.Settings.Default.OpenedTabs)
            {
                if (path != null)
                {
                    var sMatch = Regex.Match(path, @"\\([^\\]*)?$");
                    var title = sMatch.Success ? sMatch.Groups[1].Value : path;
                    try {
                    	editorPlugin.CreateNewTab(title, path);
                    }
                    catch (Exception) {}
                }
            }
        }
        #endregion
    }
}
