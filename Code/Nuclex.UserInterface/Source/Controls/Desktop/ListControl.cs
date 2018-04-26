#region CPL License

/*
Nuclex Framework
Copyright (C) 2002-2010 Nuclex Development Labs

This library is free software; you can redistribute it and/or
modify it under the terms of the IBM Common Public License as
published by the IBM Corporation; either version 1.0 of the
License, or (at your option) any later version.

This library is distributed in the hope that it will be useful,
but WITHOUT ANY WARRANTY; without even the implied warranty of
MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the
IBM Common Public License for more details.

You should have received a copy of the IBM Common Public
License along with this library
*/

#endregion CPL License

using System;
using System.Collections.Generic;

using Microsoft.Xna.Framework;

using Nuclex.Input;
using Nuclex.Support.Collections;
using Nuclex.UserInterface.Input;
using Nuclex.UserInterface.Visuals.Flat;

namespace Nuclex.UserInterface.Controls.Desktop
{
    /// <summary>How the list lets the user select items</summary>
    public enum ListSelectionMode
    {
        /// <summary>The user is not allowed to select an item</summary>
        None,

        /// <summary>The user can select only one item</summary>
        Single,

        /// <summary>The user can pick any number of items</summary>
        Multi
    }

    /// <summary>List showing a sequence of items</summary>
    public class ListControl : Control, IFocusable
    {
        /// <summary>Triggered when the selected items in list have changed</summary>
        public event EventHandler SelectionChanged;

        /// <summary>Initializes a new list box control</summary>
        public ListControl()
        {
            items = new ObservableCollection<string>();
            items.Cleared += itemsCleared;
            items.ItemAdded += itemAdded;
            items.ItemRemoved += itemRemoved;

            selectedItems = new ObservableCollection<int>();
            selectedItems.Cleared += selectionCleared;
            selectedItems.ItemAdded += selectionAdded;
            selectedItems.ItemRemoved += selectionRemoved;

            slider = new VerticalSliderControl();
            slider.Bounds = new UniRectangle(
              new UniScalar(1.0f, -20.0f), new UniScalar(0.0f, 0.0f),
              new UniScalar(0.0f, 20.0f), new UniScalar(1.0f, 0.0f)
            );
            Children.Add(slider);

            _textPopup = new TextPopupControl();
            Children.Add(_textPopup);
        }

        /// <summary>How the user can select items in the list</summary>
        public ListSelectionMode SelectionMode
        {
            get { return selectionMode; }
            set { selectionMode = value; }
        }

        /// <summary>Slider the list uses to scroll through its items</summary>
        public VerticalSliderControl Slider
        {
            get { return slider; }
        }

        /// <summary>Items being displayed in the list</summary>
        public IList<string> Items
        {
            get { return items; }
        }

        /// <summary>Indices of the items current selected in the list</summary>
        public IList<int> SelectedItems
        {
            get { return selectedItems; }
        }

        private bool _arePopupsCustom;

        /// <summary>
        /// Indicates if popups were overridden
        /// </summary>
        public bool ArePopupsCustom
        {
            get { return _arePopupsCustom; }
            set
            {
                _arePopupsCustom = value;
                if (value)
                {
                    Popups = new List<string>();
                }
            }
        }

        private TextPopupControl _textPopup;

        /// <summary>Called when a mouse button has been pressed down</summary>
        /// <param name="button">Index of the button that has been pressed</param>
        /// <remarks>
        ///   If this method states that a mouse press is processed by returning
        ///   true, that means the control did something with it and the mouse press
        ///   should not be acted upon by any other listener.
        /// </remarks>
        protected override void OnMousePressed(MouseButtons button)
        {
            if (listRowLocator != null)
            {
                int row = listRowLocator.GetRow(
                  GetAbsoluteBounds(), slider.ThumbPosition, items.Count, mouseY
                );
                if ((row >= 0) && (row < items.Count))
                {
                    OnRowClicked(row);
                }
            }
        }

