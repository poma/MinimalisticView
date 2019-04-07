using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Windows;
using System.Windows.Media;

namespace MinimalisticView
{
	public static class Extensions
	{
		public static T LoadResourceValue<T>(string xamlName)
		{
			return (T)((object)Application.LoadComponent(new Uri(Assembly.GetExecutingAssembly().GetName().Name + ";component/" + xamlName, UriKind.Relative)));
		}

		public static DependencyObject GetVisualOrLogicalParent(this DependencyObject sourceElement)
		{
			if (sourceElement is Visual)
				return VisualTreeHelper.GetParent(sourceElement) ?? LogicalTreeHelper.GetParent(sourceElement);
			return LogicalTreeHelper.GetParent(sourceElement);
		}

		public static IEnumerable<T> FindDescendants<T>(this DependencyObject obj) where T : class
		{
			List<T> descendants = new List<T>();
			obj.TraverseVisualTree<T>((Action<T>)(child => descendants.Add(child)));
			return (IEnumerable<T>)descendants;
		}

		public static void TraverseVisualTree<T>(this DependencyObject obj, Action<T> action) where T : class
		{
			for (int childIndex = 0; childIndex < VisualTreeHelper.GetChildrenCount(obj); ++childIndex) {
				DependencyObject child = VisualTreeHelper.GetChild(obj, childIndex);
				T obj1 = child as T;
				Action<T> action1 = action;
				child.TraverseVisualTreeReverse<T>(action1);
				if ((object)obj1 != null)
					action(obj1);
			}
		}

		public static void TraverseVisualTreeReverse<T>(this DependencyObject obj, Action<T> action) where T : class
		{
			for (int childIndex = VisualTreeHelper.GetChildrenCount(obj) - 1; childIndex >= 0; --childIndex) {
				DependencyObject child = VisualTreeHelper.GetChild(obj, childIndex);
				T obj1 = child as T;
				Action<T> action1 = action;
				child.TraverseVisualTreeReverse<T>(action1);
				if ((object)obj1 != null)
					action(obj1);
			}
		}
	}
}
