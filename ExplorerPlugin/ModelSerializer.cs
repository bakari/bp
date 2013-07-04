using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Xml;

using AvalonDock;

namespace ExplorerPlugin
{
	/// <summary>
	/// Loads & saves direcotoryTreeViewModel to xml file
	/// </summary>
	public class ModelSerializer
	{
		private const string XmlNodeTag = "item";
		private const string XmlViewStateTag = "viewstate";
		
		private const string XmlNodeHeaderAtt = "header";
		private const string XmlNodePathAtt = "path";
		private const string XmlNodeTypeAtt = "type";
		private const string XmlNodeExpandedStateAtt = "expandedstate";
		private const string XmlViewStateNumberAtt = "number";
		
		public void SerializeModel(DirectoryTreeViewModel model, string fileName)
		{
			XmlTextWriter textWriter = new XmlTextWriter(fileName, System.Text.Encoding.UTF8);
			textWriter.WriteStartDocument();
			textWriter.WriteStartElement("TreeView");
			
			var list = new List<ItemViewModel>(model.FirstGeneration);
			SaveItems(list, textWriter);
			
			var index = model.ViewNumber.ToString();
			textWriter.WriteStartElement(XmlViewStateTag);
			textWriter.WriteAttributeString(XmlViewStateNumberAtt, index);
			textWriter.WriteEndElement();
			
			textWriter.WriteEndElement();
			textWriter.Close();
		}
		
		private void SaveItems(List<ItemViewModel> items, XmlTextWriter textWriter)
		{
			foreach (ItemViewModel item in items)
			{
				textWriter.WriteStartElement(XmlNodeTag);
				textWriter.WriteAttributeString(XmlNodeHeaderAtt, item.Header);
				textWriter.WriteAttributeString(XmlNodePathAtt, item.Path);
				textWriter.WriteAttributeString(XmlNodeTypeAtt, item.Type.ToString());
				textWriter.WriteAttributeString(XmlNodeExpandedStateAtt, item.IsExpanded.ToString());
				
				if (item.Children.Count > 0)
				{
					SaveItems(item.Children, textWriter);
				}
				
				textWriter.WriteEndElement();
			}
		}
		
		public void DeserializeModel(DirectoryTreeViewModel model, string fileName)
		{
			XmlTextReader textReader = null;
			
			try 
			{
				textReader = new XmlTextReader(fileName);
				
				model.FirstGeneration.Clear();
				ItemViewModel parent = null;
				
				while (textReader.Read())
				{
					if (textReader.NodeType == XmlNodeType.Element)
					{
						if (textReader.Name == XmlNodeTag)
						{
							var item = new ItemViewModel(new DirectoryTreeItem());
							item.Expanded += new RoutedEventHandler(model.treeViewItemExpanded);
							var isEmptyElement = textReader.IsEmptyElement;
							
							var attributeCount = textReader.AttributeCount;
							if (attributeCount > 0)
							{
								for (int i = 0; i < attributeCount; i++) 
								{
									textReader.MoveToAttribute(i);
									SetAttributeValue(item, textReader.Name, textReader.Value);
								}
							}
							
							if (parent != null)
							{
								item.Parent = parent;
								parent.Children.Add(item);
							}
							else
							{
								model.FirstGeneration.Add(item);
							}
							
							if (!isEmptyElement)
							{
								parent = item;
							}
						}
						else if (textReader.Name == XmlViewStateTag)
						{
							var attributeCount = textReader.AttributeCount;
							if (attributeCount > 0)
							{
								for (int i = 0; i < attributeCount; i++) 
								{
									textReader.MoveToAttribute(i);
									if (textReader.Name == XmlViewStateNumberAtt)
									{
										if (model.TreeView != null)
											model.TreeView.ChangeViewComboBox.SelectedIndex = int.Parse(textReader.Value);
									}
								}
							}
						}
					}
					else if (textReader.NodeType == XmlNodeType.EndElement)
					{
						if (textReader.Name == XmlNodeTag)
						{
							parent = parent.Parent;
						}
					}
					else if (textReader.NodeType == XmlNodeType.XmlDeclaration)
					{
						// ignore
					}
					else if (textReader.NodeType == XmlNodeType.None)
					{
						return;
					}
					else if (textReader.NodeType == XmlNodeType.Text)
					{
						// parent.Children.Add(textReader.Value);
					}
				}
				model.RootItem = model.FirstGeneration[0];
			}
			catch (Exception) {}
			finally
			{
				textReader.Close();
			}
		}
		
		private void SetAttributeValue(ItemViewModel item, string propertyName, string value)
		{
			if (propertyName == XmlNodeHeaderAtt)
			{
				item.Header = value;
			}
			else if (propertyName == XmlNodePathAtt)
			{
				item.Path = value;
			}
			else if (propertyName == XmlNodeTypeAtt)
			{
				item.Type = (DirectoryTreeItem.TYPE) Enum.Parse(typeof(DirectoryTreeItem.TYPE), value, true);
			}
			else if (propertyName == XmlNodeExpandedStateAtt)
			{
				item.IsExpanded = value.Contains("True");
			}
		}
	}
}
