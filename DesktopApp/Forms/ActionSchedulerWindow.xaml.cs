using DesktopApp.DragAndDropList;
using ListViewDragDropManagerDemo.Forms;
using Models;
using Models.ActionTypes;
using System;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace DesktopApp
{
    /// <summary>
    /// Demonstrates how to use the ListViewDragManager class.
    /// </summary>
    public partial class ActionSchedulerWindow : System.Windows.Window
    {
        ListViewDragDropManager<ScriptInfoItem> dragMgr;

        public ActionSchedulerWindow()
        {
            InitializeComponent();
            this.Loaded += ActionSchedulerWindow_Loaded;
        }

        void ActionSchedulerWindow_Loaded(object sender, RoutedEventArgs e)
        {
            //todo add items
            ObservableCollection<ScriptInfoItem> items = new ObservableCollection<ScriptInfoItem>();
            listView.ItemsSource = items;

            dragMgr = new ListViewDragDropManager<ScriptInfoItem>(listView);
            listView.DragEnter += OnListViewDragEnter;
            listView.Drop += OnListViewDrop;
        }

        void dragMgr_ProcessDrop(object sender, ProcessDropEventArgs<ScriptInfoItem> e)
        {
            int higherIdx = Math.Max(e.OldIndex, e.NewIndex);
            int lowerIdx = Math.Min(e.OldIndex, e.NewIndex);

            if (lowerIdx < 0)
            {
                e.ItemsSource.Insert(higherIdx, e.DataItem);
            }
            else
            {
                if (e.ItemsSource[lowerIdx] == null ||
                    e.ItemsSource[higherIdx] == null)
                    return;

                e.ItemsSource.Move(lowerIdx, higherIdx);
                e.ItemsSource.Move(higherIdx - 1, lowerIdx);
            }

            e.Effects = DragDropEffects.Move;
        }

        void OnListViewDragEnter(object sender, DragEventArgs e)
        {
            e.Effects = DragDropEffects.Move;
        }

        void OnListViewDrop(object sender, DragEventArgs e)
        {
            if (e.Effects == DragDropEffects.None)
                return;

            ScriptInfoItem item = e.Data.GetData(typeof(ScriptInfoItem)) as ScriptInfoItem;
            if (sender == this.listView)
            {
                if (this.dragMgr.IsDragInProgress)
                    return;
            }
            else
            {
                (this.listView.ItemsSource as ObservableCollection<ScriptInfoItem>).Remove(item);
            }
        }

        private void AddButtonClick(object sender, RoutedEventArgs e)
        {
            ScriptInfoItem item = new ScriptInfoItem(
                new ControlInfo
                {
                    ControlId = "TestId",
                    WindowId = "Id",
                    WindowTitle = "Title"
                });

            if (this.dragMgr.IsDragInProgress)
                return;

            (listView.ItemsSource as ObservableCollection<ScriptInfoItem>).Add(item);
        }

        private void OnListViewItemDoubleClick(object sender, MouseButtonEventArgs e)
        {
            var item = (sender as ListView).SelectedItem;
            if (item != null && item is ScriptInfoItem scriptInfoItem)
            {

                ControlInfoWindow controlInfoWindow = new ControlInfoWindow()
                {
                    DataContext = scriptInfoItem.ControlInfo,
                   //Owner = this,
                };
                controlInfoWindow.Show();
            }
        }

    }
}