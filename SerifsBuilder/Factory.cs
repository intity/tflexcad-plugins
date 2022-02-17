using System;

namespace TFlex
{
    public class Factory : PluginFactory
    {
        public override Plugin CreateInstance()
        {
            return new PluginInstance(this);
        }

        public override Guid ID => new Guid("{1298E5D3-4E36-4F01-B70F-15C3AA5BDD7B}");
        public override string Name => "SerfsBuilder";
    }
}