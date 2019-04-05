using System;
using System.Runtime.InteropServices;
using System.Windows;
using Microsoft.VisualStudio.Shell;
using System.Diagnostics;
using System.Windows.Interop;

namespace MinimalisticView
{
	public sealed partial class MinimalisticView : AsyncPackage
	{
		public class NonClientMouseTracker
		{
			private const int WM_NCHITTEST = 0x0084;
			private const int WM_NCMOUSEMOVE = 0x00a0;
			private const int WM_NCMOUSELEAVE = 0x02a2;

			private const int HTCAPTION = 2;

			[Flags]
			public enum TMEFlags : uint
			{
				TME_CANCEL = 0x80000000,
				TME_HOVER = 0x00000001,
				TME_LEAVE = 0x00000002,
				TME_NONCLIENT = 0x00000010,
				TME_QUERY = 0x40000000,
			}

			[StructLayout(LayoutKind.Sequential)]
			public struct TRACKMOUSEEVENT
			{
				public Int32 cbSize;    // using Int32 instead of UInt32 is safe here, and this avoids casting the result  of Marshal.SizeOf()
				[MarshalAs(UnmanagedType.U4)]
				public TMEFlags dwFlags;
				public IntPtr hWnd;
				public UInt32 dwHoverTime;

				public TRACKMOUSEEVENT(TMEFlags dwFlags, IntPtr hWnd, UInt32 dwHoverTime)
				{
					this.cbSize = Marshal.SizeOf(typeof(TRACKMOUSEEVENT));
					this.dwFlags = dwFlags;
					this.hWnd = hWnd;
					this.dwHoverTime = dwHoverTime;
				}
			}

			[DllImport("user32.dll")]
			static extern int TrackMouseEvent(ref TRACKMOUSEEVENT lpEventTrack);

			public event Action MouseEnter;
			public event Action MouseLeave;
			public bool IsMouseOver { get; set; }

			private IntPtr _handle;

			public NonClientMouseTracker(Window wnd)
			{
				_handle = new WindowInteropHelper(wnd).Handle;
				HwndSource.FromHwnd(_handle).AddHook(new HwndSourceHook(WndProc));
			}

			private IntPtr WndProc(IntPtr hwnd, int msg, IntPtr wParam, IntPtr lParam, ref bool handled)
			{
				if (msg == WM_NCMOUSEMOVE) {
					if ((int)wParam == HTCAPTION) {
						if (!IsMouseOver) {
							IsMouseOver = true;
							SetTrackMouseEvent();
							OnMouseEnter();
						}
					}
				} else if (msg == WM_NCMOUSELEAVE) {
					IsMouseOver = false;
					OnMouseLeave();
				}

				return IntPtr.Zero;
			}

			private void SetTrackMouseEvent()
			{
				var tme = new TRACKMOUSEEVENT(TMEFlags.TME_NONCLIENT | TMEFlags.TME_LEAVE, _handle, 0);
				TrackMouseEvent(ref tme);
			}

			protected void OnMouseEnter()
			{
				MouseEnter?.Invoke();
			}

			protected void OnMouseLeave()
			{
				MouseLeave?.Invoke();
			}
		}
	}
}
