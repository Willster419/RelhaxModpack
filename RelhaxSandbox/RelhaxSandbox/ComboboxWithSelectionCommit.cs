using System.Windows.Controls;

namespace RelhaxSandbox
{
    //https://stackoverflow.com/questions/623451/how-can-i-make-my-own-event-in-c
    public delegate void OnSelectionCommitted(object source, SelectionChangedEventArgs e);

    public class ComboboxWithSelectionCommit : ComboBox
    {
        public event OnSelectionCommitted SelectionCommitted;

        //keeps track of the actual selected value
        private int SelectedIndexBackup = -1;

        //toggle to verify that the init load selection code only happens once
        private bool DefaultSelectionSet = false;

        //flag to determine if the selection commit event should fire
        //this needs to be set to false right before any selected properties are set
        private bool SelectionFromUser = true;

        public ComboboxWithSelectionCommit()
        {
            this.Loaded += ComboboxWithSelectionCommit_Loaded;
            this.SelectionChanged += ComboboxWithSelectionCommit_SelectionChanged;
            this.DropDownOpened += ComboboxWithSelectionCommit_DropDownOpened;
            this.DropDownClosed += ComboboxWithSelectionCommit_DropDownClosed;
        }

        private void ComboboxWithSelectionCommit_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if(!SelectionFromUser)
            {
                SelectionFromUser = true;
                return;
            }

            SelectionCommitted?.Invoke(this, e);
        }

        private void ComboboxWithSelectionCommit_Loaded(object sender, System.Windows.RoutedEventArgs e)
        {
            if(!DefaultSelectionSet)
            {
                SelectionFromUser = false;
                SelectedIndex = 0;
                DefaultSelectionSet = true;
            }
        }

        private void ComboboxWithSelectionCommit_DropDownOpened(object sender, System.EventArgs e)
        {
            //set selection to nothing, but back it up first
            SelectedIndexBackup = this.SelectedIndex;
            SelectionFromUser = false;
            this.SelectedIndex = -1;
        }

        private void ComboboxWithSelectionCommit_DropDownClosed(object sender, System.EventArgs e)
        {
            if(this.SelectedIndex == -1)
            {
                //a selection was not chosen, the user must have clicked out, so set the original back
                SelectionFromUser = false;
                this.SelectedIndex = SelectedIndexBackup;
            }
        }
    }
}
