using System;
using System.IO;
using System.Windows;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LazyPC
	{
	public class AppEntry : BaseObject
		{
		private string _filePath = "";
		private string _appName = "";

		public AppEntry(Uri path)
			{
			try
				{
				_filePath = path.LocalPath.ToString();
				_appName = Path.GetFileName(path.ToString());
				}
			catch (Exception ex)
				{
				string error = "Encountered an error when creating Model for given path: " + ex.ToString();
				MessageBox.Show(error);
				}
			}

		public string filePath
			{
			get
				{
				return _filePath;
				}
			set
				{
				_filePath = value;
				OnPropertyChanged("filePath");
				}
			}

		public string appName
			{
			get
				{
				return _appName;
				}
			set
				{
				_appName = value;
				OnPropertyChanged("appName");
				}
			}

		public string GetName()
			{
			return _appName;
			}

		public string GetPath()
			{
			return _filePath;
			}
		}
	}
