using System;
using System.Collections.Generic;
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
using RepoZ.Api.Git;
using TinyIoC;

namespace RepoZ.UI.Win.Wpf
{
	/// <summary>
	/// Interaction logic for MainWindow.xaml
	/// </summary>
	public partial class MainWindow : Window
	{
		public MainWindow()
		{
			InitializeComponent();

			var aggregator = TinyIoCContainer.Current.Resolve<IRepositoryInformationAggregator>();
			lstRepositories.ItemsSource = aggregator.Repositories;
		}
	}
}
