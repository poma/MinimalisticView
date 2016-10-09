using System;
using System.ComponentModel;
using System.Linq;
using System.Windows;

namespace MinimalisticView
{
	public partial class StyleOverrides : ResourceDictionary, INotifyPropertyChanged
	{
		public static StyleOverrides Instance { get; private set; }

		double _height;
		public double Height
		{
			get { return _height; }
			set {
				if (_height == value)
					return;
				_height = value;
				if (PropertyChanged != null)
					PropertyChanged(this, new PropertyChangedEventArgs(nameof(Height)));
			}
		}		

		public StyleOverrides()
		{
			InitializeComponent();
			Instance = this;
		}

		private void Menu_GotFocus(object sender, RoutedEventArgs e)
		{
			Height = double.NaN;
		}

		private void Menu_LostFocus(object sender, RoutedEventArgs e)
		{
			Height = 0;
		}

		public event PropertyChangedEventHandler PropertyChanged;
		protected virtual void OnPropertyChanged(object sender, PropertyChangedEventArgs e)
		{
			PropertyChanged?.Invoke(sender, e);
		}
	}
}
