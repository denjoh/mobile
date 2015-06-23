﻿using System;
using CoreGraphics;
using UIKit;
using Toggl.Ross.Theme;

namespace Toggl.Ross.Views
{
    public class TableViewRefreshView : UIRefreshControl
    {
        const int sideSize = 5;

        public TableViewRefreshView ()
        {
            this.Apply (Style.TableViewHeader);
        }

        public void AdaptToTableView (UITableView tableView)
        {
            var tableFrame = tableView.Frame;
            tableFrame.Y = -tableFrame.Size.Height;
            var view = new UIView (tableFrame);
            view.BackgroundColor = BackgroundColor;
            tableView.InsertSubviewBelow (view, this);
        }
    }
}
