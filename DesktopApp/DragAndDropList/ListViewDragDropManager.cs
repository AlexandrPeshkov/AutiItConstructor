using DesktopApp.Utils;
using Domain.Abstracts.Interfaces;
using System;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;

namespace DesktopApp.DragAndDropList
{
    public class ListViewDragDropManager<ItemType> where ItemType : class
    {
        bool canInitiateDrag;
        DragAdorner dragAdorner;
        double dragAdornerOpacity;
        int indexToSelect;
        ItemType itemUnderDragCursor;
        ListView listView;
        Point ptMouseDown;
        bool showDragAdorner;

        public ListViewDragDropManager()
        {
            canInitiateDrag = false;
            dragAdornerOpacity = 0.7;
            indexToSelect = -1;
            showDragAdorner = true;
        }

        public ListViewDragDropManager(ListView listView)
            : this()
        {
            this.ListView = listView;
        }

        public ListViewDragDropManager(ListView listView, double dragAdornerOpacity)
            : this(listView)
        {
            DragAdornerOpacity = dragAdornerOpacity;
        }

        public ListViewDragDropManager(ListView listView, bool showDragAdorner)
            : this(listView)
        {
            ShowDragAdorner = showDragAdorner;
        }

        public double DragAdornerOpacity
        {
            get { return dragAdornerOpacity; }
            set
            {
                if (IsDragInProgress)
                    throw new InvalidOperationException("Cannot set the DragAdornerOpacity property during a drag operation.");

                if (value < 0.0 || value > 1.0)
                    throw new ArgumentOutOfRangeException("DragAdornerOpacity", value, "Must be between 0 and 1.");

                dragAdornerOpacity = value;
            }
        }

        public bool IsDragInProgress { get; private set; }


        public ListView ListView
        {
            get { return listView; }
            set
            {
                if (IsDragInProgress)
                    throw new InvalidOperationException("Cannot set the ListView property during a drag operation.");

                if (listView != null)
                {

                    listView.PreviewMouseLeftButtonDown -= listView_PreviewMouseLeftButtonDown;
                    listView.PreviewMouseMove -= listView_PreviewMouseMove;
                    listView.DragOver -= listView_DragOver;
                    listView.DragLeave -= listView_DragLeave;
                    listView.DragEnter -= listView_DragEnter;
                    listView.Drop -= listView_Drop;
                }

                listView = value;

                if (listView != null)
                {
                    if (listView.AllowDrop == false)
                        listView.AllowDrop = true;

                    listView.PreviewMouseLeftButtonDown += listView_PreviewMouseLeftButtonDown;
                    listView.PreviewMouseMove += listView_PreviewMouseMove;
                    listView.DragOver += listView_DragOver;
                    listView.DragLeave += listView_DragLeave;
                    listView.DragEnter += listView_DragEnter;
                    listView.Drop += listView_Drop;
                }
            }
        }

        public event EventHandler<ProcessDropEventArgs<ItemType>> ProcessDrop;

        public bool ShowDragAdorner
        {
            get { return this.showDragAdorner; }
            set
            {
                if (this.IsDragInProgress)
                    throw new InvalidOperationException("Cannot set the ShowDragAdorner property during a drag operation.");

                this.showDragAdorner = value;
            }
        }

        void listView_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (this.IsMouseOverScrollbar)
            {

                this.canInitiateDrag = false;
                return;
            }

            int index = this.IndexUnderDragCursor;
            this.canInitiateDrag = index > -1;

            if (this.canInitiateDrag)
            {
                this.ptMouseDown = MouseUtilities.GetMousePosition(this.listView);
                this.indexToSelect = index;
            }
            else
            {
                this.ptMouseDown = new Point(-10000, -10000);
                this.indexToSelect = -1;
            }
        }

        void listView_PreviewMouseMove(object sender, MouseEventArgs e)
        {
            if (!this.CanStartDragOperation)
                return;

            if (this.listView.SelectedIndex != this.indexToSelect)
                this.listView.SelectedIndex = this.indexToSelect;


            if (this.listView.SelectedItem == null)
                return;

            ListViewItem itemToDrag = this.GetListViewItem(this.listView.SelectedIndex);
            if (itemToDrag == null)
                return;

            AdornerLayer adornerLayer = this.ShowDragAdornerResolved ? this.InitializeAdornerLayer(itemToDrag) : null;

            this.InitializeDragOperation(itemToDrag);
            this.PerformDragOperation();
            this.FinishDragOperation(itemToDrag, adornerLayer);
        }
        void listView_DragOver(object sender, DragEventArgs e)
        {
            e.Effects = DragDropEffects.Move;

            if (this.ShowDragAdornerResolved)
                this.UpdateDragAdornerLocation();

            int index = this.IndexUnderDragCursor;
            ItemUnderDragCursor = index < 0 ? null : this.ListView.Items[index] as ItemType;

        }

        void listView_DragLeave(object sender, DragEventArgs e)
        {
            if (!this.IsMouseOver(this.listView))
            {
                if (this.ItemUnderDragCursor != null)
                    this.ItemUnderDragCursor = null;

                if (this.dragAdorner != null)
                    this.dragAdorner.Visibility = Visibility.Collapsed;
            }
        }

