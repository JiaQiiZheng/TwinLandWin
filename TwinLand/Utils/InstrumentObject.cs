using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TwinLand.Utils
{
    public class InstrumentObject
    {
        public PlanarPointEmitter PlanarPointEmitter {  get; set; }

        public InstrumentObject(PlanarPointEmitter ppe)
        {
            this.PlanarPointEmitter = ppe;
        }
    }
}
