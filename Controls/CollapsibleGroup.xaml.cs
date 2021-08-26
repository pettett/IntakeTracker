using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Markup;

namespace IntakeTrackerApp.Controls
{
	/// <summary>
	/// Interaction logic for CollapsibleGroup.xaml
	/// </summary>
	[ContentProperty(nameof(Children))]
	public partial class CollapsibleGroup : UserControl
	{
		public static readonly DependencyProperty HeaderProperty = DependencyProperty.Register("Header", typeof(string), typeof(CollapsibleGroup));
		public static readonly DependencyProperty EnabledProperty = DependencyProperty.Register(
			"Enabled", typeof(bool?), typeof(CollapsibleGroup), new FrameworkPropertyMetadata
			{
				BindsTwoWayByDefault = true,
				DefaultUpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged
			}
			);

		public static readonly DependencyPropertyKey ChildrenProperty = DependencyProperty.RegisterReadOnly(
	 nameof(Children),
	 typeof(UIElementCollection),
	 typeof(CollapsibleGroup),
	 new PropertyMetadata());

		public UIElementCollection Children
		{
			get { return (UIElementCollection)GetValue(ChildrenProperty.DependencyProperty); }
			private set { SetValue(ChildrenProperty, value); }
		}



		public static KeyValuePair<bool?, string>[] IsNeededOptions { get; set; } =
{
			new KeyValuePair<bool?, string>(null, "Unknown"),
			new KeyValuePair<bool?, string>(false, "Unneeded"),
			new KeyValuePair<bool?, string>(true, "Needed"),
		};
		public string Header
		{
			get => (string)GetValue(HeaderProperty);
			set => SetValue(HeaderProperty, value);
		}

		public bool? Enabled
		{
			get => GetValue(EnabledProperty) as bool?;
			set => SetValue(EnabledProperty, value);
		}
		public CollapsibleGroup()
		{
			InitializeComponent();
			Children = PART_host.Children;
		}
	}
}
