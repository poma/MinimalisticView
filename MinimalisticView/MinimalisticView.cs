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

namespace MinimalisticView
{
	[PackageRegistration(UseManagedResourcesOnly = true)]
	[InstalledProductRegistration("#110", "#112", "2.2", IconResourceID = 400)] // Info on this package for Help/About
	[Guid(MinimalisticView.PackageGuidString)]
	[ProvideAutoLoad(UIContextGuids.NoSolution)]
	[ProvideAutoLoad(UIContextGuids.SolutionExists)]
	// todo: ensure that it correctly handles localized "Environment" category
	[ProvideOptionPage(typeof(OptionPage), "Environment", "MinimalisticView", 0, 0, true, new[] { "MinimalisticView", "menu", "tab", "title", "hide" })]
	public sealed partial class MinimalisticView : Package
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
			IsMenuVisible = ((_titleBar?.IsMouseOver ?? false) && !Options.HideMenuOnly) || (_menuBar?.IsMouseOver ?? false) || (_nonClientTracker.IsMouseOver && !Options.HideMenuOnly) || IsAggregateFocusInMenuContainer();
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
		protected override void Initialize()
		{
			base.Initialize();
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
				case nameof(Options.HideTabs):
					var dics = Application.Current.Resources.MergedDictionaries;
					if (Options.HideTabs && !dics.Contains(ResourceOverrides)) {
						dics.Add(ResourceOverrides);
					}
					if (!Options.HideTabs && dics.Contains(ResourceOverrides)) {
						dics.Remove(ResourceOverrides);
					}
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
			if (TitleBar != null && MenuBar != null) {
				_mainWindow.LayoutUpdated -= DetectLayoutElements;
			}
		}
	}
}
