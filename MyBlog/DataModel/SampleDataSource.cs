using System;
using System.Linq;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using Windows.ApplicationModel.Resources.Core;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Imaging;
using System.Collections.Specialized;

// The data model defined by this file serves as a representative example of a strongly-typed
// model that supports notification when members are added, removed, or modified.  The property
// names chosen coincide with data bindings in the standard item templates.
//
// Applications may use this model as a starting point and build on it, or discard it entirely and
// replace it with something appropriate to their needs.

namespace MyBlog.Data
{
    /// <summary>
    /// Base class for <see cref="SampleDataItem"/> and <see cref="SampleDataGroup"/> that
    /// defines properties common to both.
    /// </summary>
    [Windows.Foundation.Metadata.WebHostHidden]
    public abstract class SampleDataCommon : MyBlog.Common.BindableBase
    {
        private static Uri _baseUri = new Uri("ms-appx:///");

        public SampleDataCommon(String uniqueId, String title, String subtitle, String imagePath, String description)
        {
            this._uniqueId = uniqueId;
            this._title = title;
            this._subtitle = subtitle;
            this._description = description;
            this._imagePath = imagePath;
        }

        private string _uniqueId = string.Empty;
        public string UniqueId
        {
            get { return this._uniqueId; }
            set { this.SetProperty(ref this._uniqueId, value); }
        }

        private string _title = string.Empty;
        public string Title
        {
            get { return this._title; }
            set { this.SetProperty(ref this._title, value); }
        }

        private string _subtitle = string.Empty;
        public string Subtitle
        {
            get { return this._subtitle; }
            set { this.SetProperty(ref this._subtitle, value); }
        }

        private string _description = string.Empty;
        public string Description
        {
            get { return this._description; }
            set { this.SetProperty(ref this._description, value); }
        }

        private ImageSource _image = null;
        private String _imagePath = null;
        public ImageSource Image
        {
            get
            {
                if (this._image == null && this._imagePath != null)
                {
                    this._image = new BitmapImage(new Uri(SampleDataCommon._baseUri, this._imagePath));
                }
                return this._image;
            }

            set
            {
                this._imagePath = null;
                this.SetProperty(ref this._image, value);
            }
        }

        public string ImagePath
        {
            get { return SampleDataCommon._baseUri + this._imagePath; }
        }

        public void SetImage(String path)
        {
            this._image = null;
            this._imagePath = path;
            this.OnPropertyChanged("Image");
        }

        public override string ToString()
        {
            return this.Title;
        }
    }

    /// <summary>
    /// Generic item data model.
    /// </summary>
    public class SampleDataItem : SampleDataCommon
    {
        public SampleDataItem(String uniqueId, String title, String subtitle, String imagePath, String description, String content, SampleDataGroup group)
            : base(uniqueId, title, subtitle, imagePath, description)
        {
            this._content = content;
            this._group = group;
        }

        private string _content = string.Empty;
        public string Content
        {
            get { return this._content; }
            set { this.SetProperty(ref this._content, value); }
        }

        private SampleDataGroup _group;
        public SampleDataGroup Group
        {
            get { return this._group; }
            set { this.SetProperty(ref this._group, value); }
        }
    }

    /// <summary>
    /// Generic group data model.
    /// </summary>
    public class SampleDataGroup : SampleDataCommon
    {
        public SampleDataGroup(String uniqueId, String title, String subtitle, String imagePath, String description)
            : base(uniqueId, title, subtitle, imagePath, description)
        {
            Items.CollectionChanged += ItemsCollectionChanged;
        }

        private void ItemsCollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            // Provides a subset of the full items collection to bind to from a GroupedItemsPage
            // for two reasons: GridView will not virtualize large items collections, and it
            // improves the user experience when browsing through groups with large numbers of
            // items.
            //
            // A maximum of 12 items are displayed because it results in filled grid columns
            // whether there are 1, 2, 3, 4, or 6 rows displayed

