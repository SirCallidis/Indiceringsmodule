﻿using Indiceringsmodule.Common;
using Indiceringsmodule.Common.EventModels;
using Indiceringsmodule.Presentation;
using Indiceringsmodule.WPFViews.UserControls;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;

namespace Indiceringsmodule
{
    public class ViewModelCoupler
    {
        private protected EventAggregator Ea;
        private readonly List<Subscription> Subscriptions = new List<Subscription>();

        public ViewModelCoupler(EventAggregator ea)
        {
            Ea = ea;
            Subscriptions.Add(ea.Subscribe<RequestViewForViewModelEventModel>(m => CreateView(m.Data)));
        }

        private void CreateView(object input)
        {
            var type = input.GetType();

            switch (type.Name)
            {
                case "PopUpOptionsViewModel":
                    var view = new PopUpOptions(Ea);
                    view.DataContext = input;
                    Ea.Publish(new ProvidingViewForViewModelEventModel() { Data = view });
                    break;
                default:
                    throw new ArgumentException($"Cannot find corresponding view. {type.Name}");
            }
        }
    }
}