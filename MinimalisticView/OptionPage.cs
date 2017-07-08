using Microsoft.VisualStudio.Shell;
using System.ComponentModel;

namespace MinimalisticView
{
	public class OptionPage : DialogPage, INotifyPropertyChanged
	{
		bool _hideTabs = false;
		[DisplayName("Hide tabs")]
		[Description("Whether to hide tab bar")]
		public bool HideTabs
		{
			get {
				return _hideTabs;
			}
			set {
				if (_hideTabs == value) {
					return;
				}

				_hideTabs = value;
				PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(HideTabs)));
			}
		}

		bool _titleBarAutoHide = true;
		[DisplayName("Hide title and menu bars")]
		[Description("You can access menu bar using hotkeys such as Alt or Ctrl-Q")]
		public bool TitleBarAutoHide
		{
			get {
				return _titleBarAutoHide;
			}
			set {
				if (_titleBarAutoHide == value) {
					return;
				}

				_titleBarAutoHide = value;
				PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(TitleBarAutoHide)));
			}
		}

		int _collapsedTitleHeight = 2;
		[DisplayName("Collapsed title height")]
		[Description("Height of collapsed title bar. If set to zero removes it completely but menu cannot expand on mouse over.")]
		public int CollapsedTitleHeight
		{
			get {
				return _collapsedTitleHeight;
			}
			set {
				if (_collapsedTitleHeight == value) {
					return;
				}

				_collapsedTitleHeight = value;
				PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(CollapsedTitleHeight)));
			}
		}
		public event PropertyChangedEventHandler PropertyChanged;
	}
}
