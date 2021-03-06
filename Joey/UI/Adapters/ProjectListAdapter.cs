﻿using System;
using System.Collections.Generic;
using System.Linq;
using Android.Content;
using Android.Graphics;
using Android.Support.V7.Widget;
using Android.Views;
using Android.Widget;
using Toggl.Joey.UI.Utils;
using Toggl.Joey.UI.Views;
using Toggl.Phoebe.Data.Models;
using Toggl.Phoebe.ViewModels;
using XPlatUtils;

namespace Toggl.Joey.UI.Adapters
{
    public class ProjectListAdapter : RecyclerCollectionDataAdapter<ICommonData>
    {
        protected const int ViewTypeProject = ViewTypeContent;
        protected const int ViewTypeClient = ViewTypeContent + 1;
        protected const int ViewTypeTask = ViewTypeContent + 2;
        protected const int ViewTypeTopProjects = ViewTypeContent + 3;

        protected ProjectsCollection collectionView;
        protected ProjectListVM viewModel;
        public Action<ICommonData> HandleItemSelection { get; set; }

        public ProjectListAdapter(RecyclerView owner, ProjectListVM viewModel) : base(owner, viewModel.ProjectList)
        {
            this.viewModel = viewModel;
            this.collectionView = viewModel.ProjectList;
        }

        protected override RecyclerView.ViewHolder GetViewHolder(ViewGroup parent, int viewType)
        {
            View view;
            RecyclerView.ViewHolder holder;
            var inflater = LayoutInflater.FromContext(ServiceContainer.Resolve<Context> ());

            switch (viewType)
            {
                case ViewTypeTopProjects:
                    view = inflater.Inflate(Resource.Layout.ProjectListMostUsedList, parent, false);
                    holder = new TopProjectsHolder(this, view);
                    break;
                case ViewTypeClient:
                    view = inflater.Inflate(Resource.Layout.ProjectListClientItem, parent, false);
                    holder = new ClientItemHolder(view);
                    break;
                case ViewTypeTask:
                    view = inflater.Inflate(Resource.Layout.ProjectListTaskItem, parent, false);
                    holder = new TaskItemHolder(this, view);
                    break;
                default:
                    view = inflater.Inflate(Resource.Layout.ProjectListProjectItem, parent, false);
                    holder = new ProjectItemHolder(this, view);
                    break;
            }
            return holder;
        }

        protected override void BindHolder(RecyclerView.ViewHolder holder, int position)
        {
            var viewType = GetItemViewType(position);

            if (viewType == ViewTypeTopProjects)
            {
                ((TopProjectsHolder)holder).Bind(viewModel.TopProjects);
            }
            else if (viewType == ViewTypeTask)
            {
                ((TaskItemHolder) holder).Bind((TaskData)GetItem(position));
            }
            else if (viewType == ViewTypeClient)
            {
                ((ClientItemHolder) holder).Bind(((ClientData)GetItem(position)).Name);
            }
            else if (viewType == ViewTypeProject)
            {
                var showClientName = collectionView.SortBy == ProjectsCollection.SortProjectsBy.Projects;
                ((ProjectItemHolder) holder).Bind((ProjectsCollection.SuperProjectData)GetItem(position), showClientName);
            }
        }

        public override int GetItemViewType(int position)
        {
            if (position == 0)
            {
                return ViewTypeTopProjects;
            }
            var type = base.GetItemViewType(position);

            if (type == ViewTypeLoaderPlaceholder)
            {
                return type;
            }
            var dataObject = GetItem(position);

            if (dataObject is ProjectsCollection.SuperProjectData)
            {
                return ViewTypeProject;
            }

            if (dataObject is ClientData)
            {
                return ViewTypeClient;
            }

            if (dataObject is TaskData)
            {
                return ViewTypeTask;
            }
            return type;
        }

        #region View holders

        public class TopProjectsHolder : RecyclerView.ViewHolder
        {
            const int maxNumProjects = 3;
            readonly TextView HeaderTextView;
            private IEnumerable<ProjectListVM.CommonProjectData> projectList;
            readonly LinearLayout ProjectsContainer;
            private ProjectListAdapter adapter;

            public TopProjectsHolder(ProjectListAdapter adapter, View root) : base(root)
            {
                this.adapter = adapter;
                HeaderTextView = root.FindViewById<TextView>(Resource.Id.HeaderTextView).SetFont(Font.RobotoMedium);
                ProjectsContainer = root.FindViewById<LinearLayout>(Resource.Id.ProjectsContainer);
                InflateProjectViews();
            }

            public void Bind(IEnumerable<ProjectListVM.CommonProjectData> projectList)
            {
                var count = projectList.Count();
                HeaderTextView.Visibility = count == 0 ? ViewStates.Gone : ViewStates.Visible;
                ProjectsContainer.Visibility = count == 0 ? ViewStates.Gone : ViewStates.Visible;
                this.projectList = projectList;

                View view;
                TextView projectTextView;
                TextView clientTextView;
                TextView taskTextView;
                View colorView;
                Color color;
                ProjectListVM.CommonProjectData project;

                for (var i = 0; i < ProjectsContainer.ChildCount; i++)
                {
                    view = ProjectsContainer.GetChildAt(i);

                    if (i < count)
                    {
                        view.Visibility = ViewStates.Visible;
                        project = projectList.ElementAt(i);
                        projectTextView = view.FindViewById<TextView>(Resource.Id.ProjectTextView);
                        clientTextView = view.FindViewById<TextView>(Resource.Id.ClientTextView);
                        taskTextView = view.FindViewById<TextView>(Resource.Id.TaskTextView);
                        colorView = view.FindViewById<View>(Resource.Id.ColorView);

                        projectTextView.Text = project.Name;
                        clientTextView.Visibility = string.IsNullOrEmpty(project.ClientName) ? ViewStates.Gone : ViewStates.Visible;
                        clientTextView.Text = project.ClientName;
                        taskTextView.Text = project.Task == null ? string.Empty : project.Task.Name;

                        color = Color.ParseColor(ProjectData.HexColors[project.Color % ProjectData.HexColors.Length]);
                        colorView.SetBackgroundColor(color);
                        projectTextView.SetTextColor(color);
                    }
                    else
                    {
                        view.Visibility = ViewStates.Gone;
                    }
                }
            }