        /// <summary>Called when the user has clicked on a row in the list</summary>
        /// <param name="row">Row the user has clicked on</param>
        /// <remarks>
        ///   The default behavior of the list control in multi select mode is to
        ///   toggle items that are clicked between selected and unselected. If you
        ///   need different behavior (for example, dragging a selected region or
        ///   selecting sequences of items by holding the shift key), you can override
        ///   this method and handle the selection behavior yourself.
        /// </remarks>
        protected virtual void OnRowClicked(int row)
        {
            switch (selectionMode)
            {
                // The user isn't allowed to select items in the list
                case ListSelectionMode.None:
                    {
                        break;
                    }

                // Only a single item can be selected at a time
                case ListSelectionMode.Single:
                    {
                        if (selectedItems.Count == 1)
                        {
                            if (selectedItems[0] == row)
                            {
                                break; // do not fire the SelectionChanged event
                            }

                            selectedItems[0] = row;
                        }
                        else
                        {
                            selectedItems.Clear();
                            selectedItems.Add(row);
                        }
                        OnSelectionChanged();

                        break;
                    }

                // Any number of items can be selected
                case ListSelectionMode.Multi:
                    {
                        if (!selectedItems.Remove(row))
                        {
                            selectedItems.Add(row);
                        }
                        OnSelectionChanged();
                        break;
                    }
            }
        }

        /// <summary>Called when the mouse wheel has been rotated</summary>
        /// <param name="ticks">Number of ticks that the mouse wheel has been rotated</param>
        protected override void OnMouseWheel(float ticks)
        {
            const float ItemsPerTick = 1.0f;

            if (listRowLocator != null)
            {
                RectangleF bounds = GetAbsoluteBounds();

                float totalitems = items.Count;
                float itemsInView = bounds.Height / listRowLocator.GetRowHeight(bounds);
                float scrollableItems = totalitems - itemsInView;

                slider.ThumbPosition -= ItemsPerTick / scrollableItems * ticks;
                slider.ThumbPosition = MathHelper.Clamp(
                  slider.ThumbPosition, 0.0f, 1.0f
                );
            }
        }

        /// <summary>Called when the mouse position is updated</summary>
        /// <param name="x">X coordinate of the mouse cursor on the control</param>
        /// <param name="y">Y coordinate of the mouse cursor on the control</param>
        protected override void OnMouseMoved(float x, float y)
        {
            if (listRowLocator == null)
            {
                return;
            }

            mouseY = y;

            int row = listRowLocator.GetRow(
                GetAbsoluteBounds(), slider.ThumbPosition, items.Count, mouseY
                );

            if ((row >= 0) && (row < items.Count) && Graphics != null)
            {
                if (Graphics.MeasureString("list", new RectangleF(0, 0, 0, 0), Items[row]).Width > GetAbsoluteBounds().Width)
                {
                    _textPopup.Text = !ArePopupsCustom ? Items[row] : Popups[row];
                    _textPopup.Enabled = true;
                }
                else if (ArePopupsCustom)
                {
                    _textPopup.Text = Popups[row];
                    _textPopup.Enabled = true;
                }
                else
                {
                    _textPopup.Enabled = false;
                }
            }
            else
            {
                _textPopup.Enabled = false;
            }
        }

        /// <summary>
        ///   Called when the mouse has left the control and is no longer hovering over it
        /// </summary>
        protected override void OnMouseLeft()
        {
            base.OnMouseLeft();

            if (_textPopup != null)
            {
                _textPopup.Enabled = false;
            }
        }

        /// <summary>Called when the selected items in the list have changed</summary>
        protected virtual void OnSelectionChanged()
        {
            if (SelectionChanged != null)
            {
                SelectionChanged(this, EventArgs.Empty);
            }
        }

