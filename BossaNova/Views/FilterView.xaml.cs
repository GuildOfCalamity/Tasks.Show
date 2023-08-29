using System.Windows.Controls;

namespace Tasks.Show.Views
{
    public partial class FilterView : UserControl
    {
        public FilterView()
        {
            if (System.ComponentModel.DesignerProperties.GetIsInDesignMode(this))
                return;

            InitializeComponent();
        }
    }
}
