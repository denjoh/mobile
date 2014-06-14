using System;
using System.Linq.Expressions;
using Toggl.Phoebe.Data.DataObjects;

namespace Toggl.Phoebe.Data.NewModels
{
    public class TimeEntryTagModel : Model<TimeEntryTagData>
    {
        private static string GetPropertyName<T> (Expression<Func<TimeEntryTagModel, T>> expr)
        {
            return expr.ToPropertyName ();
        }

        public static new readonly string PropertyId = Model<TimeEntryTagData>.PropertyId;
        public static readonly string PropertyTimeEntry = GetPropertyName (m => m.TimeEntry);
        public static readonly string PropertyTag = GetPropertyName (m => m.Tag);

        public TimeEntryTagModel ()
        {
        }

        public TimeEntryTagModel (TimeEntryTagData data) : base (data)
        {
        }

        public TimeEntryTagModel (Guid id) : base (id)
        {
        }

        protected override TimeEntryTagData Duplicate (TimeEntryTagData data)
        {
            return new TimeEntryTagData (data);
        }

        protected override void OnBeforeSave ()
        {
            if (Data.TimeEntryId == Guid.Empty) {
                throw new ValidationException ("TimeEntry must be set for TimeEntryTag model.");
            }
            if (Data.TagId == Guid.Empty) {
                throw new ValidationException ("Tag must be set for TimeEntryTag model.");
            }
        }

        protected override void DetectChangedProperties (TimeEntryTagData oldData, TimeEntryTagData newData)
        {
            base.DetectChangedProperties (oldData, newData);
            if (oldData.TimeEntryId != newData.TimeEntryId || timeEntry.HasChanged)
                OnPropertyChanged (PropertyTimeEntry);
            if (oldData.TagId != newData.TagId || tag.HasChanged)
                OnPropertyChanged (PropertyTag);
        }

        private ForeignRelation<TimeEntryModel> timeEntry;
        private ForeignRelation<TagModel> tag;

        protected override void InitializeRelations ()
        {
            base.InitializeRelations ();

            timeEntry = new ForeignRelation<TimeEntryModel> () {
                ShouldLoad = EnsureLoaded,
                Factory = id => new TimeEntryModel (id),
                Changed = id => MutateData (data => data.TimeEntryId = id.Value),
            };

            tag = new ForeignRelation<TagModel> () {
                ShouldLoad = EnsureLoaded,
                Factory = id => new TagModel (id),
                Changed = id => MutateData (data => data.TagId = id.Value),
            };
        }

        [ForeignRelation]
        public TimeEntryModel TimeEntry {
            get { return timeEntry.Get (Data.TimeEntryId); }
            set { timeEntry.Set (value); }
        }

        [ForeignRelation]
        public TagModel Tag {
            get { return tag.Get (Data.TagId); }
            set { tag.Set (value); }
        }

        public static implicit operator TimeEntryTagModel (TimeEntryTagData data)
        {
            return new TimeEntryTagModel (data);
        }

        public static implicit operator TimeEntryTagData (TimeEntryTagModel model)
        {
            return model.Data;
        }
    }
}
