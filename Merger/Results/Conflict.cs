using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Merger
{
    public class Conflict
    {
        public string PropertyName { get; private set; }
        public string DestinationValue { get; private set; }
        public string SourceValue { get; private set; }

        public Conflict(string name, string sourceValue, string destinationValue)
        {
            PropertyName = name;
            SourceValue = sourceValue;
            DestinationValue = destinationValue;
        }
    }
}
