using Indiceringsmodule.Common.EventModels;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Dynamic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Documents;

namespace Indiceringsmodule.Common.DocumentObject
{
    //by now FactMember holds a lot of logic in stead of pure data.
    //May need to extract the logic bits into a new VM => FactMemberViewModel
    public partial class FactMember : Observable
    {
        #region Fields & Properties

        private protected int _ID;
        public int ID
        {
            get { return _ID; }
            set { SetProperty(ref _ID, value); }
        }

        private string _Remark;
        public string Remark
        {
            get { return _Remark; }
            set { SetProperty(ref _Remark, value); }
        }

        [JsonIgnore]
        private protected EventAggregator Ea;

        //represents the key by which the FactMember
        //can be looked up in the Fact's Dictionairy.
        [JsonIgnore]
        private Hyperlink _Link;
        [JsonIgnore]
        public Hyperlink Link
        {
            get { return _Link; }
            set { SetProperty(ref _Link, value); }
        }

        [JsonIgnore]
        private readonly List<Subscription> Subscriptions = new List<Subscription>();

        //Collection of Setnames and their array data which were loaded from
        //external file on program load (Appdata.xaml.cs)
        [JsonIgnore]
        private List<KeyValuePair<string, string[]>> _ExtraSets;
        [JsonIgnore]
        public List<KeyValuePair<string, string[]>> ExtraSets
        {
            get { return _ExtraSets; }
            set { SetProperty(ref _ExtraSets, value); }
        }
        [JsonIgnore]
        private List<string> _ExtraSetNames;
        [JsonIgnore]
        public List<string> ExtraSetNames
        {
            get { return _ExtraSetNames; }
            set { SetProperty(ref _ExtraSetNames, value); }
        }
        [JsonIgnore]
        private string _SelectedSetName;
        [JsonIgnore]
        public string SelectedSetName
        {
            get { return _SelectedSetName; }
            set
            {
                //here: if SelectedSetName != null, fire popup in UI informing user if they want to
                //confirm new selection. warning: new selection will wipe previous fields
                GetSetContent(value);
                ClearSetValues();
                SetProperty(ref _SelectedSetName, value);
            }
        }

        //ugly and cumbersome static list of properties. May need refactoring at some point
        //dynamic keyword, binding to Index of Observable Collection or ExpandoObject?
        [JsonIgnore] private string _ExtraPropVal0;
        [JsonIgnore] public string ExtraPropVal0
        {
            get { return _ExtraPropVal0; }
            set { SetProperty(ref _ExtraPropVal0, value); }
        }
        [JsonIgnore] private string _ExtraPropVal1;
        [JsonIgnore] public string ExtraPropVal1
        {
            get { return _ExtraPropVal1; }
            set { SetProperty(ref _ExtraPropVal1, value); }
        }
        [JsonIgnore] private string _ExtraPropVal2;
        [JsonIgnore] public string ExtraPropVal2
        {
            get { return _ExtraPropVal2; }
            set { SetProperty(ref _ExtraPropVal2, value); }
        }
        [JsonIgnore] private string _ExtraPropVal3;
        [JsonIgnore] public string ExtraPropVal3
        {
            get { return _ExtraPropVal3; }
            set { SetProperty(ref _ExtraPropVal3, value); }
        }
        [JsonIgnore] private string _ExtraPropVal4;
        [JsonIgnore] public string ExtraPropVal4
        {
            get { return _ExtraPropVal4; }
            set { SetProperty(ref _ExtraPropVal4, value); }
        }
        [JsonIgnore] private string _ExtraPropVal5;
        [JsonIgnore] public string ExtraPropVal5
        {
            get { return _ExtraPropVal5; }
            set { SetProperty(ref _ExtraPropVal5, value); }
        }
        [JsonIgnore] private string _ExtraPropVal6;
        [JsonIgnore] public string ExtraPropVal6
        {
            get { return _ExtraPropVal6; }
            set { SetProperty(ref _ExtraPropVal6, value); }
        }
        [JsonIgnore] private string _ExtraPropVal7;
        [JsonIgnore] public string ExtraPropVal7
        {
            get { return _ExtraPropVal7; }
            set { SetProperty(ref _ExtraPropVal7, value); }
        }
        [JsonIgnore] private string _ExtraPropVal8;
        [JsonIgnore] public string ExtraPropVal8
        {
            get { return _ExtraPropVal8; }
            set { SetProperty(ref _ExtraPropVal8, value); }
        }
        [JsonIgnore] private string _ExtraPropVal9;
        [JsonIgnore] public string ExtraPropVal9
        {
            get { return _ExtraPropVal9; }
            set { SetProperty(ref _ExtraPropVal9, value); }
        }
        [JsonIgnore] private string _ExtraPropVal10;
        [JsonIgnore] public string ExtraPropVal10
        {
            get { return _ExtraPropVal10; }
            set { SetProperty(ref _ExtraPropVal10, value); }
        }
        [JsonIgnore] private string _ExtraPropVal11;
        [JsonIgnore] public string ExtraPropVal11
        {
            get { return _ExtraPropVal11; }
            set { SetProperty(ref _ExtraPropVal11, value); }
        }
        [JsonIgnore] private string _ExtraPropVal12;
        [JsonIgnore] public string ExtraPropVal12
        {
            get { return _ExtraPropVal12; }
            set { SetProperty(ref _ExtraPropVal12, value); }
        }
        [JsonIgnore] private string _ExtraPropVal13;
        [JsonIgnore] public string ExtraPropVal13
        {
            get { return _ExtraPropVal13; }
            set { SetProperty(ref _ExtraPropVal13, value); }
        }
        [JsonIgnore] private string _ExtraPropVal14;
        [JsonIgnore] public string ExtraPropVal14
        {
            get { return _ExtraPropVal14; }
            set { SetProperty(ref _ExtraPropVal14, value); }
        }