            switch (e.Action)
            {
                case NotifyCollectionChangedAction.Add:
                    if (e.NewStartingIndex < 12)
                    {
                        TopItems.Insert(e.NewStartingIndex,Items[e.NewStartingIndex]);
                        if (TopItems.Count > 12)
                        {
                            TopItems.RemoveAt(12);
                        }
                    }
                    break;
                case NotifyCollectionChangedAction.Move:
                    if (e.OldStartingIndex < 12 && e.NewStartingIndex < 12)
                    {
                        TopItems.Move(e.OldStartingIndex, e.NewStartingIndex);
                    }
                    else if (e.OldStartingIndex < 12)
                    {
                        TopItems.RemoveAt(e.OldStartingIndex);
                        TopItems.Add(Items[11]);
                    }
                    else if (e.NewStartingIndex < 12)
                    {
                        TopItems.Insert(e.NewStartingIndex, Items[e.NewStartingIndex]);
                        TopItems.RemoveAt(12);
                    }
                    break;
                case NotifyCollectionChangedAction.Remove:
                    if (e.OldStartingIndex < 12)
                    {
                        TopItems.RemoveAt(e.OldStartingIndex);
                        if (Items.Count >= 12)
                        {
                            TopItems.Add(Items[11]);
                        }
                    }
                    break;
                case NotifyCollectionChangedAction.Replace:
                    if (e.OldStartingIndex < 12)
                    {
                        TopItems[e.OldStartingIndex] = Items[e.OldStartingIndex];
                    }
                    break;
                case NotifyCollectionChangedAction.Reset:
                    TopItems.Clear();
                    while (TopItems.Count < Items.Count && TopItems.Count < 12)
                    {
                        TopItems.Add(Items[TopItems.Count]);
                    }
                    break;
            }
        }

        private ObservableCollection<SampleDataItem> _items = new ObservableCollection<SampleDataItem>();
        public ObservableCollection<SampleDataItem> Items
        {
            get { return this._items; }
        }

