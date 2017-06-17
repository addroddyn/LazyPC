using System;
using System.ComponentModel;

namespace LazyPC
	{
	/// <summary>
	/// Description of BaseObject.
	/// </summary>
	public class BaseObject : INotifyPropertyChanged
		{
		public event PropertyChangedEventHandler PropertyChanged;

		public void OnPropertyChanged(string propertyName)
			{
			PropertyChangedEventHandler handler = PropertyChanged;
			if (handler != null)
				{
				handler(this, new PropertyChangedEventArgs(propertyName));
				}
			}
		}
	}