        #endregion Fields & Properties

        #region Methods

        /// <summary>
        /// Wiring up the parts of the Class
        /// </summary>
        protected void WireUpFactMember()
        {
            Subscriptions.Add(Ea.Subscribe<SendingExtraSetsEventModel>(m => ProcessExtraSets(m.Data)));
        }

        /// <summary>
        /// poplulates fields with the incoming data from the loaded sets
        /// </summary>
        /// <param name="sets">information loaded from an external file</param>
        private void ProcessExtraSets(List<KeyValuePair<string, string[]>> sets)
        {
            ExtraSets = sets;
            ExtraSetNames = GetExtraSetNames();
        }

        /// <summary>
        /// Forms a list of the names of the set, to which the View's Dropbox can bind.
        /// </summary>
        /// <returns></returns>
        private List<string> GetExtraSetNames()
        {
            var list = new List<string>();
            var keys = ExtraSets.Select(x => x.Key);
            foreach (var key in keys)
            {
                list.Add(key);
            }
            return list;
        }

        /// <summary>
        /// Gets the collection of fields belonging to incoming set's name parameter,
        /// and sends this to the view through an Event.
        /// </summary>
        /// <param name="value"></param>
        private void GetSetContent(string value)
        {
            var setContent = ExtraSets.Where(x => x.Key == value)
                                      .Select(x => x.Value)
                                      .First();
            Ea.Publish(new DisplaySetContentEventModel() { Data = setContent });
        }

        /// <summary>
        /// returns a list of the members of the set based on its name parameter
        /// </summary>
        /// <param name="value"></param>
        /// <returns>the name of the set to be retrieved</returns>
        public string[] GetListOfSetContent(string value)
        {
            var setContent = ExtraSets.Where(x => x.Key == value)
                                      .Select(x => x.Value)
                                      .First();
            return setContent;
        }

        /// <summary>
        /// clears each of the Extra Property values.
        /// </summary>
        private void ClearSetValues()
        {
            ExtraPropVal0 = null;
            ExtraPropVal1 = null;
            ExtraPropVal2 = null;
            ExtraPropVal3 = null;
            ExtraPropVal4 = null;
            ExtraPropVal5 = null;
            ExtraPropVal6 = null;
            ExtraPropVal7 = null;
            ExtraPropVal8 = null;
            ExtraPropVal9 = null;
            ExtraPropVal10 = null;
            ExtraPropVal11 = null;
            ExtraPropVal12 = null;
            ExtraPropVal13 = null;
            ExtraPropVal14 = null;
        }


        public void Dispose()
        {
            foreach (var sub in Subscriptions)
            {
                sub?.Dispose();
            }
        }

        #endregion Methods
    }
}