        /// <summary>Whether the control can currently obtain the input focus</summary>
        bool IFocusable.CanGetFocus
        {
            get { return true; }
        }

        /// <summary>Called when an item is removed from the items list</summary>
        /// <param name="sender">List the item has been removed from</param>
        /// <param name="arguments">Contains the item that has been removed</param>
        private void itemRemoved(object sender, ItemEventArgs<string> arguments)
        {
            updateSlider();
        }

        /// <summary>Called when an item is added to the items list</summary>
        /// <param name="sender">List the item has been added to</param>
        /// <param name="arguments">Contains the item that has been added</param>
        private void itemAdded(object sender, ItemEventArgs<string> arguments)
        {
            updateSlider();
        }

        /// <summary>Called when the items list is about to clear itself</summary>
        /// <param name="sender">Items list that is about to clear itself</param>
        /// <param name="arguments">Not used</param>
        private void itemsCleared(object sender, EventArgs arguments)
        {
            updateSlider();
        }

        /// <summary>Called when an entry is added to the list of selected items</summary>
        /// <param name="sender">List to which an item was added to</param>
        /// <param name="arguments">Contains the added item</param>
        private void selectionAdded(object sender, ItemEventArgs<int> arguments)
        {
            OnSelectionChanged();
        }

        /// <summary>
        ///   Called when an entry is removed from the list of selected items
        /// </summary>
        /// <param name="sender">List from which an item was removed</param>
        /// <param name="arguments">Contains the removed item</param>
        private void selectionRemoved(object sender, ItemEventArgs<int> arguments)
        {
            OnSelectionChanged();
        }

        /// <summary>Called when the selected items list is about to clear itself</summary>
        /// <param name="sender">List that is about to clear itself</param>
        /// <param name="arguments">Not Used</param>
        private void selectionCleared(object sender, EventArgs arguments)
        {
            OnSelectionChanged();
        }

        /// <summary>Updates the size and position of the list's slider</summary>
        private void updateSlider()
        {
            if ((Screen != null) && (listRowLocator != null))
            {
                RectangleF bounds = GetAbsoluteBounds();

                float totalitems = items.Count;
                float itemsInView = bounds.Height / listRowLocator.GetRowHeight(bounds);

                if (totalitems < itemsInView)
                {
                    Children.Remove(slider);
                }
                else
                {
                    if (!Children.Contains(slider))
                    {
                        Children.Add(slider);
                    }
                }

                slider.ThumbSize = Math.Min(1.0f, itemsInView / totalitems);
            }
        }

        /// <summary>
        /// Clears items
        /// </summary>
        public void Clear()
        {
            Items.Clear();
            SelectedItems.Clear();
        }

        /// <summary>
        ///   Can be set by renderers to enable selection of list items by mouse
        /// </summary>
        public IListRowLocator ListRowLocator
        {
            get { return listRowLocator; }
            set
            {
                if (value != listRowLocator)
                {
                    listRowLocator = value;
                    updateSlider();
                }
            }
        }

        /// <summary>
        ///   Row locator through which the list can detect which row the mouse has
        ///   been pressed down on
        /// </summary>
        private IListRowLocator listRowLocator;

        /// <summary>Last known Y coordinate of the mouse</summary>
        private float mouseY;

        /// <summary>How the list lets the user select from its items</summary>
        private ListSelectionMode selectionMode;

        /// <summary>Items contained in the list</summary>
        private ObservableCollection<string> items;

        public List<string> Popups { get; set; }

        /// <summary>Items currently selected in the list</summary>
        private ObservableCollection<int> selectedItems;

        /// <summary>Slider the lists uses to scroll through its items</summary>
        private VerticalSliderControl slider;

        // Never try this bald OOP standarts breaking at home kids
        // I need to access graphics to measure string, even if I know that I shouldn't
        public IFlatGuiGraphics Graphics;        
    }
} // namespace Nuclex.UserInterface.Controls.Desktop