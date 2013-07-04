using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel.Composition;
using System.Threading;
using System.Windows;
using System.Windows.Threading;
using Framework.Core;

namespace ExplorerPlugin
{
    /// <summary>
    /// Search logic
    /// </summary>
    public class SearchWorker : ISearchWorker
    {
        private DirectoryTreeViewModel model; 
        private List<ItemViewModel> filteredItems = new List<ItemViewModel>();
        private List<ItemViewModel> clonedModelList = new List<ItemViewModel>();
        private Dispatcher UI_Dispatcher = Application.Current.Dispatcher;
        public static EventWaitHandle WaitHandle = new AutoResetEvent(true);
        // Max number of viewed filtered results
        public static int Threshold;

        #region Constructor
        public SearchWorker(DirectoryTreeViewModel model)
        {
            this.model = model;
            Threshold = 15;
            
            // Search proceed in duplicated list of expanded items
            var duplicatedModel = new DirectoryTreeViewModel(model.TreeView, model.RootItem);
            foreach (var item in duplicatedModel.GetExpandedItems())
            {
                clonedModelList.Add(item.Clone());
            }
        }
        #endregion

        #region Search Logic
        void PerformSearch()
        {
            // expanded items go first
            var queue = new Queue<ItemViewModel>();
			var counter = 0;
            foreach (var top in clonedModelList)
            {
                queue.Enqueue(top);
            }

            while (queue.Count > 0)
            {
            	if (counter > Threshold) WaitHandle.WaitOne();
                
                var item = queue.Dequeue();

                if (item != null)
                {
                    if (item.Type == DirectoryTreeItem.TYPE.File &&
                        item.Header.ToUpper().Contains(model.SearchText.ToUpper()))
                    {
                		counter++;
                        if (counter <= Threshold)	
                        	AddItem(item);
                        if (counter == Threshold)	
                        	AddInfoItem();
                        filteredItems.Add(item);
                    }

                    item.IsExpanded = true;

                    if (item.Children != null && item.Children.Count > 0)
                    {
                        foreach (var child in item.Children)
                        {
                            queue.Enqueue(child);
                        }
                    }
                }
            }
        }

        void AddItem(ItemViewModel item)
        {
            if (filteredItems.Contains(item)) return;
            switch (model.ViewNumber)
            {
                // 0 defualt view
                case 0: AddToModel(item);; break;
                case 1: AddItemWithView1(item); break;
                case 2: AddItemWithView2(item); break;
                case 3: AddItemWithView3(item); break;
                case 4: AddItemWithView4(item); break;
                default: break;
            }
        }

        #region Info Item
        private void AddInfoItem()
        {
            var infoNode = new ItemViewModel(new DirectoryTreeItem());
            infoNode.Header = Environment.NewLine + "Found more then " + Threshold.ToString() + " items"
                + Environment.NewLine + "Click here to show more!" + Environment.NewLine;
            infoNode.Type = DirectoryTreeItem.TYPE.Info;
            AddToModel(infoNode);
        }
        #endregion
        #endregion

        #region ViewSetups

        void AddItemWithView1(ItemViewModel item)
        {
            item.Header = item.Path;
            AddToModel(item);
        }

        void AddItemWithView2(ItemViewModel item)
        {
            item.Header = item.Path.Substring(item.Path.LastIndexOf('\\') + 1) + " ("
                                + item.Path.Substring(0, item.Path.LastIndexOf('\\')) + ")";
            AddToModel(item);
        }

