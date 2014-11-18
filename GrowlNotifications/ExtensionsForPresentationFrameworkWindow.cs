using System.Windows;
using System.Windows.Forms;
using System.Windows.Interop;

namespace WpfGrowlNotifications {
	internal static class ExtensionsForPresentationFrameworkWindow {
		public static Screen GetScreen(this Window window) {
			return Screen.FromHandle(new WindowInteropHelper(window).Handle);
		}
	}
}