            private void InflateProjectViews()
            {
                var inflater = LayoutInflater.FromContext(ServiceContainer.Resolve<Context>());
                View view;
                // inflate views
                for (var i = 0; i < maxNumProjects; i++)
                {
                    view = inflater.Inflate(Resource.Layout.ProjectListUsedProjectItem, null, false);
                    view.Click += OnClickProjectView;
                    ProjectsContainer.AddView(view);
                }
            }

            private void OnClickProjectView(object sender, EventArgs e)
            {
                var index = ProjectsContainer.IndexOfChild((View)sender);
                if (index < projectList.Count())
                {
                    var project = projectList.ElementAt(index);
                    if (project.Task == null)
                    {
                        adapter.HandleItemSelection.Invoke(project);
                    }
                    else
                    {
                        adapter.HandleItemSelection.Invoke(project.Task);
                    }
                }
            }
        }

        protected class ProjectItemHolder : RecyclerView.ViewHolder, View.IOnClickListener
        {
            protected View ColorView { get; private set; }
            protected TextView ProjectTextView { get; private set; }
            protected TextView ClientTextView { get; private set; }
            protected ImageButton TasksButton { get; private set; }
            protected ImageView TasksImageView { get; private set; }

            private ProjectListAdapter adapter;
            private ProjectsCollection.SuperProjectData projectData;

            public ProjectItemHolder(ProjectListAdapter adapter, View root) : base(root)
            {
                this.adapter = adapter;
                ColorView = root.FindViewById<View> (Resource.Id.ColorView);
                ProjectTextView = root.FindViewById<TextView> (Resource.Id.ProjectTextView).SetFont(Font.Roboto);
                ClientTextView = root.FindViewById<TextView> (Resource.Id.ClientTextView).SetFont(Font.RobotoLight);
                TasksButton = root.FindViewById<ImageButton> (Resource.Id.TasksButton);
                TasksButton.Click += (sender, e) => adapter.collectionView.AddTasks(projectData);
                root.SetOnClickListener(this);
            }

            public void OnClick(View v)
            {
                if (v == TasksButton)
                {
                    return;
                }

                if (adapter.HandleItemSelection != null)
                {
                    adapter.HandleItemSelection.Invoke(projectData);
                }
            }

            public void Bind(ProjectsCollection.SuperProjectData projectData, bool showClient)
            {
                this.projectData = projectData;

                if (projectData.IsEmpty)
                {
                    var emptyColor = ColorView.Resources.GetColor(Resource.Color.dark_gray_text);
                    ColorView.SetBackgroundColor(emptyColor);
                    ProjectTextView.SetTextColor(emptyColor);
                    ClientTextView.SetTextColor(emptyColor);

                    ProjectTextView.SetText(Resource.String.ProjectsNoProject);
                    ClientTextView.Visibility = ViewStates.Gone;
                    TasksButton.Visibility = ViewStates.Gone;
                    return;
                }

                var color = Color.ParseColor(ProjectData.HexColors [projectData.Color % ProjectData.HexColors.Length]);
                ColorView.SetBackgroundColor(color);
                ProjectTextView.SetTextColor(color);
                ClientTextView.SetTextColor(color);

                ProjectTextView.Text = projectData.Name;
                ClientTextView.Text = projectData.ClientName;
                ClientTextView.Visibility = showClient ? ViewStates.Visible : ViewStates.Gone;
                TasksButton.Visibility = projectData.TaskNumber > 0 ? ViewStates.Visible : ViewStates.Gone;
                TasksButton.Selected = false;
            }
        }

        protected class TaskItemHolder : RecyclerView.ViewHolder, View.IOnClickListener
        {
            private readonly ProjectListAdapter adapter;
            private readonly TextView taskTextView;
            private TaskData taskData;

            public TaskItemHolder(ProjectListAdapter adapter, View root) : base(root)
            {
                this.adapter = adapter;
                taskTextView = root.FindViewById<TextView> (Resource.Id.TaskTextView).SetFont(Font.RobotoLight);
                root.SetOnClickListener(this);
            }

            public void Bind(TaskData data)
            {
                taskData = data;
                taskTextView.Text = data.Name;
            }

            public void OnClick(View v)
            {
                if (adapter.HandleItemSelection != null)
                {
                    adapter.HandleItemSelection.Invoke(taskData);
                }
            }
        }

        [Shadow(ShadowAttribute.Mode.Top | ShadowAttribute.Mode.Bottom)]
        protected class ClientItemHolder : RecyclerView.ViewHolder
        {
            readonly TextView clientTextView;

            public ClientItemHolder(View root) : base(root)
            {
                clientTextView = root.FindViewById<TextView> (Resource.Id.ClientTextView).SetFont(Font.RobotoMedium);
            }

            public void Bind(string name)
            {
                if (string.IsNullOrEmpty(name))
                {
                    clientTextView.SetText(Resource.String.ProjectsNoClient);
                }
                else
                {
                    clientTextView.Text = name;
                }
            }
        }
        #endregion
    }
}