        void listView_DragEnter(object sender, DragEventArgs e)
        {
            if (this.dragAdorner != null && this.dragAdorner.Visibility != Visibility.Visible)
            {
                this.UpdateDragAdornerLocation();
                this.dragAdorner.Visibility = Visibility.Visible;
            }
        }

        void listView_Drop(object sender, DragEventArgs e)
        {
            if (this.ItemUnderDragCursor != null)
                this.ItemUnderDragCursor = null;

            e.Effects = DragDropEffects.None;

            if (!e.Data.GetDataPresent(typeof(ItemType)))
                return;

            ItemType data = e.Data.GetData(typeof(ItemType)) as ItemType;
            if (data == null)
                return;

            ObservableCollection<ItemType> itemsSource = this.listView.ItemsSource as ObservableCollection<ItemType>;
            if (itemsSource == null)
                throw new Exception(
                    "A ListView managed by ListViewDragManager must have its ItemsSource set to an ObservableCollection<ItemType>.");

            int oldIndex = itemsSource.IndexOf(data);
            int newIndex = this.IndexUnderDragCursor;

            if (newIndex < 0)
            {
                if (itemsSource.Count == 0)
                    newIndex = 0;

                else if (oldIndex < 0)
                    newIndex = itemsSource.Count;

                else
                    return;
            }


            if (oldIndex == newIndex)
                return;

            if (this.ProcessDrop != null)
            {

                ProcessDropEventArgs<ItemType> args = new ProcessDropEventArgs<ItemType>(itemsSource, data, oldIndex, newIndex, e.AllowedEffects);
                this.ProcessDrop(this, args);
                e.Effects = args.Effects;
            }
            else
            {
                if (oldIndex > -1)
                    itemsSource.Move(oldIndex, newIndex);
                else
                    itemsSource.Insert(newIndex, data);

                e.Effects = DragDropEffects.Move;
            }
        }

        bool CanStartDragOperation
        {
            get
            {
                if (Mouse.LeftButton != MouseButtonState.Pressed)
                    return false;

                if (!this.canInitiateDrag)
                    return false;

                if (this.indexToSelect == -1)
                    return false;

                if (!this.HasCursorLeftDragThreshold)
                    return false;

                return true;
            }
        }

        void FinishDragOperation(ListViewItem draggedItem, AdornerLayer adornerLayer)
        {

            ListViewItemDragState.SetIsBeingDragged(draggedItem, false);

            this.IsDragInProgress = false;

            if (this.ItemUnderDragCursor != null)
                this.ItemUnderDragCursor = null;


            if (adornerLayer != null)
            {
                adornerLayer.Remove(this.dragAdorner);
                this.dragAdorner = null;
            }
        }

        ListViewItem GetListViewItem(int index)
        {
            if (this.listView.ItemContainerGenerator.Status != GeneratorStatus.ContainersGenerated)
                return null;

            return this.listView.ItemContainerGenerator.ContainerFromIndex(index) as ListViewItem;
        }

        ListViewItem GetListViewItem(ItemType dataItem)
        {
            if (this.listView.ItemContainerGenerator.Status != GeneratorStatus.ContainersGenerated)
                return null;

            return this.listView.ItemContainerGenerator.ContainerFromItem(dataItem) as ListViewItem;
        }

        bool HasCursorLeftDragThreshold
        {
            get
            {
                if (this.indexToSelect < 0)
                    return false;

                ListViewItem item = this.GetListViewItem(this.indexToSelect);
                Rect bounds = VisualTreeHelper.GetDescendantBounds(item);
                Point ptInItem = this.listView.TranslatePoint(this.ptMouseDown, item);

                double topOffset = Math.Abs(ptInItem.Y);
                double btmOffset = Math.Abs(bounds.Height - ptInItem.Y);
                double vertOffset = Math.Min(topOffset, btmOffset);

                double width = SystemParameters.MinimumHorizontalDragDistance * 2;
                double height = Math.Min(SystemParameters.MinimumVerticalDragDistance, vertOffset) * 2;
                Size szThreshold = new Size(width, height);

                Rect rect = new Rect(this.ptMouseDown, szThreshold);
                rect.Offset(szThreshold.Width / -2, szThreshold.Height / -2);
                Point ptInListView = MouseUtilities.GetMousePosition(this.listView);
                return !rect.Contains(ptInListView);
            }
        }

        int IndexUnderDragCursor
        {
            get
            {
                int index = -1;
                for (int i = 0; i < this.listView.Items.Count; ++i)
                {
                    ListViewItem item = this.GetListViewItem(i);
                    if (this.IsMouseOver(item))
                    {
                        index = i;
                        break;
                    }
                }
                return index;
            }
        }

        AdornerLayer InitializeAdornerLayer(ListViewItem itemToDrag)
        {

            VisualBrush brush = new VisualBrush(itemToDrag);
            this.dragAdorner = new DragAdorner(this.listView, itemToDrag.RenderSize, brush);
            this.dragAdorner.Opacity = this.DragAdornerOpacity;
            AdornerLayer layer = AdornerLayer.GetAdornerLayer(this.listView);
            layer.Add(dragAdorner);
            this.ptMouseDown = MouseUtilities.GetMousePosition(this.listView);

            return layer;
        }


