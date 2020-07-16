using GlacierLocalizationTools.ViewModel;
using System;
using System.Collections.Generic;
using System.IO;
using System.Windows;

namespace GlacierLocalizationTools.View
{
    /// <summary>
    /// Interaction logic for OneTimeRunWindow.xaml
    /// </summary>
    public partial class OneTimeRunWindow : Window
    {
        public OneTimeRunWindow()
        {
            InitializeComponent();

            OneTimeRunViewModel oneTimeRunViewModel = new OneTimeRunViewModel();
            oneTimeRunViewModel.RequestClose += (s, e) => this.Dispatcher.Invoke(new Action(() => Close())); // violates MVVM
            DataContext = oneTimeRunViewModel;
        }

        protected override void OnContentRendered(EventArgs e)
        {
            try
            {
                OneTimeRunViewModel oneTimeRunViewModel = DataContext as OneTimeRunViewModel;
                if (oneTimeRunViewModel != null)
                {
                    if (oneTimeRunViewModel.Export == null)
                    {
                        Close();
                    }
                    else if (oneTimeRunViewModel.Export == true)
                    {
                        oneTimeRunViewModel.ExtractByParameterCommand.Execute(oneTimeRunViewModel);
                    }
                    else
                    {
                        oneTimeRunViewModel.ImportByParameterCommand.Execute(oneTimeRunViewModel);
                    }
                }
            }
            catch (Exception ex)
            {
                var mess = new List<string>();
                mess.Add(ex.Message);
                mess.Add(ex.StackTrace);
                if (ex.InnerException != null)
                {
                    mess.Add(ex.InnerException.Message);
                    mess.Add(ex.InnerException.StackTrace);
                }

                MessageBox.Show(string.Join("\n", mess));
            }
        }
    }
}
