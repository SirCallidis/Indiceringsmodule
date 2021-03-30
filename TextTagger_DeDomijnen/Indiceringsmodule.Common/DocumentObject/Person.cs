using Indiceringsmodule.Common.EventModels;
using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Documents;

namespace Indiceringsmodule.Common.DocumentObject
{
    public class Person : FactMember
    {
        #region Fields & Properties

        private string _FirstName;
        public string FirstName
        {
            get { return _FirstName; }
            set { SetProperty(ref _FirstName, value); }
        }

        private string _LastName;
        public string LastName
        {
            get { return _LastName; }
            set { SetProperty(ref _LastName, value); }
        }

        private string _Particle;
        public string Particle
        {
            get { return _Particle; }
            set { SetProperty(ref _Particle, value); }
        }

        private string _AdditionalNames;
        public string AdditionalNames
        {
            get { return _AdditionalNames; }
            set { SetProperty(ref _AdditionalNames, value); }
        }

        private string _Role;
        public string Role
        {
            get { return _Role; }
            set { SetProperty(ref _Role, value); }
        }

        private string _DesignationOrTitle;
        public string DesignationOrTitle
        {
            get { return _DesignationOrTitle; }
            set { SetProperty(ref _DesignationOrTitle, value); }
        }

        #endregion Fields & Properties

        #region Default Constructor

        public Person(int id, Hyperlink link, EventAggregator ea)
        {
            ID = id;
            Link = link;
            Ea = ea;
            WireUpFactMember();
            Ea.Publish(new RequestExtraSetsEventModel() { });
        }

        #endregion Default Constructor

        #region Methods

        /// <summary>
        /// returns the concatonated full name of the person
        /// </summary>
        /// <returns></returns>
        public string FullName()
        {
            if (AdditionalNames == null && Particle == null)
            {
                return $"{FirstName} {LastName}";
            }
            if (AdditionalNames == null && Particle != null)
            {
                return $"{FirstName} {Particle} {LastName}";
            }
            if (AdditionalNames != null && Particle == null)
            {
                return $"{FirstName} {AdditionalNames} {LastName}";
            }
            else
            {
                return FirstName + AdditionalNames + Particle + LastName;
            }  
        }

        #endregion Methods
    }
}