        void InitializeDragOperation(ListViewItem itemToDrag)
        {
            this.IsDragInProgress = true;
            this.canInitiateDrag = false;
            ListViewItemDragState.SetIsBeingDragged(itemToDrag, true);
        }

        bool IsMouseOver(Visual target)
        {
            Rect bounds = VisualTreeHelper.GetDescendantBounds(target);
            Point mousePos = MouseUtilities.GetMousePosition(target);
            return bounds.Contains(mousePos);
        }

        bool IsMouseOverScrollbar
        {
            get
            {
                Point ptMouse = MouseUtilities.GetMousePosition(this.listView);
                HitTestResult res = VisualTreeHelper.HitTest(this.listView, ptMouse);
                if (res == null)
                    return false;

                DependencyObject depObj = res.VisualHit;
                while (depObj != null)
                {
                    if (depObj is ScrollBar)
                        return true;

                    if (depObj is Visual || depObj is System.Windows.Media.Media3D.Visual3D)
                        depObj = VisualTreeHelper.GetParent(depObj);
                    else
                        depObj = LogicalTreeHelper.GetParent(depObj);
                }

                return false;
            }
        }

        ItemType ItemUnderDragCursor
        {
            get { return this.itemUnderDragCursor; }
            set
            {
                if (this.itemUnderDragCursor == value)
                    return;


                for (int i = 0; i < 2; ++i)
                {
                    if (i == 1)
                        this.itemUnderDragCursor = value;

                    if (this.itemUnderDragCursor != null)
                    {
                        ListViewItem listViewItem = this.GetListViewItem(this.itemUnderDragCursor);
                        if (listViewItem != null)
                            ListViewItemDragState.SetIsUnderDragCursor(listViewItem, i == 1);
                    }
                }
            }
        }

        void PerformDragOperation()
        {
            ItemType selectedItem = this.listView.SelectedItem as ItemType;
            DragDropEffects allowedEffects = DragDropEffects.Move | DragDropEffects.Move | DragDropEffects.Link;
            if (DragDrop.DoDragDrop(this.listView, selectedItem, allowedEffects) != DragDropEffects.None)
            {
                this.listView.SelectedItem = selectedItem;
            }
        }

        bool ShowDragAdornerResolved
        {
            get { return this.ShowDragAdorner && this.DragAdornerOpacity > 0.0; }
        }

        void UpdateDragAdornerLocation()
        {
            if (this.dragAdorner != null)
            {
                Point ptCursor = MouseUtilities.GetMousePosition(this.ListView);

                double left = ptCursor.X - this.ptMouseDown.X;
                ListViewItem itemBeingDragged = this.GetListViewItem(this.indexToSelect);
                Point itemLoc = itemBeingDragged.TranslatePoint(new Point(0, 0), this.ListView);
                double top = itemLoc.Y + ptCursor.Y - this.ptMouseDown.Y;

                this.dragAdorner.SetOffsets(left, top);
            }
        }
    }

    public static class ListViewItemDragState
    {
        public static readonly DependencyProperty IsBeingDraggedProperty =
            DependencyProperty.RegisterAttached(
                "IsBeingDragged",
                typeof(bool),
                typeof(ListViewItemDragState),
                new UIPropertyMetadata(false));


        public static bool GetIsBeingDragged(ListViewItem item)
        {
            return (bool)item.GetValue(IsBeingDraggedProperty);
        }


        internal static void SetIsBeingDragged(ListViewItem item, bool value)
        {
            item.SetValue(IsBeingDraggedProperty, value);
        }


        public static readonly DependencyProperty IsUnderDragCursorProperty =
            DependencyProperty.RegisterAttached(
                "IsUnderDragCursor",
                typeof(bool),
                typeof(ListViewItemDragState),
                new UIPropertyMetadata(false));


        public static bool GetIsUnderDragCursor(ListViewItem item)
        {
            return (bool)item.GetValue(IsUnderDragCursorProperty);
        }

        internal static void SetIsUnderDragCursor(ListViewItem item, bool value)
        {
            item.SetValue(IsUnderDragCursorProperty, value);
        }

    }

    public class ProcessDropEventArgs<ItemType> : EventArgs where ItemType : class
    {
        internal ProcessDropEventArgs(
            ObservableCollection<ItemType> itemsSource,
            ItemType dataItem,
            int oldIndex,
            int newIndex,
            DragDropEffects allowedEffects)
        {
            ItemsSource = itemsSource;
            DataItem = dataItem;
            OldIndex = oldIndex;
            NewIndex = newIndex;
            AllowedEffects = allowedEffects;
        }


        public ObservableCollection<ItemType> ItemsSource { get; }

        public ItemType DataItem { get; }

        public int OldIndex { get; }

        public int NewIndex { get; }

        public DragDropEffects AllowedEffects { get; } = DragDropEffects.None;

        public DragDropEffects Effects { get; set; } = DragDropEffects.None;
    }

}