        private ObservableCollection<SampleDataItem> _topItem = new ObservableCollection<SampleDataItem>();
        public ObservableCollection<SampleDataItem> TopItems
        {
            get {return this._topItem; }
        }
    }

    /// <summary>
    /// Creates a collection of groups and items with hard-coded content.
    /// 
    /// SampleDataSource initializes with placeholder data rather than live production
    /// data so that sample data is provided at both design-time and run-time.
    /// </summary>
    public sealed class SampleDataSource
    {
        private static SampleDataSource _sampleDataSource = new SampleDataSource();

        private ObservableCollection<SampleDataGroup> _allGroups = new ObservableCollection<SampleDataGroup>();
        public ObservableCollection<SampleDataGroup> AllGroups
        {
            get { return this._allGroups; }
        }

        public static IEnumerable<SampleDataGroup> GetGroups(string uniqueId)
        {
            if (!uniqueId.Equals("AllGroups")) throw new ArgumentException("Only 'AllGroups' is supported as a collection of groups");
            
            return _sampleDataSource.AllGroups;
        }

        public static SampleDataGroup GetGroup(string uniqueId)
        {
            // Simple linear search is acceptable for small data sets
            var matches = _sampleDataSource.AllGroups.Where((group) => group.UniqueId.Equals(uniqueId));
            if (matches.Count() == 1) return matches.First();
            return null;
        }

        public static SampleDataItem GetItem(string uniqueId)
        {
            // Simple linear search is acceptable for small data sets
            var matches = _sampleDataSource.AllGroups.SelectMany(group => group.Items).Where((item) => item.UniqueId.Equals(uniqueId));
            if (matches.Count() == 1) return matches.First();
            return null;
        }

        public SampleDataSource()
        {
            String ITEM_CONTENT = String.Format("Item Content: {0}\n\n{0}\n\n{0}\n\n{0}\n\n{0}\n\n{0}\n\n{0}",
                        "Curabitur class aliquam vestibulum nam curae maecenas sed integer cras phasellus suspendisse quisque donec dis praesent accumsan bibendum pellentesque condimentum adipiscing etiam consequat vivamus dictumst aliquam duis convallis scelerisque est parturient ullamcorper aliquet fusce suspendisse nunc hac eleifend amet blandit facilisi condimentum commodo scelerisque faucibus aenean ullamcorper ante mauris dignissim consectetuer nullam lorem vestibulum habitant conubia elementum pellentesque morbi facilisis arcu sollicitudin diam cubilia aptent vestibulum auctor eget dapibus pellentesque inceptos leo egestas interdum nulla consectetuer suspendisse adipiscing pellentesque proin lobortis sollicitudin augue elit mus congue fermentum parturient fringilla euismod feugiat");

            var group1 = new SampleDataGroup("Group-1",
                    "Theory",
                    "Software is Math",
                    "Assets/Theory.png",
                    "Every piece of code is a theorem. It is a sequence of logical conclusions, each based on the one before, leading up to desired behavior. To validate that behavior, you need to prove the theorem. Even though most compilers don't prove those theorems for you, they can still provide some assistance.");
            group1.Items.Add(new SampleDataItem("Group-1-Item-1",
                    "Degrees of Freedom",
                    "Limit the number moving parts",
                    "Assets/saddle.gif",
                    "Understanding the degrees of freedom in the software helps to create a maintainable design. You know what is under the user's direct control, and what is affected indirectly. You know which values must be stored, and which must be calculated. And when you add new features in the future, you can add new degrees of freedom without affecting the previous ones.",
                    "A mathematical model of a problem is written with equations and unknowns. You can think of the number of unknowns as representing the number of dimensions in the problem. If there are two, it can be represented on a plane. Three and it appears floating in space. Usually there are more, making it more a difficult to visualize, multi-dimensional volume.\n\n" +
                    "The equations slice through the volume. One equation with three unknowns makes a two-dimensional sheet that flaps in the volume. Two equations with three unknowns form a thread that curves through the system. The thread is not necessarily a straight line; it can swoop through in any of the three directions like a roller coaster. But if you were on the thread you could only walk forward or backward, so the thread is really one-dimensional.\n\n" +
                    "The number of unknowns minus the number of equations gives us the degrees of freedom. So that three-dimensional roller coaster with two equations has only one degree of freedom. If you were to start at the beginning and roll along the track, you could number each point as a distance from the start. That distance will have little to do with the straight-line proximity to the starting point, but since you can't leave the track it doesn't really matter.\n\n" +
                    "The equations that keep us on the track are constraints. If we have more equations than unknowns, the problem is over constrained. If we have fewer, the problem is under constrained and we have some degrees of freedom. If they are equal, then we have a Goldilocks problem: it's just right. We can usually find the one right answer to a Goldilocks problem. But software is not about finding one answer, it's about getting work done. And to get work done we need a little wiggle room. We need degrees of freedom.",
                    group1));
            group1.Items.Add(new SampleDataItem("Group-1-Item-2",
                    "Prerequisites",
                    "One thing happens before another",
                    "Assets/Prerequisites.png",
                    "A simple rearrangement of interfaces can turn a difficult-to-enforce API into one that the compiler can prove. Just recognize which methods are prerequisites of others, and use their returns to call the successors.",
                    "We often have to call methods in a certain order. One method is a prerequisite, the other its successor. For example, we need to validate a shopping cart before we check out. Typically, this is hard to prove.\n\n" +
                    "The contract is clear, but how do we prove that the caller is following the rules? One way is to keep an internal flag. Set it in Validate(), and check it in Checkout(). If the flag is false, throw an exception.\n\n" +
                    "While this technique works, it is not ideal. It is really no different from a guard clause. It's just a different form of defensive programming. Besides, exceptions are for problems that occur even in well-written software. Calling Checkout() without calling Validate() is a defect, and should never happen at all.",
                    group1));
            group1.Items.Add(new SampleDataItem("Group-1-Item-3",
                    "Immutability",
                    "Reason about things that don't change",
                    "Assets/Immutability.jpg",
                    "Don't overload the constructor. Don't use it for convenience. Don't initialize mutable properties. When a constructor is used only to set required and immutable properties, the intent is clear, and the proof is easy.",
                    "The constructor has a very powerful contract, and one that the compiler proves. The constructor is called once and only once.\n\n" +
                    "We can use this promise to prove some very useful things. We can prove that required properties are set. We can prove that A happens before B (as we can with other prerequisite techniques). But more strongly, we can prove that A does not happen after B.\n\n" +
                    "A constructor has to be called. There is no other way to get an instance of an object. If there are any required properties, they should be constructor parameters. Otherwise, there is no way to prove that they've been set.\n\n" +
                    "We can prove that the user requesting the report and the company for which the report is requested are specified. The filter parameters are optional.",
                    group1));
            group1.Items.Add(new SampleDataItem("Group-1-Item-4",
                    "Closure",
                    "Context as part of a function",
                    "Assets/Closure.png",
                    "Closure is a useful mechanism for writing provable software. Whether we use it as a language feature or simply as a pattern, it allows us to bring values into a function or object. We can then prove interesting things about those values.",
                    "When I say \"closure\", I'm talking about two things. First, there's the language feature. Second, there's the concept. Either way, it is fundamental to proving software.\n\n" +
                    "As a language feature, closure is the ability to bring variables into scope and encapsulate them within a function. Different languages support this feature in different ways. In C#, it is supported in lambdas and anonymous delegates.\n\n" +
                    "The parameter orderId is available within the lambda, even though the lambda executes outside of the scope of this method.\n\n" +
                    "As of Java 6, closure is supported only by anonymous inner classes. It is slated for becoming a first-class language feature in Java 7.\n\n" +
                    "Again, the orderId parameter is available within the anonymous inner class. It will be set to the the correct value even though Predicate.where() is called by code not located in the method. In Java, the variables to be enclosed must be marked as \"final\" to ensure that they are not changed after the closure is created.\n\n" +
                    "As a concept, closure can be used in several places. It doesn't take an explicit language feature to take advantage of the concept. In fact, the Factory Method pattern is often used to perform closure.",
                    group1));
            group1.Items.Add(new SampleDataItem("Group-1-Item-5",
                    "Entities, Values, Facts, and Projections",
                    "Not all objects are created equal",
                    "Assets/Projection.jpg",
                    "While we as an industry have found the distinction between entities and values to be necessary, we have not found it to be sufficient. Problems still arise in real systems when we have just these two classifications.",
                    "No model is perfect, including the model of software behavior that we know as Object-Oriented. One of the most significant flaws in OO thinking is that everything is an object.\n\n" +
                    "OO has certainly brought us forward in reasoning about machines. During that time, we’ve learned to model identity, interfaces, ownership, contracts, and many other useful concepts. We’ve also learned the drawbacks of shared state, remote objects, and persistence mapping, just to name a few challenges.\n\n" +
                    "The original definitions of Object-Orientation do not necessarily insist that everything is an object. The founders simply defined the behavior of an object in software. Nevertheless, modern OO languages -- like Java, C#, and Ruby -- encourage us to make everything an object. If you ask an average developer if everything is an object, he will most likely say yes.\n\n" +
                    "Domain Driven Design draws a line between entities and values. This distinction predates DDD, but the practitioners (most notably Eric Evans) are very explicit on this point. Entities have identity; values do not.",
                    group1));
            this.AllGroups.Add(group1);

            var group2 = new SampleDataGroup("Group-2",
                    "Practice",
                    "Sample problems to help you practice Q.E.D. coding techniques.",
                    "Assets/Practice.jpg",
                    "Sample problems to help you practice Q.E.D. coding techniques.");
            group2.Items.Add(new SampleDataItem("Group-2-Item-1",
                    "Watch Movement",
                    "Prove the number of connections",
                    "Assets/WatchMovement.jpg",
                    "Given a 20 jewel watch movement, find the number of connections between gears. Prove your answer.",
                    "In Q.E.D. Hour, we will be proving things about our code. To get the group started in thinking about proof, I assigned some homework. It has to do with a watch. The mechanism inside of a wristwatch is known as a movement. It has many moving parts, but all of those parts work together in unison. It measures only one thing. It has only one degree of freedom. The secret to why the movement works is not in the parts. It is in the connections between the parts, where a connection is the point at which two moving parts touch. Which brings us to our proof. Given: A movement with 20 moving parts having one degree of freedom. Prove: There are ___ connections among those parts. Fill in the blank. I'm sure you already know the answer. But the challenge is to prove it. This is representative of the types of proofs that we will write in Q.E.D. Hour.",
                    group2));
            group2.Items.Add(new SampleDataItem("Group-2-Item-2",
                    "Service Transaction",
                    "Prove correct use of a transaction",
                    "Assets/Transaction.gif",
                    "Item Description: Pellentesque porta, mauris quis interdum vehicula, urna sapien ultrices velit, nec venenatis dui odio in augue. Cras posuere, enim a cursus convallis, neque turpis malesuada erat, ut adipiscing neque tortor ac erat.",
                    "Below is the source code for a service. Prove that:\n\n" +
                    "The service has a transaction when it is called.\n" +
                    "The transaction is disposed.\n" +
                    "The code is thread safe.\n\n" +
                    "The proof of the first two points is pretty straight forward. Because the transaction is assigned prior to calling the service function, we know the service function has a transaction. And because the transaction is created in a using statement, we know that it will be disposed.\n\n" +
                    "The third point is harder to prove. If two threads call GetOrder on the same object, they will both try to assign a transaction to the same OrderServiceProvider. We have to prove that this won't happen. For that, we look at the Castle configuration:",
                    group2));
            this.AllGroups.Add(group2);
        }
    }
}
