using CodeInn.Common;
using CodeInn.Helpers;
using CodeInn.Model;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Popups;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkID=390556

namespace CodeInn.Views
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class ReadContactList : Page
    {
        ObservableCollection<Contacts> DB_ContactList = new ObservableCollection<Contacts>();
        private NavigationHelper navigationHelper;
        private ObservableDictionary defaultViewModel = new ObservableDictionary();

        public ReadContactList()
        {
            this.InitializeComponent();
            this.Loaded += ReadContactList_Loaded;
            this.navigationHelper = new NavigationHelper(this);
            this.navigationHelper.LoadState += this.NavigationHelper_LoadState;
            this.navigationHelper.SaveState += this.NavigationHelper_SaveState;
        }

        public NavigationHelper NavigationHelper
        {
            get { return this.navigationHelper; }
        }
        public ObservableDictionary DefaultViewModel
        {
            get { return this.defaultViewModel; }
        }
        private void NavigationHelper_LoadState(object sender, LoadStateEventArgs e)
        {
        }
        private void NavigationHelper_SaveState(object sender, SaveStateEventArgs e)
        {
        }


        private void ReadContactList_Loaded(object sender, RoutedEventArgs e)
        {
            ReadAllContactsList dbcontacts = new ReadAllContactsList();
            DB_ContactList = dbcontacts.GetAllContacts();//Get all DB contacts 
            listBoxobj.ItemsSource = DB_ContactList.OrderByDescending(i => i.Id).ToList();//Binding DB data to LISTBOX and Latest contact ID can Display first. 
        }
        private void Command(IUICommand command)
        {
            if (command.Label.Equals("Yes"))
            {
                DatabaseHelperClass Db_Helper = new DatabaseHelperClass();
                Db_Helper.DeleteAllContact();//delete all DB contacts 
                DB_ContactList.Clear();//Clear collections 
                listBoxobj.ItemsSource = DB_ContactList;
            }
        }
        private void listBoxobj_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
        }
    }
}
