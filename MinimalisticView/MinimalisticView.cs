using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Automation;
using System.Windows.Controls;
using System.Windows.Input;
using Microsoft.VisualStudio.PlatformUI;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;
using System.Windows.Threading;
using System.Threading;
using Task = System.Threading.Tasks.Task;

namespace MinimalisticView
{
	[PackageRegistration(UseManagedResourcesOnly = true, AllowsBackgroundLoading = true)]
	[InstalledProductRegistration("#110", "#112", "2.2", IconResourceID = 400)] // Info on this package for Help/About
	[Guid(MinimalisticView.PackageGuidString)]
	[ProvideAutoLoad(UIContextGuids.NoSolution, PackageAutoLoadFlags.BackgroundLoad)]
	[ProvideAutoLoad(UIContextGuids.SolutionExists, PackageAutoLoadFlags.BackgroundLoad)]
	// todo: ensure that it correctly handles localized "Environment" category
	[ProvideOptionPage(typeof(OptionPage), "Environment", "MinimalisticView", 0, 0, true, new[] { "MinimalisticView", "menu", "tab", "title", "hide" })]
	public sealed partial class MinimalisticView : AsyncPackage
	{
		public const string PackageGuidString = "a06e17e0-8f3f-4625-ac80-b80e2b4a0699";

		private bool _isMenuVisible;
		public bool IsMenuVisible
		{
			get {
				return _isMenuVisible;
			}
			set {
				if (_isMenuVisible == value)
					return;
				_isMenuVisible = value;
				UpdateMenuHeight();
				UpdateTitleHeight();
			}
		}

		private FrameworkElement _menuBar;
		public FrameworkElement MenuBar
		{
			get {
				return _menuBar;
			}
			set {
				_menuBar = value;
				UpdateMenuHeight();
				AddElementHandlers(_menuBar);
			}
		}

		private FrameworkElement _titleBar;
		public FrameworkElement TitleBar
		{
			get {
				return _titleBar;
			}
			set {
				_titleBar = value;
				UpdateTitleHeight();
				AddElementHandlers(_titleBar);
			}
		}

		private FrameworkElement _feedbackPanel;
		public FrameworkElement FeedbackPanel
		{
			get {
				return _feedbackPanel;
			}
			set {
				_feedbackPanel = value;
				UpdateFeedbackVisibility();
			}
		}

		private OptionPage _options;
		public OptionPage Options
		{
			get {
				if (_options == null) {
					_options = (OptionPage)GetDialogPage(typeof(OptionPage));
				}
				return _options;
			}
		}

		private ResourceDictionary _resourceOverrides;
		public ResourceDictionary ResourceOverrides {
			get {
				if (_resourceOverrides == null) {
					_resourceOverrides = Extensions.LoadResourceValue<ResourceDictionary>("StyleOverrides.xaml");
				}
				return _resourceOverrides;
			}
		}

		private NonClientMouseTracker _nonClientTracker;
		private Window _mainWindow;
		private DispatcherTimer _mouseEnterTimer;
		private DispatcherTimer _mouseLeaveTimer;


		void UpdateMenuHeight()
		{
			UpdateElementHeight(_menuBar);
		}

		void UpdateTitleHeight()
		{
			if (!Options.HideMenuOnly) {
				UpdateElementHeight(_titleBar, Options.CollapsedTitleHeight);
			} else {
				_titleBar.ClearValue(FrameworkElement.HeightProperty);
			}
		}

		void UpdateFeedbackVisibility()
		{
			_feedbackPanel.Visibility = Options.HideFeedback ? Visibility.Collapsed : Visibility.Visible;
		}

		void UpdateElementHeight(FrameworkElement element, double collapsedHeight = 0)
		{
			if (element == null) {
				return;
			}
			if (IsMenuVisible || !Options.TitleBarAutoHide) {
				element.ClearValue(FrameworkElement.HeightProperty);
			} else {
				element.Height = collapsedHeight;
			}
		}

		void AddElementHandlers(FrameworkElement element)
		{
			if (element == null) {
				return;
			}
			element.IsKeyboardFocusWithinChanged += OnContainerFocusChanged;
			element.MouseEnter += OnIsMouseOverChanged;
			element.MouseLeave += OnIsMouseOverChanged;
		}

		private void OnContainerFocusChanged(object sender, DependencyPropertyChangedEventArgs e)
		{
			IsMenuVisible = IsAggregateFocusInMenuContainer();
		}

		private void PopupLostKeyboardFocus(object sender, KeyboardFocusChangedEventArgs e)
		{
			if (IsMenuVisible && MenuBar != null && !IsAggregateFocusInMenuContainer()) {
				IsMenuVisible = false;
			}
		}

		private async void OnIsMouseOverChanged(object sender, MouseEventArgs e)
		{
			await System.Threading.Tasks.Task.Delay(1); // Workaround for mouse transition issues between client and non-client area (when both areas have IsMouseOver set to false)
			var IsMouseOver = (_menuBar?.IsMouseOver ?? false) || (!Options.HideMenuOnly && (_nonClientTracker.IsMouseOver || (_titleBar?.IsMouseOver ?? false)));

			// reset timers
			if (IsMouseOver) {
				_mouseLeaveTimer.IsEnabled = false;
			} else {
				_mouseEnterTimer.IsEnabled = false;
			}

			if (IsMenuVisible && !IsMouseOver) {
				_mouseLeaveTimer.IsEnabled = true;
			}
			if (!IsMenuVisible && IsMouseOver) {
				_mouseEnterTimer.IsEnabled = true;
			}
		}

