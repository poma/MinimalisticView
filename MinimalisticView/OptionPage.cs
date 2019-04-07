using System;
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

		bool _hideMenuOnly = false;
		[DisplayName("Hide menu only")]
		[Description("Hide menu bar but leave title bar visible")]
		public bool HideMenuOnly
		{
			get {
				return _hideMenuOnly;
			}
			set {
				if (_hideMenuOnly == value) {
					return;
				}

				_hideMenuOnly = value;
				PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(HideMenuOnly)));
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

				_collapsedTitleHeight = Math.Max(value, 0);
				PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(CollapsedTitleHeight)));
			}
		}

		int _mouseEnterDelay = 0;
		[DisplayName("Mouse enter delay")]
		[Description("Delay after mouse enters collapsed menu and before menu pops up")]
		public int MouseEnterDelay
		{
			get {
				return _mouseEnterDelay;
			}
			set {
				if (_mouseEnterDelay == value) {
					return;
				}

				_mouseEnterDelay = Math.Max(value, 0);
				PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(MouseEnterDelay)));
			}
		}

		int _mouseLeaveDelay = 0;
		[DisplayName("Mouse leave delay")]
		[Description("Delay after mouse leaves menu and before menu collapses back")]
		public int MouseLeaveDelay
		{
			get {
				return _mouseLeaveDelay;
			}
			set {
				if (_mouseLeaveDelay == value) {
					return;
				}

				_mouseLeaveDelay = Math.Max(value, 0);
				PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(MouseLeaveDelay)));
			}
		}
		public event PropertyChangedEventHandler PropertyChanged;
	}
}