        // ugly code #1
        void AddItemWithView3(ItemViewModel item)
        {
            var parents = item.Path.Split('\\');
            if (parents[0] != null) parents[0] = parents[0] + "\\";
            // tree view item level we explore
            var list = new List<ItemViewModel>(model.FirstGeneration);
            for (int i = 0; i < parents.Length; i++)
            {
                // check if the parent[i] already exists
                var temp = list.Find(delegate(ItemViewModel ivm)
                    {
                        return ivm.Path.Contains(item.Path.Substring
                           (0, item.Path.IndexOf(parents[i]) + parents[i].Length));
                    });
                if (temp == null)
                {
                    // if parent[i] of item or item itself is not already in model, create it
                    var dti = new DirectoryTreeItem();
                    dti.Header = parents[i];
                    dti.Path = item.Path.Substring(0, item.Path.IndexOf(parents[i]) + parents[i].Length);
                    if (i == 0)
                    {
                        dti.Type = DirectoryTreeItem.TYPE.Drive;
                    }
                    else if (i != 0 && i < parents.Length - 1)
                    {
                        dti.Type = DirectoryTreeItem.TYPE.Folder;
                    }
                    else if (i == parents.Length - 1)
                    {
                        dti.Type = DirectoryTreeItem.TYPE.File;
                    }
                    var newItem = new ItemViewModel(dti);
                    newItem.IsExpanded = true;
                    list.Add(newItem);
                    if (i == 0 && model.FirstGeneration.Count == 0)
                    {
                    	AddToModel(newItem);
                    }
                    list = newItem.Children;
                }
                else
                {
                    // else set item level we explore as item's / parent[i]'s children
                    list = temp.Children;
                }
            }
            // refresh
            RefreshModel(model.FirstGeneration[0]);
            //dispatcher.Invoke((Action)(() => MainWindow.Instance.treeView.AddItemToNavigation(model.FirstGeneration[0])));
        }

        // ugly code #2
        void AddItemWithView4(ItemViewModel item)
        {
            var parentList = new List<String>();
            // parent 1 i.e. C:
            parentList.Add(item.Path.Substring(0, item.Path.IndexOf('\\') + 1));
            // parent 2 i.e. folder path
            if (item.Path.Split('\\').Length - 1 > 1)
            {
                parentList.Add(item.Path.Substring(item.Path.IndexOf('\\') + 1, item.Path.LastIndexOf('\\') - 3));
            }
            // filtered item itself
            parentList.Add(item.Path.Substring(item.Path.LastIndexOf('\\') + 1));
            var parents = parentList.ToArray();

            // tree view item level we explore
            var list = new List<ItemViewModel>(model.FirstGeneration);
            for (int i = 0; i < parents.Length; i++)
            {
                // check if the parent[i] already exists
                var temp = list.Find(delegate(ItemViewModel ivm)
                    {
                        return ivm.Path.Contains(item.Path.Substring
                           (0, item.Path.IndexOf(parents[i]) + parents[i].Length));
                    });
                if (temp == null)
                {
                    // if parent[i] of item or item itself is not already in model, create it
                    var dti = new DirectoryTreeItem();
                    dti.Header = parents[i];
                    dti.Path = item.Path.Substring(0, item.Path.IndexOf(parents[i]) + parents[i].Length);
                    if (i == 0)
                    {
                        dti.Type = DirectoryTreeItem.TYPE.Drive;
                    }
                    else if (i != 0 && i < parents.Length - 1)
                    {
                        dti.Type = DirectoryTreeItem.TYPE.Folder;
                    }
                    else if (i == parents.Length - 1)
                    {
                        dti.Type = DirectoryTreeItem.TYPE.File;
                    }
                    var newItem = new ItemViewModel(dti);
                    newItem.IsExpanded = true;
                    list.Add(newItem);
                    AddToModel(newItem);
                    list = newItem.Children;
                }
                else
                {
                    // else set item level we explore as item's / parent[i]'s children
                    list = temp.Children;
                }
            }
            // refresh
            RefreshModel(model.FirstGeneration[0]);
            //dispatcher.Invoke((Action)(() => MainWindow.Instance.treeView.AddItemToNavigation(model.FirstGeneration[0])));
        }
        #endregion
		
        public void Work()
        {
        	if (model.SearchText.Length == 0)
            {
        		RefreshModel(model.RootItem);
        		foreach (var root in model.FirstGeneration)
        		{
        			Action action = () => model.TreeView.AddItemToNavigation(root);
	        		UI_Dispatcher.Invoke(DispatcherPriority.Render, action);
        		}
            }
        	else
        	{
        		ClearModel();
        		PerformSearch();	
        	}
        }
        
        void AddToModel(ItemViewModel item)
        {
        	if (model.FirstGeneration.Contains(item)) return;
        	
        	Action action = () => model.FirstGeneration.Add(item);
        	UI_Dispatcher.Invoke(DispatcherPriority.Render, action);
        }
        
        void ClearModel()
        {
        	Action action = () => model.FirstGeneration.Clear();
	        UI_Dispatcher.Invoke(DispatcherPriority.Render, action);
        }
               
        void RefreshModel(ItemViewModel item)
        {
        	ClearModel();
        	AddToModel(item);
        }
    }
}
