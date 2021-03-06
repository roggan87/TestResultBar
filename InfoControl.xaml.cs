using EnvDTE;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.TestWindow.Extensibility;
using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace TestResultBar
{
    [Export(typeof(InfoControl))]
	public partial class InfoControl : UserControl
	{
        [Import]
        private ITestsService TestsService;

        [Import]
        internal SVsServiceProvider ServiceProvider = null;

        public InfoControl()
		{
            InitializeComponent();
		}

        public void SetPassedTests(string text)
        {
            this.Dispatcher.BeginInvoke((Action)delegate {
                PassedTestsCount.Text = text;
            });
        }

        public void UpdateWithTestResult(IEnumerable<ITest> tests)
        {
            this.Dispatcher.BeginInvoke((Action)delegate {
                ITest[] passedTests = tests.Where(t => t.State == TestState.Passed).ToArray();
                ITest[] failedTests = tests.Where(t => t.State == TestState.Failed).ToArray();
                SetTestCounts(passedTests.Count(), failedTests.Count());
                SetBackgroundColor(failedTests.Count());
                UpdatePopup(failedTests, true);
            });

        }

        public void ResetBackgroundColor()
        {
            this.Dispatcher.BeginInvoke((Action)delegate
            {
                Background = new SolidColorBrush(Colors.Transparent);
            });
        }

        private void SetTestCounts(int passedCount, int failedCount)
        {
            PassedTestsCount.Text = passedCount.ToString();
            FailedTestsCount.Text = failedCount.ToString();
        }

        private void SetBackgroundColor(int failedCount)
        {
            Color bgColor = failedCount == 0 ? Colors.Green : Colors.Red;
            Background = new SolidColorBrush(bgColor);
        }

        private void UpdatePopup(ITest[] tests, bool showPopup)
        {
            FailedTestsPopupContent.Children.Clear();
            foreach (ITest test in tests)
            {
                StackPanel stackPanel = new StackPanel
                {
                    Orientation = Orientation.Horizontal,
                    VerticalAlignment = System.Windows.VerticalAlignment.Center
                };
                stackPanel.Children.Add(new Image
                {
                    Source = new BitmapImage( new Uri("Resources/FailedTest.png", UriKind.Relative))
                });
                stackPanel.Children.Add(new TextBlock
                {
                    Text = test.DisplayName,
                    Margin = new System.Windows.Thickness(4, 0, 0, 0)
                });
                FailedTestsPopupContent.Children.Add(stackPanel);
            }
            if (showPopup && tests.Count() > 0)
            {
                FailedTestsPopup.IsOpen = true;
                FailedTestsPopup.StaysOpen = false;
            }
        }

        public void RunAllTests(object sender, System.Windows.RoutedEventArgs e)
        {
            try
            {
                DTE dte = (DTE)ServiceProvider.GetService(typeof(DTE));
                dte.ExecuteCommand("TestExplorer.RunAllTests");
            }
            catch(Exception ex)
            {
                // Write to Tests pane
            }
        }

        public void OpenTestExplorer(object sender, System.Windows.RoutedEventArgs e)
        {
            DTE dte = (DTE)ServiceProvider.GetService(typeof(DTE));
            Window window = dte.Windows.Item("{E1B7D1F8-9B3C-49B1-8F4F-BFC63A88835D}");
            window.Activate();
        }
    }
}