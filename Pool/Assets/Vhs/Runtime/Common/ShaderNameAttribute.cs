#if !VOL_FX

using System;

//  VhsFx © NullTale - https://twitter.com/NullTale/
namespace Vhs.Runtime.Common
{
    /// <summary>
    /// Used to get shader link for pass material
    /// </summary>
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = true)]
    public class ShaderNameAttribute : Attribute
    {
        public string _name;
            
        public ShaderNameAttribute(string name)
        {
            _name = name;
        }
    }
}

#endif