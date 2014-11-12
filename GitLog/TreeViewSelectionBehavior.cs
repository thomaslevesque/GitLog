using System.Windows;
using System.Windows.Controls;
using System.Windows.Interactivity;

namespace GitLog
{
    public class TreeViewSelectionBehavior : Behavior<TreeView>
    {
        protected override void OnAttached()
        {
            base.OnAttached();
            AssociatedObject.SelectedItemChanged += TreeView_SelectedItemChanged;
        }

        protected override void OnDetaching()
        {
            base.OnDetaching();
            AssociatedObject.SelectedItemChanged -= TreeView_SelectedItemChanged;
        }

        public object SelectedItem
        {
            get { return (object)GetValue(SelectedItemProperty); }
            set { SetValue(SelectedItemProperty, value); }
        }

        public static readonly DependencyProperty SelectedItemProperty =
            DependencyProperty.Register(
                "SelectedItem",
                typeof(object),
                typeof(TreeViewSelectionBehavior),
                new PropertyMetadata(null));

        private void TreeView_SelectedItemChanged(object sender, RoutedEventArgs e)
        {
            SelectedItem = AssociatedObject.SelectedItem;
        }
    }
}