		private bool IsAggregateFocusInMenuContainer()
		{
			if (MenuBar.IsKeyboardFocusWithin || (TitleBar.IsKeyboardFocusWithin && !Options.HideMenuOnly))
				return true;
			for (DependencyObject sourceElement = (DependencyObject)Keyboard.FocusedElement; sourceElement != null; sourceElement = sourceElement.GetVisualOrLogicalParent()) {
				if (sourceElement == MenuBar || (sourceElement == TitleBar && !Options.HideMenuOnly))
					return true;
			}
			return false;
		}

		private void _mouseEnterTimer_Tick(object sender, EventArgs e)
		{
			_mouseEnterTimer.IsEnabled = false;
			IsMenuVisible = true;
		}

		private void _mouseLeaveTimer_Tick(object sender, EventArgs e)
		{
			_mouseLeaveTimer.IsEnabled = false;
			if (!IsAggregateFocusInMenuContainer()) {
				IsMenuVisible = false;
			}
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="MinimalisticView"/> class.
		/// </summary>
		public MinimalisticView()
		{
			// Inside this method you can place any initialization code that does not require
			// any Visual Studio service because at this point the package object is created but
			// not sited yet inside Visual Studio environment. The place to do all the other
			// initialization is the Initialize method.
		}

		/// <summary>
		/// Initialization of the package; this method is called right after the package is sited, so this is the place
		/// where you can put all the initialization code that rely on services provided by VisualStudio.
		/// </summary>
		protected override async Task InitializeAsync(CancellationToken cancellationToken, IProgress<ServiceProgressData> progress)
		{
			await base.InitializeAsync(cancellationToken, progress);
			await JoinableTaskFactory.SwitchToMainThreadAsync(cancellationToken);

			_mainWindow = Application.Current.MainWindow;
			if (_mainWindow == null) {
				Trace.TraceError("mainWindow is null");
				return;
			}
			if (Options.HideTabs) {
				Application.Current.Resources.MergedDictionaries.Add(ResourceOverrides);
			}
			_mainWindow.LayoutUpdated += DetectLayoutElements;
			_nonClientTracker = new NonClientMouseTracker(_mainWindow);
			_nonClientTracker.MouseEnter += () => OnIsMouseOverChanged(null, null);
			_nonClientTracker.MouseLeave += () => OnIsMouseOverChanged(null, null);
			EventManager.RegisterClassHandler(typeof(UIElement), UIElement.LostKeyboardFocusEvent, new KeyboardFocusChangedEventHandler(PopupLostKeyboardFocus));
			_mouseEnterTimer = new DispatcherTimer { IsEnabled = false, Interval = TimeSpan.FromMilliseconds(Options.MouseEnterDelay) };
			_mouseLeaveTimer = new DispatcherTimer { IsEnabled = false, Interval = TimeSpan.FromMilliseconds(Options.MouseLeaveDelay) };
			_mouseEnterTimer.Tick += _mouseEnterTimer_Tick;
			_mouseLeaveTimer.Tick += _mouseLeaveTimer_Tick;
			Options.PropertyChanged += OptionsChanged;
		}

		private void OptionsChanged(object sender, PropertyChangedEventArgs e)
		{
			switch (e.PropertyName) {
				case nameof(Options.CollapsedTitleHeight):
					UpdateElementHeight(_titleBar, Options.CollapsedTitleHeight);
					break;
				case nameof(Options.TitleBarAutoHide):
				case nameof(Options.HideMenuOnly):
					UpdateMenuHeight();
					UpdateTitleHeight();
					break;
				case nameof(Options.HideFeedback):
					UpdateFeedbackVisibility();
					break;
				case nameof(Options.HideTabs):
					var dics = Application.Current.Resources.MergedDictionaries;
					if (Options.HideTabs && !dics.Contains(ResourceOverrides)) {
						dics.Add(ResourceOverrides);
					}
					if (!Options.HideTabs && dics.Contains(ResourceOverrides)) {
						dics.Remove(ResourceOverrides);
					}
					break;
				case nameof(Options.MouseEnterDelay):
					_mouseEnterTimer.Interval = TimeSpan.FromMilliseconds(Options.MouseEnterDelay);
					break;
				case nameof(Options.MouseLeaveDelay):
					_mouseLeaveTimer.Interval = TimeSpan.FromMilliseconds(Options.MouseLeaveDelay);
					break;
			}
		}

		private void DetectLayoutElements(object sender, EventArgs e)
		{
			if (MenuBar == null) {
				foreach (var descendant in _mainWindow.FindDescendants<Menu>()) {
					if (AutomationProperties.GetAutomationId(descendant) == "MenuBar") {
						FrameworkElement frameworkElement = descendant;
						var parent = descendant.GetVisualOrLogicalParent();
						if (parent != null)
							frameworkElement = parent.GetVisualOrLogicalParent() as DockPanel ?? frameworkElement;
						MenuBar = frameworkElement;
						break;
					}
				}
			}
			if (TitleBar == null) {
				var titleBar = _mainWindow.FindDescendants<MainWindowTitleBar>().FirstOrDefault();
				if (titleBar != null) {
					TitleBar = titleBar;
				}
			}
			if (FeedbackPanel == null) {
				FeedbackPanel = Application.Current.MainWindow.FindElement("FrameControlContainerBorder");
			}
			if (TitleBar != null && MenuBar != null) {
				_mainWindow.LayoutUpdated -= DetectLayoutElements;
			}
		}
	}
}
