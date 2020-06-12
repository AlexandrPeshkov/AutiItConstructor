using Domain.ActionTypes;
using Models;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;

namespace DesktopApp
{
    public class ScriptInfoItem : INotifyPropertyChanged
    {
        public ControlInfo ControlInfo { get; set; }

        public string ControlId
        {
            get
            {
                return ControlInfo.ControlId;
            }
            set
            {
                ControlInfo.ControlId = value;
                PropertyChanged(this, new PropertyChangedEventArgs(nameof(ControlId)));
            }
        }

        public ActionType ActionType { get; set; }

        public IEnumerable<ActionType> ActionTypes => Enum.GetValues(typeof(ActionType)).Cast<ActionType>();


        public ScriptInfoItem(ControlInfo controlInfo = null, ActionType actionType = ActionType.MoveMouse)
        {
            ControlInfo = controlInfo ?? new ControlInfo();
            ActionType = actionType;
        }

        public event PropertyChangedEventHandler PropertyChanged;
    }
}
