using System.Windows.Controls;

namespace WpfGrowlNotifications {
	internal class GrowlNotificationsWindowViewModel {
		private readonly string _containerWindowTitle;
        private readonly DataTemplateSelector _tempalteSelector;
	    private readonly Notifications _visibleNotifications;
	    private readonly Notifications _bufferedNotifications;

	    public GrowlNotificationsWindowViewModel(string containerWindowTitle, DataTemplateSelector templateSelector, Notifications visibleNotifications, Notifications bufferedNotifications) {
			_containerWindowTitle = containerWindowTitle;
		    _tempalteSelector = templateSelector;
	        _visibleNotifications = visibleNotifications;
	        _bufferedNotifications = bufferedNotifications;
	    }

		public string ContainerWindowTitle {
			get { return _containerWindowTitle; }
		}


        public DataTemplateSelector TemplateSelector
        {
            get { return _tempalteSelector; }
	    }

	    public Notifications VisibleNotifications {
            get { return _visibleNotifications; }
	    }

	    public Notifications BufferedNotifications {
            get { return _bufferedNotifications; }
	    }
	}
}