using System;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Markup;
using System.Xml.Linq;

namespace WpfGrowlNotifications {
    /// <summary>
    /// NotificationDataTemplateSelectorFromFile
    /// </summary>
	public class NotificationDataTemplateSelectorFromFile : DataTemplateSelector {
        public string FilesPath {
            get { return _filesPath; }
        }

        private readonly object _sync = new object();
        private readonly string _filesPath;
        private readonly string _horizontalPosition;

        /// <summary>
        /// Initialize new intance of class
        /// </summary>
        /// <param name="filesPath">Path to directory with template files</param>
        /// <param name="horizontalPosition">Horizontal position of notifications, they will be autoaligned</param>
        public NotificationDataTemplateSelectorFromFile(string filesPath, ContainerPosition horizontalPosition)
        {
            _filesPath = filesPath;
            _horizontalPosition = horizontalPosition == ContainerPosition.Left ? "Left" : "Right";
        }

        private const string HorizontalAlignmentAttribtureName = "HorizontalAlignment";

		public override DataTemplate SelectTemplate(object item, DependencyObject container) {
			if (item != null) {
				var notificationItem = item as INotification;
				if (notificationItem != null) {
					string templateFile = Path.Combine(FilesPath, notificationItem.DataTemplateSelectorArgument);
					lock (_sync) {
						// possible onetime file reading

						if (File.Exists(templateFile)) {
							XDocument doc;
							using (var fs = new FileStream(templateFile, FileMode.Open)) {
								doc = XDocument.Load(fs);
								fs.Close();
							}
							// TODO: do we need to check doc.Root for null value? not nessesary I think, if it is null - exception will be thrown
							if (doc.Root.Name == "{http://schemas.microsoft.com/winfx/2006/xaml/presentation}DataTemplate") {
								var allNodes = doc.Root.Elements().ToList();
								foreach (var node in allNodes) {
									// Overriding horizontal alignment of root template element(s?)
									if (!node.Name.ToString().Contains("DataTemplate")) {
										try {
											var overridenAttribute = node.Attribute(HorizontalAlignmentAttribtureName);
											if (overridenAttribute != null)
												overridenAttribute.Remove();
										}
										finally {
                                            node.Add(new XAttribute(HorizontalAlignmentAttribtureName, _horizontalPosition));
										}
									}
								}
								// Modified DataTemplate stored in memory:
								using (var stream = new MemoryStream()) {
									doc.Save(stream, SaveOptions.OmitDuplicateNamespaces);
									stream.Seek(0, SeekOrigin.Begin);
									var template = XamlReader.Load(stream) as DataTemplate;
									stream.Close();
									return template;
								}
							}
							throw new Exception("Problems with file stream");
						}
					    throw new Exception("Cannot open template file, file " + templateFile +"not exist");
					}
				}
				throw new Exception("Item type is " + item.GetType().FullName + ", interface " + typeof(INotification).FullName + " not implemented");
			}
            throw new NullReferenceException("item");
		}
	}
}